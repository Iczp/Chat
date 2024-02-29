using IczpNet.Chat.Blobs.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Controllers
{
    [Route($"[Controller]")]
    public class FileController : ChatController
    {
        [HttpGet]
        [Route("{id}")]
        [Route("")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var blob = await BlobManager.GetAsync(id);

            if (blob == null)
            {
                return NotFound();
            }

            var bytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);

            return File(bytes, blob.MimeType, blob.FileName);
        }

        [HttpPost]
        public async Task<BlobDto> UploadAsync(IFormFile file, string container, string folder, bool isPublic)
        {
            return await UploadFileAsync(GuidGenerator.Create(), file, container, folder, isPublic);
        }

    }
}
