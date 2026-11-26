using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace IczpNet.Chat.Blobs;

public interface IBlobSignatureService
{
    string AppendSignature(string baseUrl, Guid blobId, Guid sessionUnitId, long? messageId = null, long? rootMessageId = null, int? expireSeconds = null);

    bool ValidateSignature(Guid blobId, Guid sessionUnitId, long? messageId, long? rootMessageId, long exp, string sig);
}

public class BlobSignatureService(
    IOptions<AttachmentSignatureOptions> options,
    IClock clock) : IBlobSignatureService, ITransientDependency
{
    public virtual string AppendSignature(
        string baseUrl,
        Guid blobId,
        Guid sessionUnitId,
        long? messageId = null,
        long? rootMessageId = null,
        int? expireSeconds = null)
    {
        var ttl = expireSeconds.GetValueOrDefault(options.Value.DefaultExpireSeconds);
        if (ttl <= 0) ttl = 900;

        var exp = new DateTimeOffset(clock.Now).ToUnixTimeSeconds() + ttl;
        var sig = ComputeSignature(blobId, sessionUnitId, messageId, rootMessageId, exp);

        var query = new List<string>
        {
            $"su={sessionUnitId:D}",
            $"exp={exp}",
            $"sig={Uri.EscapeDataString(sig)}",
        };
        if (messageId.HasValue) query.Add($"msg={messageId.Value}");
        if (rootMessageId.HasValue) query.Add($"rootMsg={rootMessageId.Value}");

        var separator = baseUrl.Contains('?') ? "&" : "?";
        return $"{baseUrl}{separator}{string.Join("&", query)}";
    }

    public virtual bool ValidateSignature(Guid blobId, Guid sessionUnitId, long? messageId, long? rootMessageId, long exp, string sig)
    {
        if (string.IsNullOrWhiteSpace(sig))
        {
            return false;
        }

        var nowUnix = new DateTimeOffset(clock.Now).ToUnixTimeSeconds();
        if (exp < nowUnix)
        {
            return false;
        }

        var expected = ComputeSignature(blobId, sessionUnitId, messageId, rootMessageId, exp);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(sig);
        return expectedBytes.Length == actualBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    protected virtual string ComputeSignature(Guid blobId, Guid sessionUnitId, long? messageId, long? rootMessageId, long exp)
    {
        var signingKey = AttachmentSignatureOptions.ResolveValidatedSigningKey(options.Value);
        var payload = $"{blobId:N}|{sessionUnitId:N}|{messageId?.ToString() ?? string.Empty}|{rootMessageId?.ToString() ?? string.Empty}|{exp}";
        var keyBytes = Encoding.UTF8.GetBytes(signingKey);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
