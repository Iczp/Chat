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

    /// <summary>
    /// 上传并发送(file/sound/image/video)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="quoteMessageId">引消息Id</param>
    /// <param name="remindList">提醒（@XXX）</param>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("send-upload-file/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadFileAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList)
    {
        var sessionUnit = await SessionUnitManager.GetCacheAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        //var directoryName = $"{DateDirectoryName}/files/{sessionUnit.SessionId}/{sessionUnitId}";

        //var directoryName = await BlobResolver.GetDirectoryNameAsync(ChatFilesContainer, "files", blobId, sessionUnit.SessionId.Value, sessionUnitId);

        var directoryName = await GetDirectoryNameAsync(blobId, "files", sessionUnit, file);

        var blob = await UploadFileAsync(blobId, file, ChatFilesContainer, directoryName, false);

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
                //Url = $"/file?id={blob.Id}",
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
    [Route("send-upload-image/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadImageAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        var bytes = await file.GetAllBytesAsync();

        var suffix = GetExtension(file.ContentType, file.FileName);

        // SaveImageAsync
        async Task<Blob> SaveImageAsync(Guid blobId, byte[] bytes, string name, int maxSize, bool isResize)
        {
            byte[] resizedBytes = bytes;

            if (isResize)
            {
                var resizeder = await ImageResizer.ResizeAsync(bytes, new ImageResizeArgs()
                {
                    Width = (uint)maxSize,
                    Height = (uint)maxSize,
                    Mode = ImageResizeMode.Max
                });
                if (resizeder.State == ImageProcessState.Unsupported)
                    throw new Volo.Abp.UserFriendlyException("Unsupported image format.");
                resizedBytes = resizeder.Result;
            }

            var entity = await BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
            {
                IsPublic = true,
                FileSize = resizedBytes.Length,
                MimeType = file.ContentType,
                FileName = file.FileName,
                Name = name,
                Suffix = suffix,
                Bytes = resizedBytes
            });
            return entity;
        }

        var sessionUnit = await SessionUnitManager.GetCacheAsync(sessionUnitId);

        var filename = $"{Clock.Now:yyyyMMddhhmmss}_{ShortIdGenerator.Create()}".ToLower();
        //thumbnail
        var thumbnailBlobId = GuidGenerator.Create();

        var directoryName = await GetDirectoryNameAsync(thumbnailBlobId, "images", sessionUnit, file);

        var thumbnailImageSize = MessageSetting.ImageSetting.ThumbnailSize;

        await SaveImageAsync(thumbnailBlobId, bytes, $"{directoryName}/{filename}_{thumbnailImageSize}{suffix}", thumbnailImageSize, true);

        var imageContent = new ImageContentInfo()
        {
            BlobId = thumbnailBlobId,
            ThumbnailUrl = await GetFileUrlAsync(thumbnailBlobId, file.ContentType),
            ContentType = file.ContentType,
            Suffix = suffix
        };

        //using var image1 = System.Drawing.Image.FromStream(file.OpenReadStream());

        var bigImageSize = MessageSetting.ImageSetting.BigSize;

        if (isOriginal)
        {
            using Image image = new(bytes);
            //original
            var originalBlobId = GuidGenerator.Create();

            var maxSize = checked((int)Math.Max(image.Width, image.Height));

            await SaveImageAsync(originalBlobId, bytes, $"{directoryName}/{filename}_original{suffix}", maxSize, false);

            (int width, int height) = GetImageActualDimensions(image);

            imageContent.BlobId = originalBlobId;

            imageContent.Width = width;

            imageContent.Height = height;

            imageContent.Size = bytes.Length;

            imageContent.Url = await GetFileUrlAsync(originalBlobId, file.ContentType);

            imageContent.Profile = GetImageProfileJson(image);
        }
        else
        {
            //bigImage
            var bigImgBlobId = GuidGenerator.Create();

            var blob = await SaveImageAsync(bigImgBlobId, bytes, $"{directoryName}/{filename}_{bigImageSize}{suffix}", bigImageSize, true);

            using Image img = new(blob.Bytes);

            (int width, int height) = GetImageActualDimensions(img);

            double p = (double)width / height;

            imageContent.BlobId = bigImgBlobId;

            imageContent.Width = p > 1 ? bigImageSize : Convert.ToInt32(bigImageSize * p);

            imageContent.Height = p > 1 ? Convert.ToInt32(bigImageSize / p) : bigImageSize;

            imageContent.Size = blob.Bytes.Length;

            imageContent.Url = await GetFileUrlAsync(bigImgBlobId, file.ContentType);

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
    [Route("send-upload-sound/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadSoundAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, [FromForm] int? duration)
    {
        var sessionUnit = await SessionUnitManager.GetCacheAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        var directoryName = await GetDirectoryNameAsync(blobId, "audios", sessionUnit, file);

        var soundBlob = await UploadFileAsync(blobId, file, ChatFilesContainer, directoryName, false);

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
            var audioInfo = await MediaResolver.GetAudioInfoAsync(soundBlob.Bytes, soundBlob.FileName);

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
    [Route("send-upload-video/{sessionUnitId}")]
    [UnitOfWork(false)]
    public async Task<IActionResult> SendUploadVideoAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        var sessionUnit = await SessionUnitManager.GetCacheAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        var directoryName = await GetDirectoryNameAsync(blobId, "videos", sessionUnit, file);

        var videoBlob = await UploadFileAsync(blobId, file, ChatFilesContainer, directoryName, false);

        var content = new VideoContentInfo()
        {
            BlobId = videoBlob.Id,
            ContentType = file.ContentType,
            Size = file.Length,
            FileName = Path.GetFileName(file.FileName),
            Suffix = Path.GetExtension(file.FileName),
            //Url = $"/file?id={videoBlob.Id}"
            Url = await GetFileUrlAsync(videoBlob.Id, file.ContentType)
        };

        if (MessageSetting.VideoSetting.IsGenerateSnapshot)
        {
            await GenerateSnapshotAsync(videoBlob, content);
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
    /// <returns></returns>
    protected virtual async Task GenerateSnapshotAsync(Blob videoBlob, VideoContentInfo content)
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
                var snapBlob = await UploadToBlobStoreAsync(mediaInfo.ImageSnapshotPath, $"{videoFileName}.snapshot{suffix}");
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
                var snapshotThumbnailBlob = await BlobManager.CreateAsync(new Blob(snapshotThumbnailBlobId, ChatFilesContainer)
                {
                    IsPublic = true,
                    FileSize = thumbnailResizedBytes.Length,
                    MimeType = MimeTypes.GetByExtension(suffix),
                    FileName = $"{videoBlob.FileName}",
                    Name = $"{videoFileName}.thumbnail{suffix}",
                    Suffix = suffix,
                    Bytes = thumbnailResizedBytes
                });

                content.SnapshotThumbnailUrl = await GetFileUrlAsync(snapshotThumbnailBlob.Id, snapshotThumbnailBlob.MimeType);
            }

            if (!string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath))
            {
                var gifBlob = await UploadToBlobStoreAsync(mediaInfo.GifSnapshotPath, $"{videoFileName}.snapshot.gif");
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
    /// <returns></returns>
    protected virtual async Task<Blob> UploadToBlobStoreAsync(string srcFilePath, string fileName)
    {
        using var file = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read);

        var bytes = await file.GetAllBytesAsync();

        string mimeType = MimeTypes.GetByExtension(srcFilePath);

        var blobId = GuidGenerator.Create();

        var suffix = Path.GetExtension(srcFilePath);

        //var fileName = Path.GetFileName(srcFilePath);

        var entity = await BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
        {
            IsPublic = true,
            FileSize = bytes.Length,
            MimeType = MimeTypes.GetByExtension(srcFilePath),
            FileName = Path.GetFileName(fileName),
            Name = fileName,
            Suffix = suffix,
            Bytes = bytes
        });

        return entity;
    }
}
