using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Imaging;
using System.IO;
using Image = ImageMagick.MagickImage;
using System.Linq;
using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using ImageMagick;
using Volo.Abp.Json;
using IczpNet.Chat.Medias;
using Microsoft.CodeAnalysis;
using Volo.Abp.Http;
using Blob = IczpNet.Chat.Blobs.Blob;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Validation;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Hosting;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
namespace IczpNet.Chat.Controllers;

[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/message-sender")]
public class MessageSenderController(
        ISessionUnitManager sessionUnitManager,
        IMessageSenderAppService messageSenderAppService,
        IOptions<MessageOptions> imageSetting,
        IJsonSerializer jsonSerializer,
        IMediaResolver mediaResolver) : ChatController
{
    protected IMessageSenderAppService MessageSenderAppService { get; set; } = messageSenderAppService;
    protected ISessionUnitManager SessionUnitManager { get; set; } = sessionUnitManager;
    protected MessageOptions MessageSetting { get; set; } = imageSetting.Value;
    protected IMediaResolver MediaResolver { get; set; } = mediaResolver;
    protected IJsonSerializer JsonSerializer { get; set; } = jsonSerializer;

    /// <summary>
    /// 获取目录名称
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="folder"></param>
    /// <param name="sessionUnit"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected virtual async Task<string> GetDirectoryNameAsync(Guid blobId, string folder, ISessionUnit sessionUnit, IFormFile file)
    {
        var directoryName = await BlobResolver.GetDirectoryNameAsync(new BlobContext()
        {
            BlobId = blobId,
            Container = ChatFilesContainer,
            Folder = folder,
            SessionId = sessionUnit.SessionId.Value,
            SessionUnitId = sessionUnit.Id,
            FileSize = file.Length,
            MimeType = file.ContentType,
            FileName = file.FileName,
        });
        return directoryName;
    }

    protected virtual async Task<SessionUnitCacheItem> GetAndCheckSendSessionUnitAsync(Guid sessionUnitId)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Authentication required.");
        }

        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        var chatObjectManager = LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (!ownerIds.Contains(unit.OwnerId))
        {
            throw new AbpAuthorizationException("SessionUnit does not belong to current user.");
        }

        if (!unit.IsEnabled || !unit.IsInputEnabled || (unit.MuteExpireTime.HasValue && unit.MuteExpireTime.Value > Clock.Now))
        {
            throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied)
                .WithData("Reason", "Target session unit does not allow sending attachments or is muted.");
        }

        return unit;
    }

    protected virtual Task ValidateUploadAsync(
        SessionUnitCacheItem unit,
        AttachmentMediaType mediaType,
        string fileName,
        string contentType,
        string suffix,
        byte[] bytes,
        bool isOriginal = false,
        bool hasSnapshot = false,
        int? durationMs = null)
    {
        var validatorManager = LazyServiceProvider.LazyGetRequiredService<IAttachmentUploadValidatorManager>();
        return validatorManager.ValidateAsync(new AttachmentUploadValidationContext
        {
            SessionUnit = unit,
            MediaType = mediaType,
            FileName = fileName,
            ContentType = contentType,
            Suffix = suffix,
            FileSize = bytes?.LongLength ?? 0L,
            DurationMs = durationMs,
            IsOriginal = isOriginal,
            HasSnapshot = hasSnapshot,
            OpenReadStreamAsync = _ => Task.FromResult<Stream>(new MemoryStream(bytes ?? [], writable: false)),
        });
    }

    protected static string FormatCacheableImageMetadata(string profile)
        => !string.IsNullOrWhiteSpace(profile) && profile.Length <= 2000 ? profile : null;

    protected virtual async Task<string> ResolveBlobSha256Async(
        Blob blob,
        byte[] bytes,
        IAttachmentProcessingReuseManager reuseManager)
    {
        if (!string.IsNullOrWhiteSpace(blob?.ContentSha256))
        {
            return blob.ContentSha256;
        }

        if (!string.IsNullOrWhiteSpace(blob?.StorageObject?.Sha256))
        {
            return blob.StorageObject.Sha256;
        }

        if (blob?.StorageObjectId != null)
        {
            var storageObj = await BlobManager.FindStorageObjectAsync(blob.StorageObjectId.Value);
            if (!string.IsNullOrWhiteSpace(storageObj?.Sha256))
            {
                blob.ContentSha256 = storageObj.Sha256;
                return storageObj.Sha256;
            }
        }

        return reuseManager != null
            ? reuseManager.ComputeSha256(bytes)
            : Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(bytes ?? []));
    }

    protected virtual Task<Blob> SavePrivateChatTempBlobAsync(
        Guid blobId,
        byte[] bytes,
        string fileName,
        string contentType,
        string suffix,
        string blobName,
        SessionUnitCacheItem unit,
        BlobRoles role,
        Guid? parentBlobId = null,
        string contentSha256 = null)
    {
        var uploadOptions = LazyServiceProvider.LazyGetRequiredService<IOptions<AttachmentUploadOptions>>().Value;
        return BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
        {
            IsPublic = false,
            FileSize = bytes.Length,
            MimeType = contentType,
            FileName = Path.GetFileName(fileName),
            Name = blobName,
            Suffix = suffix,
            Bytes = bytes,
            ContentSha256 = contentSha256,
            UploaderChatObjectId = unit.OwnerId,
            UploadSessionUnitId = unit.Id,
            UploadSessionId = unit.SessionId,
            ParentBlobId = parentBlobId,
            Role = role,
            Status = BlobStatus.Temp,
            ExpireTime = Clock.Now.AddHours(uploadOptions.TempBlobExpireHours),
            RefCount = 0,
        });
    }

    protected virtual Task<Blob> SaveTempBlobFromReadyResultAsync(
        Guid blobId,
        AttachmentProcessingResult readyResult,
        string fileName,
        string fallbackMimeType,
        string fallbackSuffix,
        string blobName,
        SessionUnitCacheItem unit,
        BlobRoles role,
        Guid? parentBlobId = null)
    {
        var uploadOptions = LazyServiceProvider.LazyGetRequiredService<IOptions<AttachmentUploadOptions>>().Value;
        return BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
        {
            IsPublic = false,
            FileSize = readyResult.OutputFileSize ?? 0,
            MimeType = readyResult.OutputMimeType ?? fallbackMimeType,
            FileName = Path.GetFileName(fileName),
            Name = blobName,
            Suffix = readyResult.OutputSuffix ?? fallbackSuffix,
            StorageObjectId = readyResult.OutputStorageObjectId,
            UploaderChatObjectId = unit.OwnerId,
            UploadSessionUnitId = unit.Id,
            UploadSessionId = unit.SessionId,
            ParentBlobId = parentBlobId,
            Role = role,
            Status = BlobStatus.Temp,
            ExpireTime = Clock.Now.AddHours(uploadOptions.TempBlobExpireHours),
            RefCount = 0,
        });
    }

    /// <summary>
    /// 上传并发送(file/sound/image/video)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="quoteMessageId">引消息Id</param>
    /// <param name="remindList">提醒（@XXX）</param>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [Route("send-upload-file/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadFileAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var sessionUnit = await GetAndCheckSendSessionUnitAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        var directoryName = await GetDirectoryNameAsync(blobId, "files", sessionUnit, file);

        var bytes = await file.GetAllBytesAsync();
        var rawExt = Path.GetExtension(file.FileName);
        var suffix = string.IsNullOrWhiteSpace(rawExt) ? GetExtension(file.ContentType, file.FileName) : rawExt;
        await ValidateUploadAsync(sessionUnit, AttachmentMediaType.File, file.FileName, file.ContentType, suffix, bytes);
        var storedFileName = await GenerateFileNameAsync(GetExtension(file.ContentType, file.FileName));
        var blobName = Path.Combine(directoryName, storedFileName).Replace(@"\", "/");

        var blob = await SavePrivateChatTempBlobAsync(
            blobId,
            bytes,
            file.FileName,
            file.ContentType,
            suffix,
            blobName,
            sessionUnit,
            BlobRoles.Primary);

        var sendResult = await MessageSenderAppService.SendFileAsync(sessionUnitId, new MessageInput<FileContentInfo>()
        {
            QuoteMessageId = quoteMessageId,
            RemindList = remindList,
            Content = new FileContentInfo()
            {
                ContentType = file.ContentType,
                Size = blob.FileSize,
                FileName = file.FileName,
                Suffix = blob.Suffix,
                Url = await GetFileUrlAsync(blob.Id, file.ContentType),
                BlobId = blob.Id,
            }
        });
        return new JsonResult(sendResult);
    }

    /// <summary>
    /// 获取真实宽高
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    protected virtual (int width, int height) GetImageActualDimensions(Image image)
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

    protected virtual List<ImageProfileEntry> GetImageProfile(Image image)
    {
        var items = new List<ImageProfileEntry>();

        var profile = image.GetExifProfile();
        if (profile != null)
        {
            foreach (var exifValue in profile.Values)
            {
                items.Add(new ImageProfileEntry()
                {
                    Tag = exifValue.Tag.ToString(),
                    Name = exifValue.ToString(),
                    Value = exifValue.GetValue().ToString()
                });
            }
        }
        return items;
    }

    protected virtual string GetImageProfileJson(Image image, List<ImageProfileEntry> otherEntries = null)
    {
        var profileEntries = GetImageProfile(image);
        if (otherEntries != null)
        {
            profileEntries = [.. profileEntries, .. otherEntries];
        }
        var json = JsonSerializer.Serialize(profileEntries);
        return json;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="file">文件</param>
    /// <param name="quoteMessageId">引用消息</param>
    /// <param name="remindList">提醒器(sessionUnitId)</param>
    /// <param name="isOriginal">是否使用原始图片</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [Route("send-upload-image/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadImageAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var sessionUnit = await GetAndCheckSendSessionUnitAsync(sessionUnitId);

        var bytes = await file.GetAllBytesAsync();

        var suffix = GetExtension(file.ContentType, file.FileName);
        await ValidateUploadAsync(sessionUnit, AttachmentMediaType.Image, file.FileName, file.ContentType, suffix, bytes, isOriginal: isOriginal);

        var filename = $"{Clock.Now:yyyyMMddhhmmss}_{ShortIdGenerator.Create()}".ToLower();
        var primaryBlobId = GuidGenerator.Create();
        var thumbnailBlobId = GuidGenerator.Create();
        var directoryName = await GetDirectoryNameAsync(primaryBlobId, "images", sessionUnit, file);

        var reuseManager = LazyServiceProvider.LazyGetService<IAttachmentProcessingReuseManager>();
        var reuseEnabled = reuseManager?.IsEnabled() ?? false;
        var storageScope = CurrentTenant.Id?.ToString("N") ?? "host";
        var sourceSha256 = reuseEnabled ? reuseManager.ComputeSha256(bytes) : null;
        var bigImageSize = MessageSetting.ImageSetting.BigSize;
        var thumbnailImageSize = MessageSetting.ImageSetting.ThumbnailSize;
        var primaryRecipeHash = reuseEnabled ? reuseManager.BuildImagePrimaryRecipeHash(MessageSetting.ImageSetting) : null;
        var thumbRecipeHash = reuseEnabled ? reuseManager.BuildImageThumbnailRecipeHash(MessageSetting.ImageSetting) : null;

        Image sourceImage = null;
        Image GetOrDecodeSourceImage() => sourceImage ??= new Image(bytes);

        try
        {
            var imageContent = new ImageContentInfo()
            {
                BlobId = primaryBlobId,
                ThumbnailBlobId = thumbnailBlobId,
                ContentType = file.ContentType,
                Suffix = suffix
            };

            byte[] primaryBytesForThumb = bytes;

            if (isOriginal)
            {
                var originalBlob = await SavePrivateChatTempBlobAsync(
                    primaryBlobId,
                    bytes,
                    file.FileName,
                    file.ContentType,
                    suffix,
                    $"{directoryName}/{filename}_original{suffix}",
                    sessionUnit,
                    BlobRoles.Primary,
                    contentSha256: sourceSha256);

                var srcImg = GetOrDecodeSourceImage();
                (int width, int height) = GetImageActualDimensions(srcImg);

                imageContent.BlobId = originalBlob.Id;
                imageContent.OriginalBlobId = originalBlob.Id;
                imageContent.OriginalUrl = await GetFileUrlAsync(originalBlob.Id, originalBlob.MimeType);
                imageContent.Url = imageContent.OriginalUrl;
                imageContent.Width = width;
                imageContent.Height = height;
                imageContent.Size = originalBlob.FileSize;
                imageContent.Profile = GetImageProfileJson(srcImg);
            }
            else
            {
                Blob primaryBlob = null;
                byte[] primaryBytes = null;
                int? primaryWidth = null;
                int? primaryHeight = null;
                string profile = null;

                if (reuseEnabled)
                {
                    var readyPrimary = await reuseManager.FindReadyResultAsync(
                        storageScope, ChatFilesContainer, sourceSha256, bytes.LongLength,
                        AttachmentMediaType.Image, BlobRoles.Primary, primaryRecipeHash);
                    if (readyPrimary?.OutputStorageObjectId != null)
                    {
                        primaryBlob = await SaveTempBlobFromReadyResultAsync(
                            primaryBlobId,
                            readyPrimary,
                            file.FileName,
                            file.ContentType,
                            suffix,
                            $"{directoryName}/{filename}_{bigImageSize}{readyPrimary.OutputSuffix ?? suffix}",
                            sessionUnit,
                            BlobRoles.Primary);
                        primaryWidth = readyPrimary.Width;
                        primaryHeight = readyPrimary.Height;
                        profile = readyPrimary.MetadataJson;
                    }
                }

                if (primaryBlob == null)
                {
                    var resizeder = await ImageResizer.ResizeAsync(bytes, new ImageResizeArgs()
                    {
                        Width = (uint)bigImageSize,
                        Height = (uint)bigImageSize,
                        Mode = ImageResizeMode.Max
                    });
                    if (resizeder.State == ImageProcessState.Unsupported)
                    {
                        throw new Volo.Abp.UserFriendlyException("Unsupported image format.");
                    }

                    primaryBytes = resizeder.State == ImageProcessState.Done && resizeder.Result?.Length > 0
                        ? resizeder.Result
                        : bytes;

                    if (!ReferenceEquals(primaryBytes, bytes))
                    {
                        using Image img = new(primaryBytes);
                        (primaryWidth, primaryHeight) = GetImageActualDimensions(img);
                    }
                    else
                    {
                        (primaryWidth, primaryHeight) = GetImageActualDimensions(GetOrDecodeSourceImage());
                    }

                    profile = GetImageProfileJson(GetOrDecodeSourceImage());

                    primaryBlob = await SavePrivateChatTempBlobAsync(
                        primaryBlobId,
                        primaryBytes,
                        file.FileName,
                        file.ContentType,
                        suffix,
                        $"{directoryName}/{filename}_{bigImageSize}{suffix}",
                        sessionUnit,
                        BlobRoles.Primary,
                        contentSha256: ReferenceEquals(primaryBytes, bytes) ? sourceSha256 : null);

                    if (reuseEnabled && primaryBlob.StorageObjectId.HasValue)
                    {
                        await reuseManager.RecordReadyResultAsync(
                            storageScope, primaryBlob.Container, sourceSha256, bytes.LongLength,
                            AttachmentMediaType.Image, BlobRoles.Primary, primaryRecipeHash,
                            primaryBlob.StorageObjectId, primaryBlob.MimeType, primaryBlob.Suffix, primaryBlob.FileSize,
                            primaryWidth, primaryHeight, null,
                            metadataJson: FormatCacheableImageMetadata(profile));
                    }
                }

                primaryBytesForThumb = primaryBytes;

                if (!primaryWidth.HasValue || !primaryHeight.HasValue)
                {
                    var (srcW, srcH) = GetImageActualDimensions(GetOrDecodeSourceImage());
                    primaryWidth ??= srcW;
                    primaryHeight ??= srcH;
                }

                profile ??= GetImageProfileJson(GetOrDecodeSourceImage());

                imageContent.BlobId = primaryBlob.Id;
                imageContent.Width = primaryWidth.Value;
                imageContent.Height = primaryHeight.Value;
                imageContent.Size = primaryBlob.FileSize;
                imageContent.Url = await GetFileUrlAsync(primaryBlob.Id, primaryBlob.MimeType);
                imageContent.Profile = profile;
            }

            Blob thumbBlob = null;
            if (reuseEnabled)
            {
                var readyThumb = await reuseManager.FindReadyResultAsync(
                    storageScope, ChatFilesContainer, sourceSha256, bytes.LongLength,
                    AttachmentMediaType.Image, BlobRoles.Thumbnail, thumbRecipeHash);
                if (readyThumb?.OutputStorageObjectId != null)
                {
                    thumbBlob = await SaveTempBlobFromReadyResultAsync(
                        thumbnailBlobId,
                        readyThumb,
                        file.FileName,
                        file.ContentType,
                        suffix,
                        $"{directoryName}/{filename}_{thumbnailImageSize}{readyThumb.OutputSuffix ?? suffix}",
                        sessionUnit,
                        BlobRoles.Thumbnail,
                        parentBlobId: imageContent.BlobId);
                }
            }

            if (thumbBlob == null)
            {
                var thumbSourceBytes = primaryBytesForThumb ?? bytes;
                var resizedThumb = await ImageResizer.ResizeAsync(thumbSourceBytes, new ImageResizeArgs()
                {
                    Width = (uint)thumbnailImageSize,
                    Height = (uint)thumbnailImageSize,
                    Mode = ImageResizeMode.Max
                });
                if (resizedThumb.State == ImageProcessState.Unsupported)
                {
                    throw new Volo.Abp.UserFriendlyException("Unsupported image format.");
                }

                var thumbBytes = resizedThumb.State == ImageProcessState.Done && resizedThumb.Result?.Length > 0
                    ? resizedThumb.Result
                    : thumbSourceBytes;

                thumbBlob = await SavePrivateChatTempBlobAsync(
                    thumbnailBlobId,
                    thumbBytes,
                    file.FileName,
                    file.ContentType,
                    suffix,
                    $"{directoryName}/{filename}_{thumbnailImageSize}{suffix}",
                    sessionUnit,
                    BlobRoles.Thumbnail,
                    parentBlobId: imageContent.BlobId,
                    contentSha256: ReferenceEquals(thumbBytes, bytes) ? sourceSha256 : null);

                if (reuseEnabled && thumbBlob.StorageObjectId.HasValue)
                {
                    await reuseManager.RecordReadyResultAsync(
                        storageScope, thumbBlob.Container, sourceSha256, bytes.LongLength,
                        AttachmentMediaType.Image, BlobRoles.Thumbnail, thumbRecipeHash,
                        thumbBlob.StorageObjectId, thumbBlob.MimeType, thumbBlob.Suffix, thumbBlob.FileSize,
                        null, null, null);
                }
            }

            imageContent.ThumbnailBlobId = thumbBlob.Id;
            imageContent.ThumbnailUrl = await GetFileUrlAsync(thumbBlob.Id, thumbBlob.MimeType);

            var sendResult = await MessageSenderAppService.SendImageAsync(sessionUnitId, new MessageInput<ImageContentInfo>()
            {
                QuoteMessageId = quoteMessageId,
                RemindList = remindList,
                Content = imageContent,
            });

            return new JsonResult(sendResult);
        }
        finally
        {
            sourceImage?.Dispose();
        }
    }

    [HttpPost]
    [Authorize]
    [Route("send-upload-sound/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadSoundAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, [FromForm] int? duration)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var sessionUnit = await GetAndCheckSendSessionUnitAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        var directoryName = await GetDirectoryNameAsync(blobId, "audios", sessionUnit, file);

        var bytes = await file.GetAllBytesAsync();
        var rawExt = Path.GetExtension(file.FileName);
        var suffix = string.IsNullOrWhiteSpace(rawExt) ? GetExtension(file.ContentType, file.FileName) : rawExt;
        await ValidateUploadAsync(sessionUnit, AttachmentMediaType.Sound, file.FileName, file.ContentType, suffix, bytes, durationMs: duration);
        var storedFileName = await GenerateFileNameAsync(GetExtension(file.ContentType, file.FileName));
        var blobName = Path.Combine(directoryName, storedFileName).Replace(@"\", "/");

        var soundBlob = await SavePrivateChatTempBlobAsync(
            blobId,
            bytes,
            file.FileName,
            file.ContentType,
            suffix,
            blobName,
            sessionUnit,
            BlobRoles.Primary);

        var content = new SoundContentInfo()
        {
            BlobId = soundBlob.Id,
            ContentType = file.ContentType,
            Size = file.Length,
            FileName = Path.GetFileName(file.FileName),
            Suffix = Path.GetExtension(file.FileName),
            Url = await GetFileUrlAsync(soundBlob.Id, file.ContentType),
            Time = duration.GetValueOrDefault(),
        };

        if (!duration.HasValue)
        {
            var reuseManager = LazyServiceProvider.LazyGetService<IAttachmentProcessingReuseManager>();
            var reuseEnabled = reuseManager?.IsEnabled() ?? false;
            var storageScope = CurrentTenant.Id?.ToString("N") ?? "host";
            var soundSha256 = reuseEnabled ? await ResolveBlobSha256Async(soundBlob, bytes, reuseManager) : null;
            var soundRecipeHash = reuseEnabled ? reuseManager.BuildSoundProbeRecipeHash() : null;

            var readySound = reuseEnabled
                ? await reuseManager.FindReadyResultAsync(
                    storageScope, soundBlob.Container, soundSha256, bytes.LongLength,
                    AttachmentMediaType.Sound, BlobRoles.Primary, soundRecipeHash)
                : null;

            if (readySound?.DurationMs != null)
            {
                content.Time = readySound.DurationMs.Value;
            }
            else
            {
                var audioInfo = await MediaResolver.GetAudioInfoAsync(bytes, soundBlob.FileName);

                if (audioInfo?.Duration != null && audioInfo.Duration.Value > TimeSpan.Zero)
                {
                    content.Time = (int)audioInfo.Duration.Value.TotalMilliseconds;
                    if (reuseEnabled && soundBlob.StorageObjectId.HasValue)
                    {
                        await reuseManager.RecordReadyResultAsync(
                            storageScope, soundBlob.Container, soundSha256, bytes.LongLength,
                            AttachmentMediaType.Sound, BlobRoles.Primary, soundRecipeHash,
                            soundBlob.StorageObjectId, soundBlob.MimeType, soundBlob.Suffix, soundBlob.FileSize,
                            null, null, content.Time);
                    }
                }
            }
        }

        var sendResult = await MessageSenderAppService.SendSoundAsync(sessionUnitId, new MessageInput<SoundContentInfo>()
        {
            QuoteMessageId = quoteMessageId,
            RemindList = remindList,
            Content = content
        });
        return new JsonResult(sendResult);
    }

    [HttpPost]
    [Authorize]
    [Route("send-upload-video/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadVideoAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        if (file == null)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileEmpty);
        }

        var sessionUnit = await GetAndCheckSendSessionUnitAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        var directoryName = await GetDirectoryNameAsync(blobId, "videos", sessionUnit, file);

        var bytes = await file.GetAllBytesAsync();
        var rawExt = Path.GetExtension(file.FileName);
        var suffix = string.IsNullOrWhiteSpace(rawExt) ? GetExtension(file.ContentType, file.FileName) : rawExt;
        await ValidateUploadAsync(sessionUnit, AttachmentMediaType.Video, file.FileName, file.ContentType, suffix, bytes, isOriginal: isOriginal, hasSnapshot: MessageSetting.VideoSetting.IsGenerateSnapshot);
        var storedFileName = await GenerateFileNameAsync(GetExtension(file.ContentType, file.FileName));
        var blobName = Path.Combine(directoryName, storedFileName).Replace(@"\", "/");

        var videoBlob = await SavePrivateChatTempBlobAsync(
            blobId,
            bytes,
            file.FileName,
            file.ContentType,
            suffix,
            blobName,
            sessionUnit,
            BlobRoles.Primary);

        var content = new VideoContentInfo()
        {
            BlobId = videoBlob.Id,
            ContentType = file.ContentType,
            Size = file.Length,
            FileName = Path.GetFileName(file.FileName),
            Suffix = Path.GetExtension(file.FileName),
            Url = await GetFileUrlAsync(videoBlob.Id, file.ContentType)
        };

        if (MessageSetting.VideoSetting.IsGenerateSnapshot)
        {
            var reuseManager = LazyServiceProvider.LazyGetService<IAttachmentProcessingReuseManager>();
            var reuseEnabled = reuseManager?.IsEnabled() ?? false;
            var storageScope = CurrentTenant.Id?.ToString("N") ?? "host";
            var videoSha256 = reuseEnabled ? await ResolveBlobSha256Async(videoBlob, bytes, reuseManager) : null;
            var snapRecipeHash = reuseEnabled ? reuseManager.BuildVideoSnapshotRecipeHash(MessageSetting.VideoSetting) : null;
            var snapThumbRecipeHash = reuseEnabled ? reuseManager.BuildVideoSnapshotThumbnailRecipeHash(MessageSetting.VideoSetting) : null;
            var gifRecipeHash = reuseEnabled ? reuseManager.BuildVideoGifRecipeHash(MessageSetting.VideoSetting) : null;

            AttachmentProcessingResult readySnap = null;
            if (reuseEnabled)
            {
                readySnap = await reuseManager.FindReadyResultAsync(
                    storageScope, videoBlob.Container, videoSha256, bytes.LongLength,
                    AttachmentMediaType.Video, BlobRoles.Snapshot, snapRecipeHash);
                if (readySnap?.OutputStorageObjectId != null)
                {
                    var snapSuffix = readySnap.OutputSuffix ?? ".png";
                    var snapMime = readySnap.OutputMimeType ?? "image/png";
                    var snapBlob = await SaveTempBlobFromReadyResultAsync(
                        GuidGenerator.Create(),
                        readySnap,
                        $"{Path.GetFileNameWithoutExtension(file.FileName)}.snapshot{snapSuffix}",
                        snapMime,
                        snapSuffix,
                        $"{videoBlob.Name}.snapshot{snapSuffix}",
                        sessionUnit,
                        BlobRoles.Snapshot,
                        parentBlobId: videoBlob.Id);
                    content.SnapshotBlobId = snapBlob.Id;
                    content.SnapshotUrl = await GetFileUrlAsync(snapBlob.Id, snapBlob.MimeType);
                    if (readySnap.Width.GetValueOrDefault() > 0)
                    {
                        content.Width = readySnap.Width.Value;
                        content.ImageWidth = readySnap.Width.Value;
                    }
                    if (readySnap.Height.GetValueOrDefault() > 0)
                    {
                        content.Height = readySnap.Height.Value;
                        content.ImageHeight = readySnap.Height.Value;
                    }
                    if (readySnap.DurationMs.HasValue)
                    {
                        content.Duration = readySnap.DurationMs.Value / 1000.0;
                    }
                }

                var readySnapThumb = await reuseManager.FindReadyResultAsync(
                    storageScope, videoBlob.Container, videoSha256, bytes.LongLength,
                    AttachmentMediaType.Video, BlobRoles.SnapshotThumbnail, snapThumbRecipeHash);
                if (readySnapThumb?.OutputStorageObjectId != null)
                {
                    var thumbSuffix = readySnapThumb.OutputSuffix ?? ".png";
                    var thumbMime = readySnapThumb.OutputMimeType ?? "image/png";
                    var thumbBlob = await SaveTempBlobFromReadyResultAsync(
                        GuidGenerator.Create(),
                        readySnapThumb,
                        $"thumb_{Path.GetFileNameWithoutExtension(file.FileName)}{thumbSuffix}",
                        thumbMime,
                        thumbSuffix,
                        $"{videoBlob.Name}.thumbnail{thumbSuffix}",
                        sessionUnit,
                        BlobRoles.SnapshotThumbnail,
                        parentBlobId: videoBlob.Id);
                    content.SnapshotThumbnailBlobId = thumbBlob.Id;
                    content.SnapshotThumbnailUrl = await GetFileUrlAsync(thumbBlob.Id, thumbBlob.MimeType);
                }

                var readyGif = await reuseManager.FindReadyResultAsync(
                    storageScope, videoBlob.Container, videoSha256, bytes.LongLength,
                    AttachmentMediaType.Video, BlobRoles.Gif, gifRecipeHash);
                if (readyGif?.OutputStorageObjectId != null)
                {
                    var gifSuffix = readyGif.OutputSuffix ?? ".gif";
                    var gifMime = readyGif.OutputMimeType ?? "image/gif";
                    var gifBlob = await SaveTempBlobFromReadyResultAsync(
                        GuidGenerator.Create(),
                        readyGif,
                        $"{Path.GetFileNameWithoutExtension(file.FileName)}{gifSuffix}",
                        gifMime,
                        gifSuffix,
                        $"{videoBlob.Name}.snapshot{gifSuffix}",
                        sessionUnit,
                        BlobRoles.Gif,
                        parentBlobId: videoBlob.Id);
                    content.GifBlobId = gifBlob.Id;
                    content.GifUrl = await GetFileUrlAsync(gifBlob.Id, gifBlob.MimeType);
                }

                // 封面命中但封面缩略图缺失：直接从已就绪封面补生成缩略图并登记成品，跳过 FFmpeg
                if (content.SnapshotBlobId.HasValue && !content.SnapshotThumbnailBlobId.HasValue && readySnap?.OutputStorageObjectId != null)
                {
                    var snapObj = await BlobManager.FindStorageObjectAsync(readySnap.OutputStorageObjectId.Value);
                    if (snapObj != null && snapObj.Status == BlobObjectStatus.Ready)
                    {
                        var existingSnapBytes = await BlobManager.GetBytesAsync(snapObj.Container, snapObj.StorageName);
                        var thumbSize = MessageSetting.VideoSetting.SnapshotThumbnailSize;
                        var resized = await ImageResizer.ResizeAsync(existingSnapBytes, new ImageResizeArgs()
                        {
                            Width = (uint)thumbSize,
                            Height = (uint)thumbSize,
                            Mode = ImageResizeMode.Max
                        });
                        var snapThumbBytes = resized.State == ImageProcessState.Done && resized.Result?.Length > 0
                            ? resized.Result
                            : existingSnapBytes;
                        var snapSuffix = readySnap.OutputSuffix ?? ".png";
                        var snapMime = readySnap.OutputMimeType ?? "image/png";
                        var snapThumbBlob = await SavePrivateChatTempBlobAsync(
                            GuidGenerator.Create(),
                            snapThumbBytes,
                            $"thumb_{Path.GetFileNameWithoutExtension(file.FileName)}{snapSuffix}",
                            snapMime,
                            snapSuffix,
                            $"{videoBlob.Name}.thumbnail{snapSuffix}",
                            sessionUnit,
                            BlobRoles.SnapshotThumbnail,
                            parentBlobId: videoBlob.Id);
                        content.SnapshotThumbnailBlobId = snapThumbBlob.Id;
                        content.SnapshotThumbnailUrl = await GetFileUrlAsync(snapThumbBlob.Id, snapThumbBlob.MimeType);
                        if (snapThumbBlob.StorageObjectId.HasValue)
                        {
                            await reuseManager.RecordReadyResultAsync(
                                storageScope, videoBlob.Container, videoSha256, bytes.LongLength,
                                AttachmentMediaType.Video, BlobRoles.SnapshotThumbnail, snapThumbRecipeHash,
                                snapThumbBlob.StorageObjectId, snapThumbBlob.MimeType, snapThumbBlob.Suffix, snapThumbBlob.FileSize,
                                readySnap.Width, readySnap.Height, readySnap.DurationMs);
                        }
                    }
                }
            }

            if (!content.SnapshotBlobId.HasValue || !content.SnapshotThumbnailBlobId.HasValue)
            {
                await GenerateSnapshotAsync(videoBlob, content, sessionUnit, videoSha256);
            }
        }

        var sendResult = await MessageSenderAppService.SendVideoAsync(sessionUnitId, new MessageInput<VideoContentInfo>()
        {
            QuoteMessageId = quoteMessageId,
            RemindList = remindList,
            Content = content
        });
        return new JsonResult(sendResult);
    }

    /// <summary>
    /// 生成视频快照（仅抽静态封面与封面缩略图，GIF 交由后台任务异步生成，避免上传请求超时）
    /// </summary>
    protected virtual Task GenerateSnapshotAsync(Blob videoBlob, VideoContentInfo content, SessionUnitCacheItem sessionUnit = null)
        => GenerateSnapshotAsync(videoBlob, content, sessionUnit, precomputedVideoSha256: null);

    protected virtual async Task GenerateSnapshotAsync(
        Blob videoBlob,
        VideoContentInfo content,
        SessionUnitCacheItem sessionUnit,
        string precomputedVideoSha256)
    {
        var videoFileName = videoBlob.Name;
        var mediaInfo = await MediaResolver.GetVideoInfoAsync(
            videoBlob.Bytes,
            videoBlob.FileName,
            new VideoProcessOptions(
                IncludeSnapshot: !content.SnapshotBlobId.HasValue,
                IncludeGif: false));

        if (mediaInfo == null)
        {
            Logger.LogWarning($"GetVideoInfoAsync:videoBlobId:{videoBlob.Id} is null");
            return;
        }

        var reuseManager = LazyServiceProvider.LazyGetService<IAttachmentProcessingReuseManager>();
        var reuseEnabled = reuseManager?.IsEnabled() ?? false;
        var storageScope = CurrentTenant.Id?.ToString("N") ?? "host";
        var videoSha256 = reuseEnabled
            ? (!string.IsNullOrWhiteSpace(precomputedVideoSha256)
                ? precomputedVideoSha256
                : await ResolveBlobSha256Async(videoBlob, videoBlob.Bytes, reuseManager))
            : null;
        var snapRecipeHash = reuseEnabled ? reuseManager.BuildVideoSnapshotRecipeHash(MessageSetting.VideoSetting) : null;
        var snapThumbRecipeHash = reuseEnabled ? reuseManager.BuildVideoSnapshotThumbnailRecipeHash(MessageSetting.VideoSetting) : null;

        try
        {
            var serverSnapWidth = mediaInfo.Width.GetValueOrDefault() > 0 ? mediaInfo.Width : (int?)null;
            var serverSnapHeight = mediaInfo.Height.GetValueOrDefault() > 0 ? mediaInfo.Height : (int?)null;
            var serverDurationMs = mediaInfo.Duration.HasValue ? (int)mediaInfo.Duration.Value.TotalMilliseconds : (int?)null;

            content.Width = mediaInfo.Width.GetValueOrDefault();
            content.Height = mediaInfo.Height.GetValueOrDefault();

            if (mediaInfo.Duration != null)
            {
                content.Duration = mediaInfo.Duration.Value.TotalSeconds;
            }

            if (!string.IsNullOrWhiteSpace(mediaInfo.ImageSnapshotPath) && System.IO.File.Exists(mediaInfo.ImageSnapshotPath))
            {
                var snapBytes = await System.IO.File.ReadAllBytesAsync(mediaInfo.ImageSnapshotPath);
                var suffix = Path.GetExtension(mediaInfo.ImageSnapshotPath);
                var snapMime = MimeTypes.GetByExtension(suffix);

                using (Image snapshotImg = new(snapBytes))
                {
                    content.ImageWidth = checked((int)snapshotImg.Width);
                    content.ImageHeight = checked((int)snapshotImg.Height);

                    (int width, int height) = GetImageActualDimensions(snapshotImg);
                    if (width > 0 && height > 0)
                    {
                        content.Width = width;
                        content.Height = height;
                        serverSnapWidth ??= width;
                        serverSnapHeight ??= height;
                    }
                }

                if (!content.SnapshotBlobId.HasValue)
                {
                    var snapBlob = sessionUnit != null
                        ? await SavePrivateChatTempBlobAsync(
                            GuidGenerator.Create(),
                            snapBytes,
                            $"{Path.GetFileNameWithoutExtension(videoBlob.FileName)}.snapshot{suffix}",
                            snapMime,
                            suffix,
                            $"{videoFileName}.snapshot{suffix}",
                            sessionUnit,
                            BlobRoles.Snapshot,
                            videoBlob.Id)
                        : await UploadToBlobStoreAsync(
                            mediaInfo.ImageSnapshotPath,
                            $"{videoFileName}.snapshot{suffix}",
                            sessionUnit,
                            BlobRoles.Snapshot,
                            videoBlob.Id);
                    content.SnapshotBlobId = snapBlob.Id;
                    content.SnapshotUrl = await GetFileUrlAsync(snapBlob.Id, snapBlob.MimeType);

                    if (reuseEnabled && snapBlob.StorageObjectId.HasValue)
                    {
                        await reuseManager.RecordReadyResultAsync(
                            storageScope, videoBlob.Container, videoSha256, videoBlob.Bytes.LongLength,
                            AttachmentMediaType.Video, BlobRoles.Snapshot, snapRecipeHash,
                            snapBlob.StorageObjectId, snapBlob.MimeType, snapBlob.Suffix, snapBlob.FileSize,
                            serverSnapWidth, serverSnapHeight, serverDurationMs);
                    }
                }

                if (!content.SnapshotThumbnailBlobId.HasValue)
                {
                    var thumbnailSize = MessageSetting.VideoSetting.SnapshotThumbnailSize;
                    var resizeder = await ImageResizer.ResizeAsync(snapBytes, new ImageResizeArgs()
                    {
                        Width = (uint)thumbnailSize,
                        Height = (uint)thumbnailSize,
                        Mode = ImageResizeMode.Max
                    });

                    if (resizeder.State == ImageProcessState.Unsupported)
                    {
                        throw new Volo.Abp.UserFriendlyException("Unsupported image format.");
                    }

                    var thumbnailResizedBytes = resizeder.State == ImageProcessState.Done && resizeder.Result?.Length > 0
                        ? resizeder.Result
                        : snapBytes;

                    var snapshotThumbnailBlobId = GuidGenerator.Create();
                    var snapshotThumbnailBlob = sessionUnit != null
                        ? await SavePrivateChatTempBlobAsync(
                            snapshotThumbnailBlobId,
                            thumbnailResizedBytes,
                            $"{videoBlob.FileName}",
                            snapMime,
                            suffix,
                            $"{videoFileName}.thumbnail{suffix}",
                            sessionUnit,
                            BlobRoles.SnapshotThumbnail,
                            videoBlob.Id)
                        : await BlobManager.CreateAsync(new Blob(snapshotThumbnailBlobId, ChatFilesContainer)
                        {
                            IsPublic = false,
                            FileSize = thumbnailResizedBytes.Length,
                            MimeType = snapMime,
                            FileName = $"{videoBlob.FileName}",
                            Name = $"{videoFileName}.thumbnail{suffix}",
                            Suffix = suffix,
                            Bytes = thumbnailResizedBytes,
                            ParentBlobId = videoBlob.Id,
                            Role = BlobRoles.SnapshotThumbnail,
                            Status = BlobStatus.Temp,
                        });

                    content.SnapshotThumbnailBlobId = snapshotThumbnailBlob.Id;
                    content.SnapshotThumbnailUrl = await GetFileUrlAsync(snapshotThumbnailBlob.Id, snapshotThumbnailBlob.MimeType);

                    if (reuseEnabled && snapshotThumbnailBlob.StorageObjectId.HasValue)
                    {
                        await reuseManager.RecordReadyResultAsync(
                            storageScope, videoBlob.Container, videoSha256, videoBlob.Bytes.LongLength,
                            AttachmentMediaType.Video, BlobRoles.SnapshotThumbnail, snapThumbRecipeHash,
                            snapshotThumbnailBlob.StorageObjectId, snapshotThumbnailBlob.MimeType, snapshotThumbnailBlob.Suffix, snapshotThumbnailBlob.FileSize,
                            serverSnapWidth, serverSnapHeight, serverDurationMs);
                    }
                }
            }

            if (!content.GifBlobId.HasValue &&
                !string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath) &&
                System.IO.File.Exists(mediaInfo.GifSnapshotPath))
            {
                var gifBlob = await UploadToBlobStoreAsync(
                    mediaInfo.GifSnapshotPath,
                    $"{videoFileName}.snapshot.gif",
                    sessionUnit,
                    BlobRoles.Gif,
                    videoBlob.Id);
                content.GifBlobId = gifBlob.Id;
                content.GifUrl = await GetFileUrlAsync(gifBlob.Id, gifBlob.MimeType);
            }
        }
        finally
        {
            DeleteGeneratedMediaFile(mediaInfo.ImageSnapshotPath);
            DeleteGeneratedMediaFile(mediaInfo.GifSnapshotPath);
            DeleteGeneratedMediaFile(mediaInfo.TempVideoPath);
        }
    }

    private void DeleteGeneratedMediaFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            var root = Path.GetFullPath(MessageSetting.OutputTempPath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            if (fullPath.StartsWith(root, StringComparison.Ordinal))
                System.IO.File.Delete(fullPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            Logger.LogWarning(exception, "Could not delete generated Chat media file {Path}", path);
        }
    }


    /// <summary>
    /// 保存到 BlobStore
    /// </summary>
    /// <param name="srcFilePath">原文件地址</param>
    /// <param name="fileName"></param>
    /// <param name="sessionUnit"></param>
    /// <param name="role"></param>
    /// <param name="parentBlobId"></param>
    /// <returns></returns>
    protected virtual async Task<Blob> UploadToBlobStoreAsync(
        string srcFilePath,
        string fileName,
        SessionUnitCacheItem sessionUnit = null,
        BlobRoles role = BlobRoles.Primary,
        Guid? parentBlobId = null)
    {
        using var file = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read);

        var bytes = await file.GetAllBytesAsync();

        string mimeType = MimeTypes.GetByExtension(srcFilePath);

        var blobId = GuidGenerator.Create();

        var suffix = Path.GetExtension(srcFilePath);

        if (sessionUnit != null)
        {
            return await SavePrivateChatTempBlobAsync(
                blobId,
                bytes,
                Path.GetFileName(fileName),
                mimeType,
                suffix,
                fileName,
                sessionUnit,
                role,
                parentBlobId);
        }

        var entity = await BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
        {
            IsPublic = false,
            FileSize = bytes.Length,
            MimeType = mimeType,
            FileName = Path.GetFileName(fileName),
            Name = fileName,
            Suffix = suffix,
            Bytes = bytes,
            ParentBlobId = parentBlobId,
            Role = role,
            Status = BlobStatus.Temp,
        });

        return entity;
    }

    public class SendTypingInput
    {
        public Guid SessionId { get; set; }
        public string SenderName { get; set; }
        public bool IsTyping { get; set; }
        public string Action { get; set; } = "typing";
        public string ActionText { get; set; }
    }

    /// <summary>
    /// 上报正在输入状态（瞬态，不落库）
    /// </summary>
    [HttpPost("send-typing/{sessionUnitId}")]
    [Authorize]
    [UnitOfWork(false)]
    public virtual async Task SendTypingAsync(Guid sessionUnitId, [FromBody] SendTypingInput input)
    {
        if (!CurrentUser.Id.HasValue || sessionUnitId == Guid.Empty || input == null)
        {
            return;
        }

        var unit = await SessionUnitManager.GetCacheAsync(sessionUnitId);
        if (unit?.SessionId == null)
        {
            return;
        }

        if (input.SessionId != Guid.Empty && unit.SessionId.Value != input.SessionId)
        {
            return;
        }

        var chatObjectManager = LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (!ownerIds.Contains(unit.OwnerId))
        {
            return;
        }

        var currentHosted = LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
        var distributedEventBus = LazyServiceProvider.LazyGetRequiredService<IDistributedEventBus>();
        var resolvedSessionId = unit.SessionId.Value;

        await distributedEventBus.PublishAsync(
            new SendMessageToClientDistributedEto
            {
                Command = CommandConsts.SessionTyping,
                HostName = currentHosted.Name,
                SessionId = resolvedSessionId,
                SenderSessionUnitId = unit.Id,
                SenderName = input.SenderName,
                IsTyping = input.IsTyping,
                Action = string.IsNullOrWhiteSpace(input.Action) ? "typing" : input.Action,
                ActionText = input.ActionText,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = null,
            },
            onUnitOfWorkComplete: false,
            useOutbox: false);
    }
}
