using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class SendMessageToClientDistributedEto : ISendMessageToClientDistributedEto
{
    /// <summary>
    /// Command
    /// </summary>
    public virtual string Command { get; set; }

    /// <summary>
    /// MessageId
    /// </summary>
    public virtual long MessageId { get; set; }

    /// <summary>
    /// CacheKey
    /// </summary>
    public virtual string CacheKey { get; set; }

    /// <summary>
    /// Emiter's HostName
    /// </summary>
    public virtual string HostName { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public virtual MessageInfo<object> Message { get; set; }

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{nameof(SendMessageToClientDistributedEto)}]:{nameof(Command)}={Command},{nameof(MessageId)}={MessageId},{nameof(HostName)}={HostName}";
    }
}
