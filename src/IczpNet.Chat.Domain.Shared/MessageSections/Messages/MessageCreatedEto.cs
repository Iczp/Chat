using System;

namespace IczpNet.Chat.MessageSections.Messages;


[Serializable]
public class MessageCreatedEto
{
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
        return $"{nameof(MessageId)}={MessageId},{nameof(HostName)}={HostName}";
    }
}
