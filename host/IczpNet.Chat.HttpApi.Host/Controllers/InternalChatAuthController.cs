using IczpNet.Chat.ChatObjects;
using IczpNet.AppDevice.Devices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.Controllers;

// AuthServer is a separate process. Keep the business operations in Chat and expose
// only the small integration surface it needs; this route is never gateway-proxied.
[ApiController]
[Route("internal/chat-auth")]
public class InternalChatAuthController(
    IConfiguration configuration,
    IChatObjectManager chatObjectManager,
    IDeviceManager deviceManager) : ControllerBase
{
    private bool IsAuthorized()
    {
        var expected = configuration["ChatInternal:ApiKey"];
        var provided = Request.Headers["X-Chat-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(expected) || expected.Length < 32 || string.IsNullOrWhiteSpace(provided))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)),
            SHA256.HashData(Encoding.UTF8.GetBytes(provided)));
    }

    [HttpPost("personal-chat-object")]
    public async Task<IActionResult> CreatePersonalChatObjectAsync([FromBody] PersonalChatObjectRequest input)
    {
        if (!IsAuthorized()) return Unauthorized();
        await chatObjectManager.CreatePersonalAsync(input.Name, input.Code, chatObject =>
        {
            chatObject.SetPortrait(input.Portrait);
            chatObject.SetThumbnail(input.Portrait);
            chatObject.SetAppUserId(input.UserId);
        });
        return NoContent();
    }

    [HttpPost("device-binding")]
    public async Task<IActionResult> BindDeviceAsync([FromBody] UserDeviceInput input)
    {
        if (!IsAuthorized()) return Unauthorized();
        await deviceManager.BindUserDeviceAsync(input);
        return NoContent();
    }
}

public class PersonalChatObjectRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Code { get; set; }

    public string Portrait { get; set; }

    public Guid UserId { get; set; }
}
