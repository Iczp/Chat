namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 文件消息
/// </summary>
public class FileContentInfo : MessageContentAttachmentsInfoBase, IContentInfo
{
    /// <summary>
    /// 文件地址
    /// </summary>
    public virtual string ActionUrl { get; set; }
}