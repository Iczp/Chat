using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat
{
    [Route($"[Controller]")]
    public class BlobController : ChatController
    {
        [HttpGet]
        public async Task<string> GetAsync(string id)
        {
            await Task.Yield();
            return id;
        }

        [HttpPost]
        public async Task<string> UploadFileAsync(IFormFile file, string container, string folder)
        {

            if (file == null)
            {
                throw new UserFriendlyException("No file found!");
            }

            var suffix = Path.GetExtension(file.FileName);

            var bytes = await file.GetAllBytesAsync();

            return $"ContentType={file.ContentType},FileName={file.FileName},bytes=${bytes.Length}";
        }


    }
}
