using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class MessageCacheItem : MessageQuoteCacheItem
{
    /// <summary>
    /// 引用消息
    /// </summary>
    public virtual MessageQuoteCacheItem QuoteMessage { get; set; }

}
