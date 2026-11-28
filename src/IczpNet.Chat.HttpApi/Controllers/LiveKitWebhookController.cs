using IczpNet.Chat.Calls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/chat/call/livekit-webhook")]
[AllowAnonymous]
public class LiveKitWebhookController(
    ILiveKitClient liveKitClient,
    CallSessionManager callSessionManager,
    IRepository<CallWebhookEvent, string> webhookEventRepository) : AbpControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        var auth = Request.Headers.Authorization.ToString();
        if (!liveKitClient.ValidateWebhookSignature(auth, rawBody))
            return Unauthorized();

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;
        var eventId = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var eventType = root.TryGetProperty("event", out var evEl) ? evEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventType))
            return Ok();

        if (await webhookEventRepository.FindAsync(eventId, includeDetails: false) != null)
            return Ok();

        var roomName = root.TryGetProperty("room", out var rEl) && rEl.TryGetProperty("name", out var rnEl) ? rnEl.GetString() : null;
        var roomSid = root.TryGetProperty("room", out rEl) && rEl.TryGetProperty("sid", out var rsEl) ? rsEl.GetString() : null;

        await webhookEventRepository.InsertAsync(new CallWebhookEvent(eventId)
        {
            RoomName = roomName,
            RoomSid = roomSid,
            EventType = eventType,
            ProcessedAt = DateTime.UtcNow
        }, autoSave: true);

        if (eventType == "participant_joined" && root.TryGetProperty("participant", out var pEl))
        {
            var metaStr = pEl.TryGetProperty("metadata", out var mEl) ? mEl.GetString() : null;
            var pSid = pEl.TryGetProperty("sid", out var psEl) ? psEl.GetString() : null;
            if (!string.IsNullOrWhiteSpace(metaStr) && !string.IsNullOrWhiteSpace(roomName) && roomName.StartsWith("gotoim_call_"))
            {
                using var metaDoc = JsonDocument.Parse(metaStr);
                if (metaDoc.RootElement.TryGetProperty("sessionUnitId", out var suEl)
                    && metaDoc.RootElement.TryGetProperty("deviceId", out var dEl)
                    && Guid.TryParse(suEl.GetString(), out var sessionUnitId))
                {
                    var parts = roomName["gotoim_call_".Length..].Split("_v");
                    if (parts.Length == 2 && Guid.TryParseExact(parts[0], "N", out var callId) && int.TryParse(parts[1], out var epoch))
                    {
                        await callSessionManager.TryMarkParticipantConnectedAsync(
                            callId, epoch, roomSid, sessionUnitId, dEl.GetString(), pSid, DateTime.UtcNow);
                    }
                }
            }
        }

        return Ok();
    }
}
