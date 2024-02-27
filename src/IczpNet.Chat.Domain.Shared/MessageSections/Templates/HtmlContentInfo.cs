using IczpNet.Chat.Enums;

namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// HTML消息
/// </summary>
public class HtmlContentInfo : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// 编辑器类型
    /// </summary>
    public virtual EditorTypes EditorType { get; set; }

    /// <summary>
    /// 文本内容
    /// </summary>
    //[Required(ErrorMessage = "文本内容[Title]必填！")]
    public virtual string Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public virtual string Content { get; set; }

    /// <summary>
    /// 原始地址
    /// </summary>
    public virtual string OriginalUrl { get; set; }

}