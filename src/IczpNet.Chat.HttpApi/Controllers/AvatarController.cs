using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Controllers;

[Route($"avatar")]
public class AvatarController : ChatController
{
    [HttpGet]
    [Route("")]
    public Task<IActionResult> GetAsync(string name, string size)
    {
        //var blob = await BlobManager.GetAsync(PortraitsContainer, name);

        //if (blob == null)
        //{
        //    return NotFound();
        //}

        //var bytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);

        //return File(bytes, blob.MimeType, blob.FileName);
        throw new NotImplementedException();
    }



}
