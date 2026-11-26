using System;

namespace IczpNet.Chat.Options;

public class AttachmentSignatureOptions
{
    public const string DefaultInsecureSigningKey = "IczpNet.Chat.Default.Attachment.Signing.Key.2026";

    /// <summary>
    /// HMAC-SHA256 签名密钥（生产环境必须通过配置注入非默认值且至少 16 个字符）
    /// </summary>
    public string SigningKey { get; set; } = DefaultInsecureSigningKey;

    /// <summary>
    /// 是否显式允许使用默认示例密钥（仅限本地测试或过渡排查，默认 false）
    /// </summary>
    public bool AllowInsecureDefaultSigningKey { get; set; } = false;

    /// <summary>
    /// 默认签名有效期（秒），默认 15 分钟
    /// </summary>
    public int DefaultExpireSeconds { get; set; } = 900;

    /// <summary>
    /// 是否允许对私有聊天附件重定向到对象存储预签名直链（默认 false，要求每次请求均经过应用鉴权）
    /// </summary>
    public bool EnableDirectUrlForPrivateBlobs { get; set; } = false;

    /// <summary>
    /// 私有聊天附件直链最大有效期（秒），默认 60 秒
    /// </summary>
    public int PrivateDirectUrlExpireSeconds { get; set; } = 60;

    internal static string ResolveValidatedSigningKey(AttachmentSignatureOptions options)
    {
        var key = options?.SigningKey?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length < 16)
        {
            throw new InvalidOperationException("AttachmentSignature:SigningKey must be configured with at least 16 characters.");
        }

        if (!options!.AllowInsecureDefaultSigningKey &&
            string.Equals(key, DefaultInsecureSigningKey, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("AttachmentSignature:SigningKey cannot use the built-in default value when AllowInsecureDefaultSigningKey is false.");
        }

        return key;
    }
}
