using IczpNet.Chat.CacheKeys;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageCacheKey(long messageId) : CacheKey<MessageCacheKey>
{
    /// <summary>
    /// MessageId
    /// </summary>
    public long MessageId { get; set; } = messageId;

    public bool Equals(MessageCacheKey other)
        => other != null && MessageId == other.MessageId;

    protected override bool EqualsCore(MessageCacheKey other)
        => MessageId == other.MessageId;

    protected override int GetKeyHashCode()
        => MessageId.GetHashCode();

    public override string ToString()
        => $"{nameof(MessageCacheKey)}-{nameof(MessageId)}:{MessageId}";
}
