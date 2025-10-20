using IczpNet.AbpCommons.Utils;
using IczpNet.Chat.ChatObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace IczpNet.Chat.Controllers;

//[Route($"avatar")]
public class AvatarController(
    IPortraitService portraitService,
    IDistributedCache<byte[], string> cache) : ChatController
{
    protected IPortraitService PortraitService { get; } = portraitService;
    protected IDistributedCache<byte[], string> Cache { get; } = cache;

    protected virtual async Task<byte[]> GetPortraitByCacheAsync(long chatObjectId, bool hd, bool cache = true)
    {
        var cacheKey = $"{chatObjectId}-{hd}";

        if (!cache)
        {
            await Cache.RemoveAsync(cacheKey);
        }

        // 缓存中没有找到图片数据，从服务中获取
        return await Cache.GetOrAddAsync(cacheKey, async () =>
        {
            var bytes = await PortraitService.GetPortraitAsync(chatObjectId, !hd);

            if (bytes == null)
            {
                // 如果没有获取到图片数据，则返回一个空的 PNG 图片
                return ImageHelper.GetEmptyPngBytes(1, 1);
            }
            return bytes;
        });
    }

    [HttpGet]
    [AllowAnonymous]
    //[Route("")]
    public virtual async Task<IActionResult> GetAsync(long chatObjectId, bool hd = false, bool cache = true)
    {
        var bytes = await GetPortraitByCacheAsync(chatObjectId, hd, cache);

        return File(bytes, "image/png", enableRangeProcessing: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MargeAsync(IFormFile[] files, string outputPath = @"C:\\Users\\ZP\\Pictures\\5\\merge.png", int size = 640, int gap = 20, int cornerRadius = 180)
    {
        // 将上传的文件转换为图片的 byte[] 数组
        var imageBytesList = new List<byte[]>();

        foreach (var file in files)
        {
            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);
            var imageBytes = memoryStream.ToArray();
            imageBytesList.Add(imageBytes);
        }

        // 调用 MakeMergeThumbnails 方法合并图片
        var mergedImageBytes = ImageHelper.MakeMergeThumbnails(imageBytesList, size, gap, cornerRadius);

        // 保存合并后的图片到本地
        await System.IO.File.WriteAllBytesAsync(outputPath, mergedImageBytes);

        return Ok(outputPath);

    }
}
