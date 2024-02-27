using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using IczpNet.AbpCommons;

namespace IczpNet.Chat;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/[Controller]/[Action]")]
[ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
public abstract class ChatController : AbpControllerBase
{

    protected IBlobManager BlobManager => LazyServiceProvider.LazyGetRequiredService<IBlobManager>();
    public virtual string ChatFilesContainer => "chat-files";
    public virtual string EditorFilesContainer => "editor-files";
    public virtual string UserPortraitsContainer => "user-portraits";
    public virtual string RoomPortraitsContainer => "room-portraits";
    public virtual string PkgFilesContainer => "pkg-files";

    protected ChatController()
    {
        LocalizationResource = typeof(ChatResource);
    }

    protected virtual async Task<BlobDto> UploadFileAsync(IFormFile file, string container, string folder, bool isPublic)
    {
        Assert.If(file == null, "No file found!");

        var suffix = Path.GetExtension(file.FileName);

        var bytes = await file.GetAllBytesAsync();

        var entity = await BlobManager.CreateAsync(new Blob(GuidGenerator.Create(), container)
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
}
