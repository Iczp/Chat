using IczpNet.Chat.Calls;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace IczpNet.Chat.Domain.Tests.Calls;

public class CallProtocolAndToken_Tests
{
    [Fact]
    public void SummaryClientMessageId_is_deterministic_and_within_26_chars()
    {
        var callId = Guid.NewGuid();
        var id1 = CallSessionManager.ComputeSummaryClientMessageId(callId);
        var id2 = CallSessionManager.ComputeSummaryClientMessageId(callId);

        Assert.Equal(id1, id2);
        Assert.StartsWith("c_", id1);
        Assert.Equal(24, id1.Length);
        Assert.True(id1.Length <= 26);
    }

    [Fact]
    public void MessageTypes_Call_is_in_DisabledForwardList()
    {
        Assert.Contains(MessageTypes.Call, MessageExtentions.DisabledForwardList);
        Assert.Equal(20, (int)MessageTypes.Call);
    }

    [Fact]
    public void LiveKitHttpClient_generates_valid_token_and_validates_webhook_signature()
    {
        var opts = Microsoft.Extensions.Options.Options.Create(new LiveKitOptions
        {
            ApiKey = "lk_test_key",
            ApiSecret = "0123456789abcdef0123456789abcdef"
        });
        var client = new LiveKitHttpClient(Substitute.For<IHttpClientFactory>(), opts);

        var token = client.GenerateParticipantToken("gotoim_call_test_v1", "1001:dev1", "{\"sessionUnitId\":\"u1\"}", 180);
        Assert.Equal(3, token.Split('.').Length);

        var body = "{\"id\":\"ev_1\",\"event\":\"participant_joined\"}";
        var sha = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        var headerB64 = Base64Url(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payloadB64 = Base64Url(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { iss = "lk_test_key", sha256 = sha })));
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(opts.Value.ApiSecret));
        var sigB64 = Base64Url(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{headerB64}.{payloadB64}")));

        Assert.True(client.ValidateWebhookSignature($"Bearer {headerB64}.{payloadB64}.{sigB64}", body));
        Assert.False(client.ValidateWebhookSignature($"Bearer {headerB64}.{payloadB64}.{sigB64}", body + "tampered"));
    }

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
