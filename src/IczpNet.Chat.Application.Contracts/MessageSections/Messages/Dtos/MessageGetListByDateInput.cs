namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageGetListByDateInput : MessageGetListInput
{

    /// <summary>
    /// 每组获取条数
    /// </summary>
    public virtual int? TakeCount { get; set; }
}
