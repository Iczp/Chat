using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Calls;

public class LiveKitHttpClient(
    IHttpClientFactory httpClientFactory,
    IOptions<LiveKitOptions> options) : ILiveKitClient, ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private LiveKitOptions Options => options.Value;

    public async Task<LiveKitRoomInfo> CreateRoomAsync(string roomName, int emptyTimeoutSeconds, int maxParticipants, CancellationToken cancellationToken = default)
    {
        var payload = new { name = roomName, emptyTimeout = emptyTimeoutSeconds, maxParticipants };
        using var resp = await PostTwirpAsync("CreateRoom", payload, new { roomAdmin = true, roomCreate = true, room = roomName }, cancellationToken);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<LiveKitRoomInfo>(json, JsonOptions);
    }

    public async Task DeleteRoomAsync(string roomName, CancellationToken cancellationToken = default)
    {
        var payload = new { room = roomName };
        using var resp = await PostTwirpAsync("DeleteRoom", payload, new { roomAdmin = true, room = roomName }, cancellationToken);
        if (resp.StatusCode == HttpStatusCode.NotFound) return;
        resp.EnsureSuccessStatusCode();
    }

    public async Task RemoveParticipantAsync(string roomName, string identity, CancellationToken cancellationToken = default)
    {
        var payload = new { room = roomName, identity };
        using var resp = await PostTwirpAsync("RemoveParticipant", payload, new { roomAdmin = true, room = roomName }, cancellationToken);
        if (resp.StatusCode == HttpStatusCode.NotFound) return;
        resp.EnsureSuccessStatusCode();
    }

    public async Task<List<LiveKitRoomInfo>> ListRoomsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var payload = new { names };
        using var resp = await PostTwirpAsync("ListRooms", payload, new { roomList = true }, cancellationToken);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("rooms", out var roomsEl))
            return JsonSerializer.Deserialize<List<LiveKitRoomInfo>>(roomsEl.GetRawText(), JsonOptions) ?? [];
        return [];
    }

    public async Task<List<LiveKitParticipantInfo>> ListParticipantsAsync(string roomName, CancellationToken cancellationToken = default)
    {
        var payload = new { room = roomName };
        using var resp = await PostTwirpAsync("ListParticipants", payload, new { roomAdmin = true, room = roomName }, cancellationToken);
        if (resp.StatusCode == HttpStatusCode.NotFound) return [];
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("participants", out var pEl))
            return JsonSerializer.Deserialize<List<LiveKitParticipantInfo>>(pEl.GetRawText(), JsonOptions) ?? [];
        return [];
    }

    public string GenerateParticipantToken(string roomName, string identity, string metadataJson, int ttlSeconds = CallConsts.DefaultTokenTtlSeconds)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["iss"] = Options.ApiKey,
            ["sub"] = identity,
            ["nbf"] = now - 5,
            ["exp"] = now + ttlSeconds,
            ["metadata"] = metadataJson,
            ["video"] = new
            {
                roomJoin = true,
                room = roomName,
                canPublish = true,
                canSubscribe = true,
                canPublishData = true,
            }
        };
        return SignJwt(claims);
    }

    public bool ValidateWebhookSignature(string authorizationHeader, string rawBody)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) || string.IsNullOrWhiteSpace(Options.ApiSecret))
            return false;
        var token = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[7..].Trim()
            : authorizationHeader.Trim();
        var parts = token.Split('.');
        if (parts.Length != 3) return false;

        var signingInput = $"{parts[0]}.{parts[1]}";
        var expectedSig = Base64UrlEncode(ComputeHmacSha256(signingInput, Options.ApiSecret));
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expectedSig), Encoding.UTF8.GetBytes(parts[2])))
            return false;

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
        using var doc = JsonDocument.Parse(payloadJson);
        if (doc.RootElement.TryGetProperty("sha256", out var shaEl))
        {
            var bodyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawBody ?? string.Empty)));
            return string.Equals(shaEl.GetString(), bodyHash, StringComparison.Ordinal);
        }
        return false;
    }

    private async Task<HttpResponseMessage> PostTwirpAsync(string method, object body, object videoGrant, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var adminJwt = SignJwt(new Dictionary<string, object>
        {
            ["iss"] = Options.ApiKey,
            ["nbf"] = now - 5,
            ["exp"] = now + 60,
            ["video"] = videoGrant
        });
        var baseUrl = Options.InternalUrl.TrimEnd('/');
        var client = httpClientFactory.CreateClient(nameof(LiveKitHttpClient));
        var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/twirp/livekit.RoomService/{method}")
        {
            Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminJwt);
        return await client.SendAsync(req, cancellationToken);
    }

    private string SignJwt(Dictionary<string, object> payload)
    {
        var headerJson = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" }, JsonOptions);
        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var sig = Base64UrlEncode(ComputeHmacSha256($"{headerB64}.{payloadB64}", Options.ApiSecret));
        return $"{headerB64}.{payloadB64}.{sig}";
    }

    private static byte[] ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string Base64UrlEncode(byte[] input)
        => Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
