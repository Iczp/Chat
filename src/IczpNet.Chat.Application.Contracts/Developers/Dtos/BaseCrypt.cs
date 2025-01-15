namespace IczpNet.Chat.Developers.Dtos;

/// <summary>
/// 
/// </summary>
public abstract class BaseCrypt
{
    /// <summary>
    /// 公众号Id
    /// </summary>
    public string ChatObjectId { get; set; }

    /// <summary>
    /// 公众平台上，开发者设置的 EncodingAesKey
    /// </summary>
    public string EncodingAesKey { get; set; }

    /// <summary>
    /// 公众平台上，开发者设置的 Token
    /// </summary>
    public string Token { get; set; }
}
