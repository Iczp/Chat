namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageFastGetListInput : MessageGetListInput
{
    /// <summary>
    /// 是否统计真实总数
    /// </summary>
    public virtual bool? IsRealTotalCount { get; set; } = false;
}
