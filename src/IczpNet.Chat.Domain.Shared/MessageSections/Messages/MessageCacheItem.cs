using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class MessageCacheItem : MessageQuoteCacheItem
{
    /// <summary>
    /// 引用消息
    /// </summary>
    public virtual MessageQuoteCacheItem QuoteMessage { get; set; }

    /// <summary>
    /// 生成面向 Redis 存储的瘦身副本：仅保留标量 SenderSessionUnitId / SenderId，剥离嵌套的 SenderSessionUnit 快照，
    /// 节省 50%~65% 缓存体积并杜绝改名/换头像后出现陈旧快照；读取时由 MessageManager 按去重 ID 批量水合。
    /// </summary>
    public virtual MessageCacheItem ToSlimRedisCopy()
    {
        var copy = (MessageCacheItem)MemberwiseClone();
        copy.SenderSessionUnit = null;
        if (copy.QuoteMessage != null)
        {
            var quoteCopy = copy.QuoteMessage.CloneWithoutSender();
            copy.QuoteMessage = quoteCopy;
        }
        return copy;
    }
}
