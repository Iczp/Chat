using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;

namespace IczpNet.Chat.Controllers;

[Route($"[Controller]")]
public class FileController : ChatController
{
    [HttpGet]
    [Route("{id}")]
    [Route("")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var blob = await BlobManager.FindAsync(id);

        if (blob == null)
        {
            return NotFound();
        }

        var bytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);

        //PhysicalFile
        return File(bytes, blob.MimeType, blob.FileName, enableRangeProcessing: true);
    }

    [HttpPost]
    public async Task<BlobDto> UploadAsync(IFormFile file, string container, string folder, bool isPublic)
    {
        var entity = await UploadFileAsync(GuidGenerator.Create(), file, container, folder, isPublic);

        return ObjectMapper.Map<Blob, BlobDto>(entity);
    }

    

}
