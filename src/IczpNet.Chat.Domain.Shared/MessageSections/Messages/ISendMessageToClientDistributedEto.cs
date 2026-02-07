namespace IczpNet.Chat.MessageSections.Messages;

public interface ISendMessageToClientDistributedEto
{
    /// <summary>
    /// Command
    /// </summary>
    string Command { get; set; }
    /// <summary>
    /// MessageId
    /// </summary>
    long MessageId { get; set; }

    /// <summary>
    /// CacheKey
    /// </summary>
    string CacheKey { get; set; }

    /// <summary>
    /// Emiter's HostName
    /// </summary>
    string HostName { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    MessageInfo<object> Message {  get; set; }
}
