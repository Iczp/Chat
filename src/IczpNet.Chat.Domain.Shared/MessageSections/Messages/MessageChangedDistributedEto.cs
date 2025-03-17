using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class MessageChangedDistributedEto : IMessageChangedDistributedEto
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
    /// HostName
    /// </summary>
    public virtual string HostName { get; set; }

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(Command)}={Command},{nameof(MessageId)}={MessageId},{nameof(HostName)}={HostName}";
    }
}
