namespace IczpNet.Chat.Developers.Dtos;

public class DeveloperDto
{
    /// <summary>
    /// 开发者设置的Token
    /// </summary>
    public virtual string Token { get; set; }

    /// <summary>
    /// 开发者设置的EncodingAESKey
    /// </summary>
    public virtual string EncodingAesKey { get; set; }

    /// <summary>
    /// 开发者设置 的 Url
    /// </summary>
    public virtual string PostUrl { get; set; }

}
