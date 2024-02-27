namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 分享链接消息
/// </summary>
public class LinkContentInfo : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// Url
    /// </summary>
    public virtual string Url { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// 简要说明
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 图片
    /// </summary>
    public virtual string Image { get; set; }

    /// <summary>
    /// 发行人名称
    /// </summary>
    public virtual string IssuerName { get; set; }

    /// <summary>
    /// 发行人图标
    /// </summary>
    public virtual string IssuerIcon { get; set; }
}