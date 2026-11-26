using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace IczpNet.Chat.Blobs;

public sealed class AttachmentReuseTokenPayload
{
    public Guid UserId { get; set; }
    public Guid PrimaryBlobId { get; set; }
    public AttachmentMediaType MediaType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; }
    public string Profile { get; set; }
    public long Exp { get; set; }
}

public interface IAttachmentReuseTokenService
{
    string IssueToken(
        Guid userId,
        Guid primaryBlobId,
        AttachmentMediaType mediaType,
        int? width = null,
        int? height = null,
        int? duration = null,
        string profile = null,
        DateTime? expireTime = null);

    bool TryValidateToken(string token, Guid currentUserId, out AttachmentReuseTokenPayload payload);
}

public class AttachmentReuseTokenService(
    IOptions<AttachmentSignatureOptions> signatureOptions,
    IOptions<AttachmentUploadOptions> uploadOptions,
    IClock clock) : IAttachmentReuseTokenService, ITransientDependency
{
    public virtual string IssueToken(
        Guid userId,
        Guid primaryBlobId,
        AttachmentMediaType mediaType,
        int? width = null,
        int? height = null,
        int? duration = null,
        string profile = null,
        DateTime? expireTime = null)
    {
        if (userId == Guid.Empty || primaryBlobId == Guid.Empty)
        {
            return null;
        }

        var now = clock.Now;
        var defaultExpire = now.AddHours(Math.Clamp(uploadOptions.Value.TempBlobExpireHours, 1, 24));
        var effectiveExpire = expireTime.HasValue && expireTime.Value > now
            ? (expireTime.Value < defaultExpire ? expireTime.Value : defaultExpire)
            : defaultExpire;

        var payload = new AttachmentReuseTokenPayload
        {
            UserId = userId,
            PrimaryBlobId = primaryBlobId,
            MediaType = mediaType,
            Width = width,
            Height = height,
            Duration = duration,
            Profile = profile,
            Exp = new DateTimeOffset(effectiveExpire).ToUnixTimeSeconds(),
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var sig = ComputeHmac(encodedPayload);
        return $"{encodedPayload}.{sig}";
    }

    public virtual bool TryValidateToken(string token, Guid currentUserId, out AttachmentReuseTokenPayload payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(token) || currentUserId == Guid.Empty)
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        var expectedSig = ComputeHmac(parts[0]);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSig);
        var actualBytes = Encoding.UTF8.GetBytes(parts[1]);
        if (expectedBytes.Length != actualBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            return false;
        }

        try
        {
            var payloadBytes = Base64UrlDecode(parts[0]);
            var parsed = JsonSerializer.Deserialize<AttachmentReuseTokenPayload>(payloadBytes);
            if (parsed == null ||
                parsed.UserId != currentUserId ||
                parsed.PrimaryBlobId == Guid.Empty)
            {
                return false;
            }

            var nowUnix = new DateTimeOffset(clock.Now).ToUnixTimeSeconds();
            if (parsed.Exp < nowUnix)
            {
                return false;
            }

            payload = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string ComputeHmac(string encodedPayload)
    {
        var signingKey = AttachmentSignatureOptions.ResolveValidatedSigningKey(signatureOptions.Value);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"reuse|{encodedPayload}"));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

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
