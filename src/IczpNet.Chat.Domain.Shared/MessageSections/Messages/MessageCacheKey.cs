namespace IczpNet.Chat.MessageSections.Messages;

public class MessageCacheKey(long id)
{
    /// <summary>
    /// MessageId
    /// </summary>
    public long Id { get; set; } = id;

    public override string ToString()
    {
        return $"{nameof(MessageCacheKey)}-{nameof(Id)}:{Id}";
    }
}
