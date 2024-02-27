namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 文件消息
/// </summary>
public class FileContentInfo : MessageContentAttachmentsInfoBase, IContentInfo
{
    /// <summary>
    /// FileName
    /// </summary>
    public virtual string FileName { get; set; }

    /// <summary>
    /// 文件地址
    /// </summary>
    public virtual string Url { get; set; }

    /// <summary>
    /// 文件地址
    /// </summary>
    public virtual string ActionUrl { get; set; }

    /// <summary>
    /// ContentType
    /// </summary>
    public virtual string ContentType { get; set; }

    /// <summary>
    /// 文件后缀名
    /// </summary>
    public virtual string Suffix { get; set; }

    /// <summary>
    /// 大小 ContentLength(Size)
    /// </summary>
    public virtual long? ContentLength { get; set; }
}