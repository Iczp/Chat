namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 聊天历史
/// </summary>
public class HistoryContentOutput : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// 标题内容
    /// </summary>
    public virtual string Title { get; set; }

    /// <summary>
    /// 简要说明
    /// </summary>
    public virtual string Description { get; set; }
}