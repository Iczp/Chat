using IczpNet.AbpCommons.Utils;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects;

public class PortraitService : DomainService, IPortraitService
{
    protected IChatObjectManager ChatObjectManager { get; }
    protected IReadOnlyRepository<SessionUnit, Guid> SessionUintRepository { get; }
    protected IBlobManager BlobManager { get; }

    protected MessageOptions MessageSetting { get; set; }

    public PortraitService(IChatObjectManager chatObjectManager,
        IReadOnlyRepository<SessionUnit, Guid> sessionUintRepository,
        IBlobManager blobManager,
        IOptions<MessageOptions> imageSetting)
    {
        ChatObjectManager = chatObjectManager;
        SessionUintRepository = sessionUintRepository;
        BlobManager = blobManager;
        MessageSetting = imageSetting.Value;
    }


    public async Task<byte[]> GetPortraitAsync(long chatObjectId, bool isThumbnail)
    {
        var entity = await ChatObjectManager.GetAsync(chatObjectId);

        var size = isThumbnail ? MessageSetting.ImageSetting.ThumbnailSize : MessageSetting.ImageSetting.BigSize;

        if (entity.ObjectType == ChatObjectTypeEnums.Room)
        {
            var gap = size / 32;

            var cornerRadius = (size - gap * 4) / 3;

           var imgBytes = await GetRoomPortraitAsync(chatObjectId, size, gap, cornerRadius);

            if (imgBytes != null)
            {
                return imgBytes;
            }
        }

        return await GetChatObjectPortraitAsync(entity, isThumbnail);
    }

    protected virtual async Task<byte[]> GetChatObjectPortraitAsync(ChatObject entity, bool isThumbnail)
    {
        var portrait = isThumbnail ? entity.Thumbnail : entity.Portrait;

        if (string.IsNullOrWhiteSpace(portrait))
        {
            return null;
        }
        var blobId = await GetBlobIdFromPortraitAsync(portrait);

        if (blobId == null)
        {
            return null;
        }
        var blob = await BlobManager.FindAsync(blobId.Value);

        if (blob == null)
        {
            return null;
        }

        return await BlobManager.GetBytesAsync(blob.Container, blob.Name);
    }

    protected virtual async Task<Guid?> GetBlobIdFromPortraitAsync(string portrait)
    {
        await Task.Yield();

        var parameters = GetParametersFromUrl(portrait);

        if (parameters != null && Guid.TryParse(parameters["id"], out Guid id))
        {
            return id;
        }
        return null;
    }

    /// <summary>
    /// 合成群头像
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="size"></param>
    /// <param name="gap"></param>
    /// <param name="cornerRadius"></param>
    /// <returns></returns>
    protected virtual async Task<byte[]> GetRoomPortraitAsync(long roomId, int size = 640, int gap = 10, int cornerRadius = 10)
    {
        var portraits = (await SessionUintRepository.GetQueryableAsync())
            .Where(x => x.DestinationId == roomId)
            .Where(x => x.OwnerId != roomId)
            .Where(x => x.Setting.IsEnabled && x.Setting.IsPublic && !x.Setting.IsKilled)
            .Where(x => x.Owner.IsEnabled)
            .Where(x => x.Owner.Portrait.StartsWith(@"/file?"))
            .Select(x => x.Owner.Portrait)
            .Take(9)
            .ToList();

        var blobIdList = new List<Guid>();

        foreach (var portrait in portraits)
        {
            var blobId = await GetBlobIdFromPortraitAsync(portrait);

            if (blobId != null)
            {
                blobIdList.Add(blobId.Value);
            }
        }

        var imageBytesList = new List<byte[]>();

        foreach (var blobId in blobIdList)
        {
            var blob = await BlobManager.FindAsync(blobId);

            if (blob == null)
            {
                continue;
            }

            var bytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);

            imageBytesList.Add(bytes);
        }

        if (imageBytesList.Count == 0)
        {
            return null;
        }

        // 调用 MakeMergeThumbnails 方法合并图片
        var mergedImageBytes = ImageHelper.MakeMergeThumbnails(imageBytesList, size, gap, cornerRadius);

        //// 保存合并后的图片到本地
        //await System.IO.File.WriteAllBytesAsync(outputPath, mergedImageBytes);

        return mergedImageBytes;
    }

    protected virtual NameValueCollection GetParametersFromUrl(string url)
    {
        try
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                // 如果是相对路径，则尝试将其转换为绝对路径
                string baseUrl = "https://www.iczp.net";

                var baseUri = new Uri(baseUrl);

                uri = new Uri(baseUri, url);
            }
            // 获取查询参数
            string query = uri.Query;

            // 使用 HttpUtility.ParseQueryString 解析查询参数为 NameValueCollection
            var parameters = HttpUtility.ParseQueryString(query);

            return parameters;
        }
        catch (UriFormatException)
        {
            // URL 格式不正确
            return null;
        }
    }


}
