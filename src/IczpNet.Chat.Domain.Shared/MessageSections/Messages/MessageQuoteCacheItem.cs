using System;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class MessageQuoteCacheItem : MessageInfo<object>
{
    public virtual MessageQuoteCacheItem CloneWithoutSender()
    {
        var copy = (MessageQuoteCacheItem)MemberwiseClone();
        copy.SenderSessionUnit = null;
        return copy;
    }
}
