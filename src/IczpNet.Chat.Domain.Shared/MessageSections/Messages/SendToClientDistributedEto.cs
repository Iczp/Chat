using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class SendToClientDistributedEto : IMessageChangedDistributedEto
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
    public virtual MessageInfo Message { get; set; }

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{nameof(SendToClientDistributedEto)}]:{nameof(Command)}={Command},{nameof(MessageId)}={MessageId},{nameof(HostName)}={HostName}";
    }
}
