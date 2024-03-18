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
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Extensions.Logging;
namespace IczpNet.Chat.Controllers;

[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/message-sender")]
public class MessageSenderController : ChatController
{
    protected IMessageSenderAppService MessageSenderAppService { get; set; }
    protected ISessionUnitManager SessionUnitManager { get; set; }
    protected MessageOptions MessageSetting { get; set; }
    protected IMediaResolver MediaResolver { get; set; }
    protected IJsonSerializer JsonSerializer { get; set; }

    public MessageSenderController(
            ISessionUnitManager sessionUnitManager,
            IMessageSenderAppService messageSenderAppService,
            IOptions<MessageOptions> imageSetting,
            IJsonSerializer jsonSerializer,
            IMediaResolver mediaResolver)
    {
        SessionUnitManager = sessionUnitManager;
        MessageSenderAppService = messageSenderAppService;
        MessageSetting = imageSetting.Value;
        JsonSerializer = jsonSerializer;
        MediaResolver = mediaResolver;
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
    [Route("send-upload/{sessionUnitId}")]
    public async Task<IActionResult> UploadToSendAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        var blob = await UploadFileAsync(GuidGenerator.Create(), file, ChatFilesContainer, $"{sessionUnit.SessionId}/{sessionUnitId}", false);

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
                Url = $"/file?id={blob.Id}",
            }
        });
        return new JsonResult(sendResult);
    }

    /// <summary>
    /// 获取真实宽高
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    protected static (int width, int height) GetActualDimensions(Image image)
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

    protected static List<ImageProfileEntry> GetProfile(Image image)
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

    protected string GetProfileJson(Image image, List<ImageProfileEntry> otherEntries = null)
    {
        var profileEntries = GetProfile(image);
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

        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        var prefixName = $"{sessionUnit.SessionId}/{sessionUnitId}/images/{GuidGenerator.Create()}";

        //thumbnail
        var thumbnailBlobId = GuidGenerator.Create();

        var thumbnailImageSize = MessageSetting.ImageSetting.ThumbnailSize;

        await SaveImageAsync(thumbnailBlobId, bytes, $"{prefixName}_{thumbnailImageSize}{suffix}", thumbnailImageSize, true);

        var imageContent = new ImageContentInfo()
        {
            ThumbnailUrl = $"/file?id={thumbnailBlobId}",
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

            await SaveImageAsync(originalBlobId, bytes, $"{prefixName}_original{suffix}", maxSize, false);

            (int width, int height) = GetActualDimensions(image);

            imageContent.Width = width;

            imageContent.Height = height;

            imageContent.Size = bytes.Length;

            imageContent.Url = $"/file?id={originalBlobId}";

            imageContent.Profile = GetProfileJson(image);
        }
        else
        {
            //bigImage
            var bigImgBlobId = GuidGenerator.Create();

            var blob = await SaveImageAsync(bigImgBlobId, bytes, $"{prefixName}_{bigImageSize}{suffix}", bigImageSize, true);

            using Image img = Image.Load(blob.Bytes);

            (int width, int height) = GetActualDimensions(img);

            double p = (double)width / height;

            imageContent.Width = p > 1 ? bigImageSize : Convert.ToInt32(bigImageSize * p);

            imageContent.Height = p > 1 ? Convert.ToInt32(bigImageSize / p) : bigImageSize;

            imageContent.Size = blob.Bytes.Length;

            imageContent.Url = $"/file?id={bigImgBlobId}";

            imageContent.Profile = GetProfileJson(img);
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
    [Route("send-upload-video/{sessionUnitId}")]
    public async Task<IActionResult> SendUploadVideoAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        var prefixName = $"{sessionUnit.SessionId}/{sessionUnitId}/Video/";

        var videoBlob = await UploadFileAsync(GuidGenerator.Create(), file, ChatFilesContainer, prefixName, false);

        var content = new VideoContentInfo()
        {
            ContentType = file.ContentType,
            Size = file.Length,
            FileName = Path.GetFileName(file.FileName),
            Suffix = Path.GetExtension(file.FileName),
            Url = $"/file?id={videoBlob.Id}"
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
        var videoFileName = Path.GetFileNameWithoutExtension(videoBlob.Name);

        var prefixName = Path.GetDirectoryName(videoBlob.Name);

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
            var snapBlob = await UploadToBlobStoreAsync(mediaInfo.ImageSnapshotPath, $"{prefixName}{videoFileName}_snapshot{suffix}");

            content.SnapshotUrl = $"/file?id={snapBlob.Id}";

            // Actual width | height
            using Image snapshotImg = Image.Load(snapBlob.Bytes);

            content.Width = snapshotImg.Width;

            content.Height = snapshotImg.Height;

            (int width, int height) = GetActualDimensions(snapshotImg);

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
                FileName = videoBlob.FileName,
                Name = $"{prefixName}{videoFileName}_thumbnail{suffix}",
                Suffix = suffix,
                Bytes = thumbnailResizedBytes
            });

            content.SnapshotThumbnailUrl = $"/file?id={snapshotThumbnailBlob.Id}";
        }
        //GifSnapshot
        if (!string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath))
        {
            var gifBlob = await UploadToBlobStoreAsync(mediaInfo.GifSnapshotPath, $"{prefixName}{videoFileName}_snapshot.gif");
            content.GifUrl = $"/file?id={gifBlob.Id}";
        }

    }


    protected async Task<Blob> UploadToBlobStoreAsync(string filePath, string fileName)
    {
        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var bytes = await file.GetAllBytesAsync();

        string mimeType = MimeTypes.GetByExtension(filePath);

        var blobId = GuidGenerator.Create();

        var suffix = Path.GetExtension(filePath);

        //var fileName = Path.GetFileName(filePath);

        var entity = await BlobManager.CreateAsync(new Blob(blobId, ChatFilesContainer)
        {
            IsPublic = true,
            FileSize = bytes.Length,
            MimeType = MimeTypes.GetByExtension(filePath),
            FileName = fileName,
            Name = fileName,
            Suffix = suffix,
            Bytes = bytes
        });

        return entity;
    }
}
