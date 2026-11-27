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

    protected virtual Task<Blob> SavePrivateChatTempBlobAsync(
        Guid blobId,
        byte[] bytes,
        string fileName,
        string contentType,
        string suffix,
        string blobName,
        SessionUnitCacheItem unit,
        BlobRoles role,
        Guid? parentBlobId = null)
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

        async Task<Blob> SaveImageAsync(Guid blobId, byte[] srcBytes, string name, int maxSize, bool isResize, BlobRoles role, Guid? parentBlobId = null)
        {
            byte[] resizedBytes = srcBytes;

            if (isResize)
            {
                var resizeder = await ImageResizer.ResizeAsync(srcBytes, new ImageResizeArgs()
                {
                    Width = (uint)maxSize,
                    Height = (uint)maxSize,
                    Mode = ImageResizeMode.Max
                });
                if (resizeder.State == ImageProcessState.Unsupported)
                    throw new Volo.Abp.UserFriendlyException("Unsupported image format.");
                resizedBytes = resizeder.Result;
            }

            return await SavePrivateChatTempBlobAsync(
                blobId,
                resizedBytes,
                file.FileName,
                file.ContentType,
                suffix,
                name,
                sessionUnit,
                role,
                parentBlobId);
        }

        var thumbnailImageSize = MessageSetting.ImageSetting.ThumbnailSize;

        await SaveImageAsync(
            thumbnailBlobId,
            bytes,
            $"{directoryName}/{filename}_{thumbnailImageSize}{suffix}",
            thumbnailImageSize,
            true,
            BlobRoles.Thumbnail,
            primaryBlobId);

        var imageContent = new ImageContentInfo()
        {
            BlobId = primaryBlobId,
            ThumbnailBlobId = thumbnailBlobId,
            ThumbnailUrl = await GetFileUrlAsync(thumbnailBlobId, file.ContentType),
            ContentType = file.ContentType,
            Suffix = suffix
        };

        var bigImageSize = MessageSetting.ImageSetting.BigSize;

        if (isOriginal)
        {
            using Image image = new(bytes);
            var maxSize = checked((int)Math.Max(image.Width, image.Height));

            await SaveImageAsync(
                primaryBlobId,
                bytes,
                $"{directoryName}/{filename}_original{suffix}",
                maxSize,
                false,
                BlobRoles.Primary);

            (int width, int height) = GetImageActualDimensions(image);

            imageContent.BlobId = primaryBlobId;
            imageContent.OriginalBlobId = primaryBlobId;
            imageContent.OriginalUrl = await GetFileUrlAsync(primaryBlobId, file.ContentType);

            imageContent.Width = width;

            imageContent.Height = height;

            imageContent.Size = bytes.Length;

            imageContent.Url = imageContent.OriginalUrl;

            imageContent.Profile = GetImageProfileJson(image);
        }
        else
        {
            var blob = await SaveImageAsync(
                primaryBlobId,
                bytes,
                $"{directoryName}/{filename}_{bigImageSize}{suffix}",
                bigImageSize,
                true,
                BlobRoles.Primary);

            using Image img = new(blob.Bytes);

            (int width, int height) = GetImageActualDimensions(img);

            double p = (double)width / height;

            imageContent.BlobId = primaryBlobId;

            imageContent.Width = p > 1 ? bigImageSize : Convert.ToInt32(bigImageSize * p);

            imageContent.Height = p > 1 ? Convert.ToInt32(bigImageSize / p) : bigImageSize;

            imageContent.Size = blob.Bytes.Length;

            imageContent.Url = await GetFileUrlAsync(primaryBlobId, file.ContentType);

            imageContent.Profile = GetImageProfileJson(img);
        }

        var sendResult = await MessageSenderAppService.SendImageAsync(sessionUnitId, new MessageInput<ImageContentInfo>()
        {
            QuoteMessageId = quoteMessageId,
            RemindList = remindList,
            Content = imageContent,
        });

        return new JsonResult(sendResult);
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
            var audioInfo = await MediaResolver.GetAudioInfoAsync(bytes, soundBlob.FileName);

            if (audioInfo != null)
            {
                content.Time = (int)audioInfo.Duration.Value.TotalMilliseconds;
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
            await GenerateSnapshotAsync(videoBlob, content, sessionUnit);
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
    /// 生成视频快照
    /// </summary>
    /// <param name="videoBlob"></param>
    /// <param name="content"></param>
    /// <param name="sessionUnit"></param>
    /// <returns></returns>
    protected virtual async Task GenerateSnapshotAsync(Blob videoBlob, VideoContentInfo content, SessionUnitCacheItem sessionUnit = null)
    {
        //var videoFileName = Path.GetFileNameWithoutExtension(videoBlob.Name);
        var videoFileName = videoBlob.Name;

        var directoryName = Path.GetDirectoryName(videoBlob.Name);

        var mediaInfo = await MediaResolver.GetVideoInfoAsync(videoBlob.Bytes, videoBlob.FileName);

        if (mediaInfo == null)
        {
            Logger.LogWarning($"GetVideoInfoAsync:videoBlobId:{videoBlob.Id} is null");
            return;
        }

        try
        {
            content.Width = mediaInfo.Width.GetValueOrDefault();
            content.Height = mediaInfo.Height.GetValueOrDefault();

            if (mediaInfo.Duration != null)
                content.Duration = mediaInfo.Duration.Value.TotalSeconds;

            if (!string.IsNullOrWhiteSpace(mediaInfo.ImageSnapshotPath))
            {
                var suffix = Path.GetExtension(mediaInfo.ImageSnapshotPath);
                var snapBlob = await UploadToBlobStoreAsync(
                    mediaInfo.ImageSnapshotPath,
                    $"{videoFileName}.snapshot{suffix}",
                    sessionUnit,
                    BlobRoles.Snapshot,
                    videoBlob.Id);
                content.SnapshotBlobId = snapBlob.Id;
                content.SnapshotUrl = await GetFileUrlAsync(snapBlob.Id, videoBlob.MimeType);

                using Image snapshotImg = new(snapBlob.Bytes);
                content.ImageWidth = checked((int)snapshotImg.Width);
                content.ImageHeight = checked((int)snapshotImg.Height);

                (int width, int height) = GetImageActualDimensions(snapshotImg);
                if (width > 0 && height > 0)
                {
                    content.Width = width;
                    content.Height = height;
                }

                var thumbnailSize = MessageSetting.VideoSetting.SnapshotThumbnailSize;
                var resizeder = await ImageResizer.ResizeAsync(snapBlob.Bytes, new ImageResizeArgs()
                {
                    Width = (uint)thumbnailSize,
                    Height = (uint)thumbnailSize,
                    Mode = ImageResizeMode.Max
                });

                if (resizeder.State == ImageProcessState.Unsupported)
                    throw new Volo.Abp.UserFriendlyException("Unsupported image format.");
                var thumbnailResizedBytes = resizeder.Result;

                var snapshotThumbnailBlobId = GuidGenerator.Create();
                var snapshotThumbnailBlob = sessionUnit != null
                    ? await SavePrivateChatTempBlobAsync(
                        snapshotThumbnailBlobId,
                        thumbnailResizedBytes,
                        $"{videoBlob.FileName}",
                        MimeTypes.GetByExtension(suffix),
                        suffix,
                        $"{videoFileName}.thumbnail{suffix}",
                        sessionUnit,
                        BlobRoles.SnapshotThumbnail,
                        videoBlob.Id)
                    : await BlobManager.CreateAsync(new Blob(snapshotThumbnailBlobId, ChatFilesContainer)
                    {
                        IsPublic = false,
                        FileSize = thumbnailResizedBytes.Length,
                        MimeType = MimeTypes.GetByExtension(suffix),
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
            }

            if (!string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath))
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
