using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.Blobs.Validation;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Medias;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Http;
using Volo.Abp.Imaging;
using Volo.Abp.Json;
using Volo.Abp.Uow;
using Blob = IczpNet.Chat.Blobs.Blob;
using Image = ImageMagick.MagickImage;

namespace IczpNet.Chat.Controllers;

[Authorize]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/attachments")]
public class AttachmentController(
    ISessionUnitManager sessionUnitManager,
    IChatObjectManager chatObjectManager,
    IAttachmentUploadValidatorManager validatorManager,
    IMediaResolver mediaResolver,
    IOptions<MessageOptions> messageOptions,
    IOptions<AttachmentUploadOptions> uploadOptions,
    IJsonSerializer jsonSerializer) : ChatController
{
    protected MessageOptions MessageSetting { get; } = messageOptions.Value;
    protected AttachmentUploadOptions UploadSetting { get; } = uploadOptions.Value;

    protected virtual async Task<SessionUnitCacheItem> GetAndCheckSessionUnitAsync(Guid sessionUnitId)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Authentication required.");
        }

        var unit = await sessionUnitManager.GetCacheAsync(sessionUnitId);
        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (!ownerIds.Contains(unit.OwnerId))
        {
            throw new AbpAuthorizationException("SessionUnit does not belong to current user.");
        }

        return unit;
    }

    protected virtual DateTime GetTempExpireTime() => Clock.Now.AddHours(UploadSetting.TempBlobExpireHours);

    protected virtual async Task<Blob> SaveTempBlobAsync(
        Guid blobId,
        byte[] bytes,
        string fileName,
        string contentType,
        string suffix,
        SessionUnitCacheItem unit,
        AttachmentMediaType mediaType,
        BlobRoles role,
        Guid? parentBlobId = null)
    {
        var directoryName = await BlobResolver.GetDirectoryNameAsync(new BlobContext
        {
            BlobId = blobId,
            Container = ChatFilesContainer,
            SessionId = unit.SessionId!.Value,
            SessionUnitId = unit.Id,
            ChatObjectId = unit.OwnerId,
            MediaType = mediaType,
            BlobRole = role,
            FileSize = bytes.Length,
            MimeType = contentType,
            FileName = fileName,
        });

        var storedFileName = await GenerateFileNameAsync(suffix);
        var blobName = Path.Combine(directoryName, storedFileName).Replace(@"\", "/");

        return await BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
        {
            IsPublic = false,
            FileSize = bytes.Length,
            MimeType = contentType,
            FileName = Path.GetFileName(fileName),
            Name = blobName,
            Suffix = suffix,
            Bytes = bytes,
            UploaderChatObjectId = unit.OwnerId,
            UploadSessionUnitId = unit.Id,
            UploadSessionId = unit.SessionId,
            ParentBlobId = parentBlobId,
            Role = role,
            Status = BlobStatus.Temp,
            ExpireTime = GetTempExpireTime(),
            RefCount = 0,
        });
    }

    [HttpPost]
    [Route("upload-image/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<UploadAttachmentOutput> UploadImageAsync(
        Guid sessionUnitId,
        IFormFile file,
        IFormFile thumbnailFile = null,
        [FromForm] bool isOriginal = false)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var unit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        var suffix = GetExtension(file.ContentType, file.FileName);

        await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = unit,
            MediaType = AttachmentMediaType.Image,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Suffix = suffix,
            FileSize = file.Length,
            IsOriginal = isOriginal,
            OpenReadStreamAsync = _ => Task.FromResult(file.OpenReadStream()),
        });

        var sourceBytes = await file.GetAllBytesAsync();
        using var sourceImage = new Image(sourceBytes);
        var (actualWidth, actualHeight) = GetImageActualDimensions(sourceImage);

        var primaryBlobId = GuidGenerator.Create();
        Guid? originalBlobId = null;
        string originalUrl = null;

        byte[] primaryBytes;
        int primaryWidth = actualWidth;
        int primaryHeight = actualHeight;

        var bigImageSize = MessageSetting.ImageSetting.BigSize;
        if (isOriginal)
        {
            // 原图模式下同时保留 Original（原图）和 Primary（高清压缩图）
            originalBlobId = GuidGenerator.Create();
            var originalBlob = await SaveTempBlobAsync(
                originalBlobId.Value,
                sourceBytes,
                file.FileName,
                file.ContentType,
                suffix,
                unit,
                AttachmentMediaType.Image,
                BlobRoles.Original,
                parentBlobId: primaryBlobId);
            originalUrl = await GetFileUrlAsync(originalBlob.Id, originalBlob.MimeType);

            var resized = await ImageResizer.ResizeAsync(sourceBytes, new ImageResizeArgs
            {
                Width = (uint)bigImageSize,
                Height = (uint)bigImageSize,
                Mode = ImageResizeMode.Max,
            });
            primaryBytes = resized.State == ImageProcessState.Done && resized.Result?.Length > 0
                ? resized.Result
                : sourceBytes;
        }
        else
        {
            var resized = await ImageResizer.ResizeAsync(sourceBytes, new ImageResizeArgs
            {
                Width = (uint)bigImageSize,
                Height = (uint)bigImageSize,
                Mode = ImageResizeMode.Max,
            });
            if (resized.State == ImageProcessState.Unsupported)
            {
                throw new UserFriendlyException("Unsupported image format.");
            }
            primaryBytes = resized.Result?.Length > 0 ? resized.Result : sourceBytes;
        }

        if (primaryBytes != sourceBytes)
        {
            using var primaryImg = new Image(primaryBytes);
            (primaryWidth, primaryHeight) = GetImageActualDimensions(primaryImg);
        }

        var primaryBlob = await SaveTempBlobAsync(
            primaryBlobId,
            primaryBytes,
            file.FileName,
            file.ContentType,
            suffix,
            unit,
            AttachmentMediaType.Image,
            BlobRoles.Primary);

        // 缩略图：优先采用前端上传的 thumbnailFile，否则服务端兜底生成
        byte[] thumbBytes;
        string thumbMime = file.ContentType;
        string thumbSuffix = suffix;
        if (thumbnailFile != null && thumbnailFile.Length > 0)
        {
            thumbSuffix = GetExtension(thumbnailFile.ContentType, thumbnailFile.FileName);
            await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
            {
                SessionUnit = unit,
                MediaType = AttachmentMediaType.Snapshot,
                FileName = thumbnailFile.FileName,
                ContentType = thumbnailFile.ContentType,
                Suffix = thumbSuffix,
                FileSize = thumbnailFile.Length,
                OpenReadStreamAsync = _ => Task.FromResult(thumbnailFile.OpenReadStream()),
            });
            thumbBytes = await thumbnailFile.GetAllBytesAsync();
            thumbMime = thumbnailFile.ContentType;
        }
        else
        {
            var thumbSize = MessageSetting.ImageSetting.ThumbnailSize;
            var resizedThumb = await ImageResizer.ResizeAsync(primaryBytes, new ImageResizeArgs
            {
                Width = (uint)thumbSize,
                Height = (uint)thumbSize,
                Mode = ImageResizeMode.Max,
            });
            thumbBytes = resizedThumb.State == ImageProcessState.Done && resizedThumb.Result?.Length > 0
                ? resizedThumb.Result
                : primaryBytes;
        }

        var thumbBlobId = GuidGenerator.Create();
        var thumbBlob = await SaveTempBlobAsync(
            thumbBlobId,
            thumbBytes,
            $"thumb_{file.FileName}",
            thumbMime,
            thumbSuffix,
            unit,
            AttachmentMediaType.Image,
            BlobRoles.Thumbnail,
            parentBlobId: primaryBlob.Id);

        var profile = GetImageProfileJson(sourceImage);
        return new UploadAttachmentOutput
        {
            BlobId = primaryBlob.Id,
            MediaType = AttachmentMediaType.Image,
            Url = await GetFileUrlAsync(primaryBlob.Id, primaryBlob.MimeType),
            ThumbnailBlobId = thumbBlob.Id,
            ThumbnailUrl = await GetFileUrlAsync(thumbBlob.Id, thumbBlob.MimeType),
            OriginalBlobId = originalBlobId,
            OriginalUrl = originalUrl,
            FileName = primaryBlob.FileName,
            ContentType = primaryBlob.MimeType,
            Suffix = primaryBlob.Suffix,
            Size = isOriginal ? sourceBytes.Length : primaryBytes.Length,
            Width = primaryWidth,
            Height = primaryHeight,
            Profile = profile,
            ExpireTime = primaryBlob.ExpireTime,
            ReuseToken = IssueReuseToken(primaryBlob.Id, AttachmentMediaType.Image, primaryWidth, primaryHeight, null, profile, primaryBlob.ExpireTime),
        };
    }

    [HttpPost]
    [Route("upload-video/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<UploadAttachmentOutput> UploadVideoAsync(
        Guid sessionUnitId,
        IFormFile file,
        IFormFile snapshotFile = null,
        IFormFile snapshotThumbnailFile = null,
        [FromForm] int? width = null,
        [FromForm] int? height = null,
        [FromForm] int? durationMs = null,
        [FromForm] bool isOriginal = false)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var unit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        var suffix = GetExtension(file.ContentType, file.FileName);

        await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = unit,
            MediaType = AttachmentMediaType.Video,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Suffix = suffix,
            FileSize = file.Length,
            Width = width,
            Height = height,
            DurationMs = durationMs,
            IsOriginal = isOriginal,
            HasSnapshot = snapshotFile != null && snapshotFile.Length > 0,
            OpenReadStreamAsync = _ => Task.FromResult(file.OpenReadStream()),
        });

        var videoBytes = await file.GetAllBytesAsync();
        var videoBlobId = GuidGenerator.Create();
        var videoBlob = await SaveTempBlobAsync(
            videoBlobId,
            videoBytes,
            file.FileName,
            file.ContentType,
            suffix,
            unit,
            AttachmentMediaType.Video,
            isOriginal ? BlobRoles.Original : BlobRoles.Primary);

        Guid? snapshotBlobId = null;
        string snapshotUrl = null;
        Guid? snapshotThumbBlobId = null;
        string snapshotThumbUrl = null;
        Guid? gifBlobId = null;
        string gifUrl = null;
        int? resolvedWidth = width;
        int? resolvedHeight = height;
        int? resolvedDurationMs = durationMs;

        if (snapshotFile != null && snapshotFile.Length > 0)
        {
            var snapSuffix = GetExtension(snapshotFile.ContentType, snapshotFile.FileName);
            await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
            {
                SessionUnit = unit,
                MediaType = AttachmentMediaType.Snapshot,
                FileName = snapshotFile.FileName,
                ContentType = snapshotFile.ContentType,
                Suffix = snapSuffix,
                FileSize = snapshotFile.Length,
                OpenReadStreamAsync = _ => Task.FromResult(snapshotFile.OpenReadStream()),
            });

            var snapBytes = await snapshotFile.GetAllBytesAsync();
            using (var snapImg = new Image(snapBytes))
            {
                var (snapW, snapH) = GetImageActualDimensions(snapImg);
                if (!resolvedWidth.HasValue || !resolvedHeight.HasValue)
                {
                    resolvedWidth = snapW;
                    resolvedHeight = snapH;
                }
                else if (snapW > 0 && snapH > 0 && resolvedWidth.Value > 0 && resolvedHeight.Value > 0)
                {
                    var isSnapLandscape = snapW > snapH;
                    var isResolvedLandscape = resolvedWidth.Value > resolvedHeight.Value;
                    if (isSnapLandscape != isResolvedLandscape)
                    {
                        var temp = resolvedWidth.Value;
                        resolvedWidth = resolvedHeight.Value;
                        resolvedHeight = temp;
                    }
                }
            }

            var snapBlob = await SaveTempBlobAsync(
                GuidGenerator.Create(),
                snapBytes,
                snapshotFile.FileName,
                snapshotFile.ContentType,
                snapSuffix,
                unit,
                AttachmentMediaType.Snapshot,
                BlobRoles.Snapshot,
                parentBlobId: videoBlob.Id);
            snapshotBlobId = snapBlob.Id;
            snapshotUrl = await GetFileUrlAsync(snapBlob.Id, snapBlob.MimeType);

            byte[] snapThumbBytes;
            string snapThumbMime = snapshotFile.ContentType;
            string snapThumbSuffix = snapSuffix;
            if (snapshotThumbnailFile != null && snapshotThumbnailFile.Length > 0)
            {
                snapThumbSuffix = GetExtension(snapshotThumbnailFile.ContentType, snapshotThumbnailFile.FileName);
                await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
                {
                    SessionUnit = unit,
                    MediaType = AttachmentMediaType.Snapshot,
                    FileName = snapshotThumbnailFile.FileName,
                    ContentType = snapshotThumbnailFile.ContentType,
                    Suffix = snapThumbSuffix,
                    FileSize = snapshotThumbnailFile.Length,
                    OpenReadStreamAsync = _ => Task.FromResult(snapshotThumbnailFile.OpenReadStream()),
                });
                snapThumbBytes = await snapshotThumbnailFile.GetAllBytesAsync();
                snapThumbMime = snapshotThumbnailFile.ContentType;
            }
            else
            {
                var thumbSize = MessageSetting.VideoSetting.SnapshotThumbnailSize;
                if (resolvedWidth.HasValue && resolvedHeight.HasValue &&
                    resolvedWidth.Value <= thumbSize && resolvedHeight.Value <= thumbSize)
                {
                    snapThumbBytes = snapBytes;
                }
                else
                {
                    var resized = await ImageResizer.ResizeAsync(snapBytes, new ImageResizeArgs
                    {
                        Width = (uint)thumbSize,
                        Height = (uint)thumbSize,
                        Mode = ImageResizeMode.Max,
                    });
                    snapThumbBytes = resized.State == ImageProcessState.Done && resized.Result?.Length > 0
                        ? resized.Result
                        : snapBytes;
                }
            }

            var snapThumbBlob = await SaveTempBlobAsync(
                GuidGenerator.Create(),
                snapThumbBytes,
                $"thumb_{snapshotFile.FileName}",
                snapThumbMime,
                snapThumbSuffix,
                unit,
                AttachmentMediaType.Snapshot,
                BlobRoles.SnapshotThumbnail,
                parentBlobId: videoBlob.Id);
            snapshotThumbBlobId = snapThumbBlob.Id;
            snapshotThumbUrl = await GetFileUrlAsync(snapThumbBlob.Id, snapThumbBlob.MimeType);
        }
        else if (MessageSetting.VideoSetting.IsGenerateSnapshot)
        {
            var fallback = await GenerateVideoSnapshotFallbackAsync(
                videoBytes,
                file.FileName,
                unit,
                videoBlob.Id,
                resolvedWidth,
                resolvedHeight,
                resolvedDurationMs);
            snapshotBlobId = fallback.SnapshotBlobId;
            snapshotUrl = fallback.SnapshotUrl;
            snapshotThumbBlobId = fallback.SnapshotThumbnailBlobId;
            snapshotThumbUrl = fallback.SnapshotThumbnailUrl;
            gifBlobId = fallback.GifBlobId;
            gifUrl = fallback.GifUrl;
            resolvedWidth = fallback.Width;
            resolvedHeight = fallback.Height;
            resolvedDurationMs = fallback.DurationMs;
        }

        return new UploadAttachmentOutput
        {
            BlobId = videoBlob.Id,
            MediaType = AttachmentMediaType.Video,
            Url = await GetFileUrlAsync(videoBlob.Id, videoBlob.MimeType),
            SnapshotBlobId = snapshotBlobId,
            SnapshotUrl = snapshotUrl,
            SnapshotThumbnailBlobId = snapshotThumbBlobId,
            SnapshotThumbnailUrl = snapshotThumbUrl,
            GifBlobId = gifBlobId,
            GifUrl = gifUrl,
            FileName = videoBlob.FileName,
            ContentType = videoBlob.MimeType,
            Suffix = videoBlob.Suffix,
            Size = videoBlob.FileSize,
            Width = resolvedWidth,
            Height = resolvedHeight,
            Duration = resolvedDurationMs,
            ExpireTime = videoBlob.ExpireTime,
            ReuseToken = IssueReuseToken(videoBlob.Id, AttachmentMediaType.Video, resolvedWidth, resolvedHeight, resolvedDurationMs, null, videoBlob.ExpireTime),
        };
    }

    protected sealed record VideoSnapshotFallbackResult(
        Guid? SnapshotBlobId,
        string SnapshotUrl,
        Guid? SnapshotThumbnailBlobId,
        string SnapshotThumbnailUrl,
        Guid? GifBlobId,
        string GifUrl,
        int? Width,
        int? Height,
        int? DurationMs);

    /// <summary>
    /// 客户端未携带封面时，由服务端兜底抽帧生成封面与封面缩略图
    /// </summary>
    protected virtual async Task<VideoSnapshotFallbackResult> GenerateVideoSnapshotFallbackAsync(
        byte[] videoBytes,
        string fileName,
        SessionUnitCacheItem unit,
        Guid videoBlobId,
        int? currentWidth,
        int? currentHeight,
        int? currentDurationMs)
    {
        Guid? snapshotBlobId = null;
        string snapshotUrl = null;
        Guid? snapshotThumbBlobId = null;
        string snapshotThumbUrl = null;
        Guid? gifBlobId = null;
        string gifUrl = null;
        int? resolvedWidth = currentWidth;
        int? resolvedHeight = currentHeight;
        int? resolvedDurationMs = currentDurationMs;

        var mediaInfo = await mediaResolver.GetVideoInfoAsync(videoBytes, fileName);
        if (mediaInfo == null)
        {
            return new VideoSnapshotFallbackResult(
                snapshotBlobId,
                snapshotUrl,
                snapshotThumbBlobId,
                snapshotThumbUrl,
                gifBlobId,
                gifUrl,
                resolvedWidth,
                resolvedHeight,
                resolvedDurationMs);
        }

        try
        {
            if (!resolvedWidth.HasValue && mediaInfo.Width.GetValueOrDefault() > 0)
            {
                resolvedWidth = mediaInfo.Width.Value;
            }
            if (!resolvedHeight.HasValue && mediaInfo.Height.GetValueOrDefault() > 0)
            {
                resolvedHeight = mediaInfo.Height.Value;
            }
            if (!resolvedDurationMs.HasValue && mediaInfo.Duration.HasValue)
            {
                resolvedDurationMs = (int)mediaInfo.Duration.Value.TotalMilliseconds;
            }

            if (!string.IsNullOrWhiteSpace(mediaInfo.ImageSnapshotPath) && System.IO.File.Exists(mediaInfo.ImageSnapshotPath))
            {
                var snapBytes = await System.IO.File.ReadAllBytesAsync(mediaInfo.ImageSnapshotPath);
                var snapSuffix = Path.GetExtension(mediaInfo.ImageSnapshotPath);
                var snapMime = MimeTypes.GetByExtension(snapSuffix);

                using (var snapImg = new Image(snapBytes))
                {
                    var (w, h) = GetImageActualDimensions(snapImg);
                    if (w > 0 && h > 0)
                    {
                        if (!resolvedWidth.HasValue || !resolvedHeight.HasValue)
                        {
                            resolvedWidth = w;
                            resolvedHeight = h;
                        }
                        else
                        {
                            var isSnapLandscape = w > h;
                            var isResolvedLandscape = resolvedWidth.Value > resolvedHeight.Value;
                            if (isSnapLandscape != isResolvedLandscape)
                            {
                                var temp = resolvedWidth.Value;
                                resolvedWidth = resolvedHeight.Value;
                                resolvedHeight = temp;
                            }
                        }
                    }
                }

                var snapBlob = await SaveTempBlobAsync(
                    GuidGenerator.Create(),
                    snapBytes,
                    $"{Path.GetFileNameWithoutExtension(fileName)}.snapshot{snapSuffix}",
                    snapMime,
                    snapSuffix,
                    unit,
                    AttachmentMediaType.Snapshot,
                    BlobRoles.Snapshot,
                    parentBlobId: videoBlobId);
                snapshotBlobId = snapBlob.Id;
                snapshotUrl = await GetFileUrlAsync(snapBlob.Id, snapBlob.MimeType);

                var thumbSize = MessageSetting.VideoSetting.SnapshotThumbnailSize;
                var resized = await ImageResizer.ResizeAsync(snapBytes, new ImageResizeArgs
                {
                    Width = (uint)thumbSize,
                    Height = (uint)thumbSize,
                    Mode = ImageResizeMode.Max,
                });
                var snapThumbBytes = resized.State == ImageProcessState.Done && resized.Result?.Length > 0
                    ? resized.Result
                    : snapBytes;

                var snapThumbBlob = await SaveTempBlobAsync(
                    GuidGenerator.Create(),
                    snapThumbBytes,
                    $"thumb_{Path.GetFileNameWithoutExtension(fileName)}{snapSuffix}",
                    snapMime,
                    snapSuffix,
                    unit,
                    AttachmentMediaType.Snapshot,
                    BlobRoles.SnapshotThumbnail,
                    parentBlobId: videoBlobId);
                snapshotThumbBlobId = snapThumbBlob.Id;
                snapshotThumbUrl = await GetFileUrlAsync(snapThumbBlob.Id, snapThumbBlob.MimeType);
            }

            if (!string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath) && System.IO.File.Exists(mediaInfo.GifSnapshotPath))
            {
                var gifBytes = await System.IO.File.ReadAllBytesAsync(mediaInfo.GifSnapshotPath);
                var gifBlob = await SaveTempBlobAsync(
                    GuidGenerator.Create(),
                    gifBytes,
                    $"{Path.GetFileNameWithoutExtension(fileName)}.gif",
                    "image/gif",
                    ".gif",
                    unit,
                    AttachmentMediaType.Video,
                    BlobRoles.Gif,
                    parentBlobId: videoBlobId);
                gifBlobId = gifBlob.Id;
                gifUrl = await GetFileUrlAsync(gifBlob.Id, gifBlob.MimeType);
            }
        }
        finally
        {
            DeleteGeneratedTempFile(mediaInfo.ImageSnapshotPath);
            DeleteGeneratedTempFile(mediaInfo.GifSnapshotPath);
            DeleteGeneratedTempFile(mediaInfo.TempVideoPath);
        }

        return new VideoSnapshotFallbackResult(
            snapshotBlobId,
            snapshotUrl,
            snapshotThumbBlobId,
            snapshotThumbUrl,
            gifBlobId,
            gifUrl,
            resolvedWidth,
            resolvedHeight,
            resolvedDurationMs);
    }

    [HttpPost]
    [Route("upload-sound/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<UploadAttachmentOutput> UploadSoundAsync(
        Guid sessionUnitId,
        IFormFile file,
        [FromForm] int? durationMs = null)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var unit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        var suffix = GetExtension(file.ContentType, file.FileName);

        await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = unit,
            MediaType = AttachmentMediaType.Sound,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Suffix = suffix,
            FileSize = file.Length,
            DurationMs = durationMs,
            OpenReadStreamAsync = _ => Task.FromResult(file.OpenReadStream()),
        });

        var bytes = await file.GetAllBytesAsync();
        var resolvedDurationMs = durationMs;
        if (!resolvedDurationMs.HasValue)
        {
            var audioInfo = await mediaResolver.GetAudioInfoAsync(bytes, file.FileName);
            if (audioInfo?.Duration != null)
            {
                resolvedDurationMs = (int)audioInfo.Duration.Value.TotalMilliseconds;
            }
        }

        var soundBlob = await SaveTempBlobAsync(
            GuidGenerator.Create(),
            bytes,
            file.FileName,
            file.ContentType,
            suffix,
            unit,
            AttachmentMediaType.Sound,
            BlobRoles.Primary);

        return new UploadAttachmentOutput
        {
            BlobId = soundBlob.Id,
            MediaType = AttachmentMediaType.Sound,
            Url = await GetFileUrlAsync(soundBlob.Id, soundBlob.MimeType),
            FileName = soundBlob.FileName,
            ContentType = soundBlob.MimeType,
            Suffix = soundBlob.Suffix,
            Size = soundBlob.FileSize,
            Duration = resolvedDurationMs,
            ExpireTime = soundBlob.ExpireTime,
            ReuseToken = IssueReuseToken(soundBlob.Id, AttachmentMediaType.Sound, null, null, resolvedDurationMs, null, soundBlob.ExpireTime),
        };
    }

    [HttpPost]
    [Route("upload-file/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<UploadAttachmentOutput> UploadFileAsync(Guid sessionUnitId, IFormFile file)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var unit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        var suffix = GetExtension(file.ContentType, file.FileName);

        await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = unit,
            MediaType = AttachmentMediaType.File,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Suffix = suffix,
            FileSize = file.Length,
            OpenReadStreamAsync = _ => Task.FromResult(file.OpenReadStream()),
        });

        var bytes = await file.GetAllBytesAsync();
        var fileBlob = await SaveTempBlobAsync(
            GuidGenerator.Create(),
            bytes,
            file.FileName,
            file.ContentType,
            suffix,
            unit,
            AttachmentMediaType.File,
            BlobRoles.Primary);

        return new UploadAttachmentOutput
        {
            BlobId = fileBlob.Id,
            MediaType = AttachmentMediaType.File,
            Url = await GetFileUrlAsync(fileBlob.Id, fileBlob.MimeType),
            FileName = fileBlob.FileName,
            ContentType = fileBlob.MimeType,
            Suffix = fileBlob.Suffix,
            Size = fileBlob.FileSize,
            ExpireTime = fileBlob.ExpireTime,
            ReuseToken = IssueReuseToken(fileBlob.Id, AttachmentMediaType.File, null, null, null, null, fileBlob.ExpireTime),
        };
    }

    /// <summary>
    /// 同账号多身份复用已上传附件（兑换当前会话身份独立的 Temp Blob，复用底层物理存储对象）
    /// </summary>
    [HttpPost]
    [Route("reuse/{sessionUnitId}")]
    [UnitOfWork]
    public async Task<UploadAttachmentOutput> ReuseAsync(
        Guid sessionUnitId,
        [FromBody] ReuseAttachmentInput input)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Authentication required.");
        }

        if (input == null || string.IsNullOrWhiteSpace(input.ReuseToken))
        {
            throw new AbpAuthorizationException("Attachment reuse token required.");
        }

        var unit = await GetAndCheckSessionUnitAsync(sessionUnitId);
        if (!unit.IsEnabled || !unit.IsInputEnabled || (unit.MuteExpireTime.HasValue && unit.MuteExpireTime.Value > Clock.Now))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Target session unit does not allow sending attachments or is muted.");
        }

        var tokenService = LazyServiceProvider.LazyGetRequiredService<IAttachmentReuseTokenService>();
        if (!tokenService.TryValidateToken(input.ReuseToken, CurrentUser.Id.Value, out var payload))
        {
            throw new AbpAuthorizationException("Invalid or expired attachment reuse token.");
        }

        var blobRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<Blob, Guid>>();
        var reuseRecordRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<BlobReuseRecord, Guid>>();
        var sourcePrimary = await blobRepository.FindAsync(payload.PrimaryBlobId);
        if (sourcePrimary == null || sourcePrimary.Status == BlobStatus.Expired)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
        }

        if (sourcePrimary.Status == BlobStatus.Temp &&
            sourcePrimary.ExpireTime.HasValue &&
            sourcePrimary.ExpireTime.Value < Clock.Now)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
        }

        var effectiveFileName = string.IsNullOrWhiteSpace(input.FileName)
            ? sourcePrimary.FileName
            : Path.GetFileName(input.FileName.Trim());
        var fileNameDigest = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(effectiveFileName ?? string.Empty)))[..16];

        var normalizedClientUploadId = string.IsNullOrWhiteSpace(input.ClientUploadId)
            ? null
            : input.ClientUploadId.Trim();
        var clientUploadKey = normalizedClientUploadId == null
            ? null
            : $"attach-reuse:{CurrentUser.Id.Value:N}:{sessionUnitId:N}:{payload.PrimaryBlobId:N}:{normalizedClientUploadId}:{fileNameDigest}";
        var reuseCache = clientUploadKey != null
            ? LazyServiceProvider.LazyGetRequiredService<IDistributedCache<UploadAttachmentOutput, string>>()
            : null;
        if (reuseCache != null && clientUploadKey != null)
        {
            var cachedOutput = await reuseCache.GetAsync(clientUploadKey);
            if (cachedOutput != null)
            {
                var cachedBlob = await blobRepository.FindAsync(cachedOutput.BlobId);
                if (cachedBlob != null &&
                    cachedBlob.Status == BlobStatus.Temp &&
                    (!cachedBlob.ExpireTime.HasValue || cachedBlob.ExpireTime.Value >= Clock.Now))
                {
                    return cachedOutput;
                }
            }
        }

        if (normalizedClientUploadId != null)
        {
            var existingRecord = await reuseRecordRepository.FindAsync(x =>
                x.UserId == CurrentUser.Id.Value &&
                x.SessionUnitId == sessionUnitId &&
                x.SourcePrimaryBlobId == payload.PrimaryBlobId &&
                x.ClientUploadId == normalizedClientUploadId);
            if (existingRecord != null)
            {
                if (!string.Equals(existingRecord.FileNameDigest, fileNameDigest, StringComparison.Ordinal))
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                        .WithData("Reason", "ClientUploadId reused with different attachment parameters.");
                }

                var persistedBlob = await blobRepository.FindAsync(existingRecord.PrimaryBlobId);
                if (persistedBlob != null &&
                    persistedBlob.Status == BlobStatus.Temp &&
                    (!persistedBlob.ExpireTime.HasValue || persistedBlob.ExpireTime.Value >= Clock.Now))
                {
                    return jsonSerializer.Deserialize<UploadAttachmentOutput>(existingRecord.ResultJson);
                }

                // Expired stale idempotency record: remove so a fresh upload/reuse for this ClientUploadId can proceed.
                await reuseRecordRepository.DeleteAsync(existingRecord, autoSave: true);
            }
        }

        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (sourcePrimary.UploaderChatObjectId.HasValue && !ownerIds.Contains(sourcePrimary.UploaderChatObjectId.Value))
        {
            throw new AbpAuthorizationException("Source attachment does not belong to current account.");
        }

        var childBlobs = (await blobRepository.GetQueryableAsync())
            .Where(x => x.ParentBlobId == sourcePrimary.Id && x.Status != BlobStatus.Expired)
            .ToList();

        var effectiveSuffix = !string.IsNullOrWhiteSpace(sourcePrimary.Suffix)
            ? sourcePrimary.Suffix
            : GetExtension(sourcePrimary.MimeType, effectiveFileName);

        await validatorManager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = unit,
            MediaType = payload.MediaType,
            FileName = effectiveFileName,
            ContentType = sourcePrimary.MimeType,
            Suffix = effectiveSuffix,
            FileSize = sourcePrimary.FileSize,
            Width = payload.Width,
            Height = payload.Height,
            DurationMs = payload.Duration,
            HasSnapshot = childBlobs.Any(x => x.Role == BlobRoles.Snapshot),
            OpenReadStreamAsync = async _ => new MemoryStream(await BlobManager.GetBytesAsync(sourcePrimary.Id)),
        });

        var newPrimaryId = GuidGenerator.Create();
        var newPrimaryBlob = await CloneTempBlobAsync(
            sourcePrimary,
            newPrimaryId,
            unit,
            payload.MediaType,
            BlobRoles.Primary,
            parentBlobId: null,
            overrideFileName: effectiveFileName);

        Guid? thumbId = null;
        string thumbUrl = null;
        Guid? originalId = null;
        string originalUrl = null;
        Guid? snapshotId = null;
        string snapshotUrl = null;
        Guid? snapshotThumbId = null;
        string snapshotThumbUrl = null;
        Guid? gifId = null;
        string gifUrl = null;

        foreach (var child in childBlobs)
        {
            var childMediaType = child.Role is BlobRoles.Snapshot or BlobRoles.SnapshotThumbnail
                ? AttachmentMediaType.Snapshot
                : payload.MediaType;
            var clonedChild = await CloneTempBlobAsync(
                child,
                GuidGenerator.Create(),
                unit,
                childMediaType,
                child.Role,
                parentBlobId: newPrimaryBlob.Id);
            var childUrl = await GetFileUrlAsync(clonedChild.Id, clonedChild.MimeType);

            switch (child.Role)
            {
                case BlobRoles.Thumbnail:
                    thumbId = clonedChild.Id;
                    thumbUrl = childUrl;
                    break;
                case BlobRoles.Original:
                    originalId = clonedChild.Id;
                    originalUrl = childUrl;
                    break;
                case BlobRoles.Snapshot:
                    snapshotId = clonedChild.Id;
                    snapshotUrl = childUrl;
                    break;
                case BlobRoles.SnapshotThumbnail:
                    snapshotThumbId = clonedChild.Id;
                    snapshotThumbUrl = childUrl;
                    break;
                case BlobRoles.Gif:
                    gifId = clonedChild.Id;
                    gifUrl = childUrl;
                    break;
            }
        }

        var output = new UploadAttachmentOutput
        {
            BlobId = newPrimaryBlob.Id,
            MediaType = payload.MediaType,
            Url = await GetFileUrlAsync(newPrimaryBlob.Id, newPrimaryBlob.MimeType),
            ThumbnailBlobId = thumbId,
            ThumbnailUrl = thumbUrl,
            OriginalBlobId = originalId,
            OriginalUrl = originalUrl,
            SnapshotBlobId = snapshotId,
            SnapshotUrl = snapshotUrl,
            SnapshotThumbnailBlobId = snapshotThumbId,
            SnapshotThumbnailUrl = snapshotThumbUrl,
            GifBlobId = gifId,
            GifUrl = gifUrl,
            FileName = newPrimaryBlob.FileName,
            ContentType = newPrimaryBlob.MimeType,
            Suffix = newPrimaryBlob.Suffix,
            Size = newPrimaryBlob.FileSize,
            Width = payload.Width,
            Height = payload.Height,
            Duration = payload.Duration,
            Profile = payload.Profile,
            ExpireTime = newPrimaryBlob.ExpireTime,
            ReuseToken = IssueReuseToken(
                newPrimaryBlob.Id,
                payload.MediaType,
                payload.Width,
                payload.Height,
                payload.Duration,
                payload.Profile,
                newPrimaryBlob.ExpireTime),
        };

        if (normalizedClientUploadId != null)
        {
            await reuseRecordRepository.InsertAsync(
                new BlobReuseRecord(
                    GuidGenerator.Create(),
                    CurrentUser.Id.Value,
                    sessionUnitId,
                    payload.PrimaryBlobId,
                    normalizedClientUploadId,
                    fileNameDigest,
                    output.BlobId,
                    jsonSerializer.Serialize(output),
                    output.ExpireTime),
                autoSave: true);
        }

        if (reuseCache != null && clientUploadKey != null)
        {
            var currentUow = UnitOfWorkManager?.Current;
            if (currentUow != null)
            {
                currentUow.OnCompleted(async () =>
                {
                    await reuseCache.SetAsync(clientUploadKey, output, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                    });
                });
            }
            else
            {
                await reuseCache.SetAsync(clientUploadKey, output, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                });
            }
        }

        return output;
    }

    protected virtual string IssueReuseToken(
        Guid primaryBlobId,
        AttachmentMediaType mediaType,
        int? width,
        int? height,
        int? duration,
        string profile,
        DateTime? expireTime)
    {
        if (!CurrentUser.Id.HasValue || primaryBlobId == Guid.Empty)
        {
            return null;
        }

        var tokenService = LazyServiceProvider.LazyGetRequiredService<IAttachmentReuseTokenService>();
        return tokenService.IssueToken(
            CurrentUser.Id.Value,
            primaryBlobId,
            mediaType,
            width,
            height,
            duration,
            profile,
            expireTime);
    }

    protected virtual async Task<Blob> CloneTempBlobAsync(
        Blob sourceBlob,
        Guid newBlobId,
        SessionUnitCacheItem targetUnit,
        AttachmentMediaType mediaType,
        BlobRoles role,
        Guid? parentBlobId,
        string overrideFileName = null)
    {
        var fileName = string.IsNullOrWhiteSpace(overrideFileName) ? sourceBlob.FileName : overrideFileName;
        var directoryName = await BlobResolver.GetDirectoryNameAsync(new BlobContext
        {
            BlobId = newBlobId,
            Container = sourceBlob.Container ?? ChatFilesContainer,
            SessionId = targetUnit.SessionId!.Value,
            SessionUnitId = targetUnit.Id,
            ChatObjectId = targetUnit.OwnerId,
            MediaType = mediaType,
            BlobRole = role,
            FileSize = sourceBlob.FileSize,
            MimeType = sourceBlob.MimeType,
            FileName = fileName,
        });

        var storedFileName = await GenerateFileNameAsync(sourceBlob.Suffix);
        var blobName = Path.Combine(directoryName, storedFileName).Replace(@"\", "/");
        byte[] fallbackBytes = null;
        if (!sourceBlob.StorageObjectId.HasValue)
        {
            fallbackBytes = await BlobManager.GetBytesAsync(sourceBlob.Id);
        }

        return await BlobManager.CreateAsync(new Blob(newBlobId, sourceBlob.Container ?? ChatFilesContainer)
        {
            IsPublic = false,
            FileSize = sourceBlob.FileSize,
            MimeType = sourceBlob.MimeType,
            FileName = Path.GetFileName(fileName),
            Name = blobName,
            Suffix = sourceBlob.Suffix,
            StorageObjectId = sourceBlob.StorageObjectId,
            Bytes = fallbackBytes,
            UploaderChatObjectId = targetUnit.OwnerId,
            UploadSessionUnitId = targetUnit.Id,
            UploadSessionId = targetUnit.SessionId,
            ParentBlobId = parentBlobId,
            Role = role,
            Status = BlobStatus.Temp,
            ExpireTime = GetTempExpireTime(),
            RefCount = 0,
        });
    }

    private static (int width, int height) GetImageActualDimensions(Image image)
    {
        int width = checked((int)image.Width);
        int height = checked((int)image.Height);
        if (image.Orientation is OrientationType.LeftTop or OrientationType.RightTop or
            OrientationType.RightBottom or OrientationType.LeftBottom)
        {
            (height, width) = (width, height);
        }
        return (width, height);
    }

    private string GetImageProfileJson(Image image)
    {
        var items = new List<ImageProfileEntry>();
        var profile = image.GetExifProfile();
        if (profile != null)
        {
            foreach (var exifValue in profile.Values)
            {
                items.Add(new ImageProfileEntry
                {
                    Tag = exifValue.Tag.ToString(),
                    Name = exifValue.ToString(),
                    Value = exifValue.GetValue()?.ToString(),
                });
            }
        }
        return jsonSerializer.Serialize(items);
    }

    private void DeleteGeneratedTempFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var root = Path.GetFullPath(MessageSetting.OutputTempPath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            if (fullPath.StartsWith(root, StringComparison.Ordinal))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            Logger.LogWarning(ex, "Could not delete generated temp media file {Path}", path);
        }
    }
}
