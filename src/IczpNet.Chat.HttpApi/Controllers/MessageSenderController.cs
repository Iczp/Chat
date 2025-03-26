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
using Image = SixLabors.ImageSharp.Image;
using System.Linq;
using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Volo.Abp.Json;
using IczpNet.Chat.Medias;
using Microsoft.CodeAnalysis;
using Volo.Abp.Http;
using Blob = IczpNet.Chat.Blobs.Blob;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.MessageSections;
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
    /// 上传并发送(file/sound/image/video)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="quoteMessageId">引消息Id</param>
    /// <param name="remindList">提醒（@XXX）</param>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("send-upload-file/{sessionUnitId}")]
    public async Task<IActionResult> SendUploadFileAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList)
    {
        var sessionUnit = await SessionUnitManager.GetByCacheAsync(sessionUnitId);

        var blobId = GuidGenerator.Create();

        //var directoryName = $"{DateDirectoryName}/files/{sessionUnit.SessionId}/{sessionUnitId}";

        var directoryName = await BlobResolver.GetDirectoryNameAsync(ChatFilesContainer, "files", blobId, sessionUnit.SessionId.Value, sessionUnitId);

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
                Url = await BlobResolver.GetFileUrlAsync(blob.Id),
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
        int width = image.Width;

        int height = image.Height;

        // Check if image has Exif metadata
        if (image.Metadata.ExifProfile != null)
        {
            var item = image.Metadata.ExifProfile.Values.FirstOrDefault(x => x.Tag == ExifTag.Orientation);

            if (item != null)
            {
                var orientation = (ushort)item.GetValue();

                if (orientation == 6 || orientation == 8) // 6: Rotate 90 degrees CW, 8: Rotate 90 degrees CCW
                {
                    // Swap width and height if rotated 90 or 270 degrees
                    (height, width) = (width, height);
                }
            }
        }
        return (width, height);
    }

    protected virtual List<ImageProfileEntry> GetImageProfile(Image image)
    {
        var items = new List<ImageProfileEntry>();

        if (image.Metadata.ExifProfile != null)
        {
            foreach (var exifValue in image.Metadata.ExifProfile.Values)
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

    [HttpPost]
    [Route("send-upload-image/{sessionUnitId}")]
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
                    Width = maxSize,
                    Height = maxSize,
                    Mode = ImageResizeMode.Max
                });
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

        var sessionUnit = await SessionUnitManager.GetByCacheAsync(sessionUnitId);

        //var directoryName = $"{DateDirectoryName}/images/{sessionUnit.SessionId}/{sessionUnitId}";

        var filename = $"{Clock.Now:yyyyMMddhhmmss}_{ShortIdGenerator.Create()}".ToLower();
        //thumbnail
        var thumbnailBlobId = GuidGenerator.Create();

        var directoryName = await BlobResolver.GetDirectoryNameAsync(ChatFilesContainer, "images", thumbnailBlobId, sessionUnit.SessionId.Value, sessionUnitId);

        var thumbnailImageSize = MessageSetting.ImageSetting.ThumbnailSize;

        await SaveImageAsync(thumbnailBlobId, bytes, $"{directoryName}/{filename}_{thumbnailImageSize}{suffix}", thumbnailImageSize, true);

        var imageContent = new ImageContentInfo()
        {
            BlobId = thumbnailBlobId,
            ThumbnailUrl = await BlobResolver.GetFileUrlAsync(thumbnailBlobId),
            ContentType = file.ContentType,
            Suffix = suffix
        };

        //using var image1 = System.Drawing.Image.FromStream(file.OpenReadStream());

        var bigImageSize = MessageSetting.ImageSetting.BigSize;

        if (isOriginal)
        {
            using Image image = Image.Load(bytes);
            //original
            var originalBlobId = GuidGenerator.Create();

            var maxSize = new List<int> { image.Width, image.Height }.Max();

            await SaveImageAsync(originalBlobId, bytes, $"{directoryName}/{filename}_original{suffix}", maxSize, false);

            (int width, int height) = GetImageActualDimensions(image);

            imageContent.BlobId = originalBlobId;

            imageContent.Width = width;

            imageContent.Height = height;

            imageContent.Size = bytes.Length;

            imageContent.Url = await BlobResolver.GetFileUrlAsync(originalBlobId);

            imageContent.Profile = GetImageProfileJson(image);
        }
        else
        {
            //bigImage
            var bigImgBlobId = GuidGenerator.Create();

            var blob = await SaveImageAsync(bigImgBlobId, bytes, $"{directoryName}/{filename}_{bigImageSize}{suffix}", bigImageSize, true);

            using Image img = Image.Load(blob.Bytes);

            (int width, int height) = GetImageActualDimensions(img);

            double p = (double)width / height;

            imageContent.BlobId = bigImgBlobId;

            imageContent.Width = p > 1 ? bigImageSize : Convert.ToInt32(bigImageSize * p);

            imageContent.Height = p > 1 ? Convert.ToInt32(bigImageSize / p) : bigImageSize;

            imageContent.Size = blob.Bytes.Length;

            imageContent.Url = await BlobResolver.GetFileUrlAsync(bigImgBlobId);

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
    public async Task<IActionResult> SendUploadSoundAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, [FromForm] int? duration)
    {
        var sessionUnit = await SessionUnitManager.GetByCacheAsync(sessionUnitId);

        //var directoryName = $"{DateDirectoryName}/audios/{sessionUnit.SessionId}/{sessionUnitId}";
        var blobId = GuidGenerator.Create();

        var directoryName = await BlobResolver.GetDirectoryNameAsync(ChatFilesContainer, "audios", blobId, sessionUnit.SessionId.Value, sessionUnitId);

        var soundBlob = await UploadFileAsync(blobId, file, ChatFilesContainer, directoryName, false);

        var content = new SoundContentInfo()
        {
            BlobId = soundBlob.Id,
            ContentType = file.ContentType,
            Size = file.Length,
            FileName = Path.GetFileName(file.FileName),
            Suffix = Path.GetExtension(file.FileName),
            //Url = $"/file?id={soundBlob.Id}",
            Url = await BlobResolver.GetFileUrlAsync(soundBlob.Id),
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
    public async Task<IActionResult> SendUploadVideoAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        var sessionUnit = await SessionUnitManager.GetByCacheAsync(sessionUnitId);

        //var directoryName = $"{DateDirectoryName}/videos/{sessionUnit.SessionId}/{sessionUnitId}";

        var blobId = GuidGenerator.Create();

        var directoryName = await BlobResolver.GetDirectoryNameAsync(ChatFilesContainer, "videos", blobId, sessionUnit.SessionId.Value, sessionUnitId);

        var videoBlob = await UploadFileAsync(blobId, file, ChatFilesContainer, directoryName, false);

        var content = new VideoContentInfo()
        {
            BlobId = videoBlob.Id,
            ContentType = file.ContentType,
            Size = file.Length,
            FileName = Path.GetFileName(file.FileName),
            Suffix = Path.GetExtension(file.FileName),
            //Url = $"/file?id={videoBlob.Id}"
            Url = await BlobResolver.GetFileUrlAsync(videoBlob.Id)
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
        content.Width = mediaInfo.Width.GetValueOrDefault();

        content.Height = mediaInfo.Height.GetValueOrDefault();

        //Duration
        if (mediaInfo.Duration != null)
        {
            content.Duration = mediaInfo.Duration.Value.TotalSeconds;
        }

        //Snapshot
        if (!string.IsNullOrWhiteSpace(mediaInfo.ImageSnapshotPath))
        {
            var suffix = Path.GetExtension(mediaInfo.ImageSnapshotPath);

            //Snapshot
            var snapBlob = await UploadToBlobStoreAsync(mediaInfo.ImageSnapshotPath, $"{videoFileName}.snapshot{suffix}");

            //content.SnapshotUrl = $"/file?id={snapBlob.Id}";

            content.SnapshotUrl = await BlobResolver.GetFileUrlAsync(snapBlob.Id);

            // Actual width | height
            using Image snapshotImg = Image.Load(snapBlob.Bytes);

            content.ImageWidth = snapshotImg.Width;

            content.ImageHeight = snapshotImg.Height;

            (int width, int height) = GetImageActualDimensions(snapshotImg);

            // reset video width/height 重设视频的宽高
            if (width > 0 && height > 0)
            {
                content.Width = width;
                content.Height = height;
            }

            //Snapshot thumbnail Size
            var thumbnailSize = MessageSetting.VideoSetting.SnapshotThumbnailSize;

            var resizeder = await ImageResizer.ResizeAsync(snapBlob.Bytes, new ImageResizeArgs()
            {
                Width = thumbnailSize,
                Height = thumbnailSize,
                Mode = ImageResizeMode.Max
            });

            var thumbnailResizedBytes = resizeder.Result;

            var snapshotThumbnailBlobId = GuidGenerator.Create();

            //snapshotThumbnailBlob
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

            //content.SnapshotThumbnailUrl = $"/file?id={snapshotThumbnailBlob.Id}";
            content.SnapshotThumbnailUrl = await BlobResolver.GetFileUrlAsync(snapshotThumbnailBlob.Id);
        }
        //GifSnapshot
        if (!string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath))
        {
            var gifBlob = await UploadToBlobStoreAsync(mediaInfo.GifSnapshotPath, $"{videoFileName}.snapshot.gif");
            //content.GifUrl = $"/file?id={gifBlob.Id}";
            content.GifUrl = await BlobResolver.GetFileUrlAsync(gifBlob.Id);
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
