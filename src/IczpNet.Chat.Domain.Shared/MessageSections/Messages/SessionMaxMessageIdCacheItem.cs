using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class SessionMaxMessageIdCacheItem
{
    /// <summary>
    /// MaxMessageId
    /// </summary>
    public long MaxMessageId { get; set; } 
}
