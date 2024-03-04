using IczpNet.Chat.Blobs;
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

namespace IczpNet.Chat.Controllers;

[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/message-sender")]
public class MessageSenderController : ChatController
{
    protected IMessageSenderAppService MessageSenderAppService { get; set; }
    protected ISessionUnitManager SessionUnitManager { get; set; }
    protected IOptions<ImageContentOptions> ImageResizeOption { get; set; }

    public MessageSenderController(
        ISessionUnitManager sessionUnitManager,
        IMessageSenderAppService messageSenderAppService,
        IOptions<ImageContentOptions> imageResizeOption)
    {
        SessionUnitManager = sessionUnitManager;
        MessageSenderAppService = messageSenderAppService;
        ImageResizeOption = imageResizeOption;
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

    [HttpPost]
    [Route("send-upload-image/{sessionUnitId}")]
    public async Task<IActionResult> SendUploadImageAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList, bool isOriginal)
    {
        var bytes = await file.GetAllBytesAsync();

        using Image image = Image.Load(bytes);

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

        var thumbnailImageSize = ImageResizeOption.Value.ThumbnailSize;

        await SaveImageAsync(thumbnailBlobId, bytes, $"{prefixName}_{thumbnailImageSize}{suffix}", thumbnailImageSize, true);

        var imageContent = new ImageContentInfo()
        {
            ThumbnailUrl = $"/file?id={thumbnailBlobId}",
            ContentType = file.ContentType,
            Suffix = suffix
        };

        var maxSize = new List<int> { image.Width, image.Height }.Max();

        var bigImageSize = ImageResizeOption.Value.BigSize;

        if (isOriginal || maxSize < bigImageSize)
        {
            //original
            var originalBlobId = GuidGenerator.Create();

            await SaveImageAsync(originalBlobId, bytes, $"{prefixName}_original{suffix}", maxSize, false);

            imageContent.Width = image.Width;

            imageContent.Height = image.Height;

            imageContent.Size = bytes.Length;

            imageContent.Url = $"/file?id={originalBlobId}";
        }
        else
        {
            //bigImage
            var bigImgBlobId = GuidGenerator.Create();

            var blob = await SaveImageAsync(bigImgBlobId, bytes, $"{prefixName}_{bigImageSize}{suffix}", bigImageSize, true);

            double p = (double)image.Width / image.Height;

            imageContent.Width = p > 1 ? bigImageSize : Convert.ToInt32(bigImageSize * p);

            imageContent.Height = p > 1 ? Convert.ToInt32(bigImageSize / p) : bigImageSize;

            imageContent.Size = blob.Bytes.Length;

            imageContent.Url = $"/file?id={bigImgBlobId}";
        }

        var sendResult = await MessageSenderAppService.SendImageAsync(sessionUnitId, new MessageInput<ImageContentInfo>()
        {
            QuoteMessageId = quoteMessageId,
            RemindList = remindList,
            Content = imageContent,
        });

        return new JsonResult(sendResult);
    }
}
