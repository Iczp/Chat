using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using IczpNet.AbpCommons;
using IczpNet.Pusher.ShortIds;
using System;
using Volo.Abp.Imaging;
using System.Collections.Generic;

namespace IczpNet.Chat;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/[Controller]/[Action]")]
[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
public abstract class ChatController : AbpControllerBase
{
    protected IShortIdGenerator ShortIdGenerator => LazyServiceProvider.LazyGetRequiredService<IShortIdGenerator>();
    protected IBlobManager BlobManager => LazyServiceProvider.LazyGetRequiredService<IBlobManager>();
    protected IImageResizer ImageResizer => LazyServiceProvider.LazyGetRequiredService<IImageResizer>();
    protected IImageCompressor ImageCompressor => LazyServiceProvider.LazyGetRequiredService<IImageCompressor>();

    public virtual string ChatFilesContainer => "chat-files";
    public virtual string EditorFilesContainer => "editor-files";
    public virtual string PortraitsContainer => "chat-object-portraits";
    public virtual string PkgFilesContainer => "pkg-files";

    protected ChatController()
    {
        LocalizationResource = typeof(ChatResource);
    }

    protected virtual bool IsImageMimeType(string mimeType)
    {
        var imageMimetypes = new List<string>() { "image/jpg", "image/jpeg", "image/gif", "image/png" };

        return imageMimetypes.Contains(mimeType);
    }
    protected virtual string GetExtension(string contentType, string fileName)
    {
        var suffix = Path.GetExtension(fileName);
        if (!string.IsNullOrWhiteSpace(suffix))
        {
            return suffix;
        }

        switch (contentType)
        {
            case "image/jpeg":
            case "image/jpg":
                suffix = ".jpg";
                break;
            case "image/png":
                suffix = ".png";
                break;
            case "image/gif":
                suffix = ".gif";
                break;
        }
        return suffix;

    }

    protected virtual async Task<BlobDto> UploadFileAsync(Guid blobId, IFormFile file, string container, string folder, bool isPublic)
    {
        Assert.If(file == null, "No file found!");

        var suffix = GetExtension(file.ContentType, file.FileName);

        var bytes = await file.GetAllBytesAsync();

        var entity = await BlobManager.CreateAsync(new Blob(blobId, container)
        {
            IsPublic = isPublic,
            FileSize = bytes.Length,
            MimeType = file.ContentType,
            FileName = file.FileName,
            Name = $"{folder}/{GuidGenerator.Create()}{suffix}",
            Suffix = Path.GetExtension(file.FileName),
        }, bytes);

        return ObjectMapper.Map<Blob, BlobDto>(entity);
    }

    /// <summary>
    /// 保存头像
    /// </summary>
    /// <param name="file"></param>
    /// <param name="chatObjectId"></param>
    /// <param name="thumbnailBlobId"></param>
    /// <param name="bigImgBlobId"></param>
    /// <returns></returns>
    protected async Task SavePortraitAsync(IFormFile file, long chatObjectId, Guid thumbnailBlobId, Guid bigImgBlobId)
    {
        var bytes = await file.GetAllBytesAsync();

        var suffix = GetExtension(file.ContentType, file.FileName);

        //var compressorBytes = await ImageCompressor.CompressAsync(resizedBytes.Result);
        async Task<Blob> SaveFileAsync(Guid blobId, byte[] bytes, string name, int size)
        {
            var resizedBytes = await ImageResizer.ResizeAsync(bytes, new ImageResizeArgs()
            {
                Width = size,
                Height = size,
                Mode = ImageResizeMode.Crop
            });

            var entity = await BlobManager.CreateAsync(new Blob(blobId, PortraitsContainer)
            {
                IsPublic = true,
                FileSize = resizedBytes.Result.Length,
                MimeType = file.ContentType,
                FileName = file.FileName,
                Name = name,
                Suffix = suffix,
            }, resizedBytes.Result);
            return entity;
        }

        var randomName = GuidGenerator.Create();

        //thumbnail
        await SaveFileAsync(thumbnailBlobId, bytes, $"{chatObjectId}/{randomName}_128{suffix}", 128);

        //bigImage
        await SaveFileAsync(bigImgBlobId, bytes, $"{chatObjectId}/{randomName}_540{suffix}", 540);
    }


}
