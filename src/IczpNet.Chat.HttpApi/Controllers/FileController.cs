using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
            return BadRequest(new { message = "blob is null" });
        }
        try
        {
            var bytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);
            //PhysicalFile
            return File(bytes, blob.MimeType, blob.FileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Blob.ID={blob.Id},Container={blob.Container},Name={blob.Name},MimeType={blob.MimeType},FileName={blob.FileName}");
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<BlobDto> UploadAsync(IFormFile file, string container, string folder, bool isPublic)
    {
        var entity = await UploadFileAsync(GuidGenerator.Create(), file, container, folder, isPublic);

        return ObjectMapper.Map<Blob, BlobDto>(entity);
    }
}
