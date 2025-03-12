using System;

namespace IczpNet.Chat.MessageSections.Messages;


[Serializable]
public class MessageUpdatedEto
{
    /// <summary>
    /// MessageId
    /// </summary>
    public virtual long MessageId { get; set; }

    /// <summary>
    /// SessionCacheKey
    /// </summary>
    public virtual string SessionCacheKey { get; set; }
}
