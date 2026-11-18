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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
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
            Profile = GetImageProfileJson(sourceImage),
            ExpireTime = primaryBlob.ExpireTime,
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
        int? resolvedWidth = width;
        int? resolvedHeight = height;

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
            if (!resolvedWidth.HasValue || !resolvedHeight.HasValue)
            {
                using var snapImg = new Image(snapBytes);
                (resolvedWidth, resolvedHeight) = GetImageActualDimensions(snapImg);
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

        return new UploadAttachmentOutput
        {
            BlobId = videoBlob.Id,
            MediaType = AttachmentMediaType.Video,
            Url = await GetFileUrlAsync(videoBlob.Id, videoBlob.MimeType),
            SnapshotBlobId = snapshotBlobId,
            SnapshotUrl = snapshotUrl,
            SnapshotThumbnailBlobId = snapshotThumbBlobId,
            SnapshotThumbnailUrl = snapshotThumbUrl,
            FileName = videoBlob.FileName,
            ContentType = videoBlob.MimeType,
            Suffix = videoBlob.Suffix,
            Size = videoBlob.FileSize,
            Width = resolvedWidth,
            Height = resolvedHeight,
            Duration = durationMs,
            ExpireTime = videoBlob.ExpireTime,
        };
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
        };
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
}
