namespace IczpNet.Chat.Options;

public class AttachmentSignatureOptions
{
    /// <summary>
    /// HMAC-SHA256 签名密钥（生产环境建议通过配置覆盖）
    /// </summary>
    public string SigningKey { get; set; } = "IczpNet.Chat.Default.Attachment.Signing.Key.2026";

    /// <summary>
    /// 默认签名有效期（秒），默认 15 分钟
    /// </summary>
    public int DefaultExpireSeconds { get; set; } = 900;
}
