using IczpNet.Chat.CacheKeys;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

public class SessionMaxMessageIdCacheKey(Guid sessionId) : CacheKey<SessionMaxMessageIdCacheKey>
{
    /// <summary>
    /// MessageId
    /// </summary>
    public Guid SessionId { get; set; } = sessionId;

    public bool Equals(SessionMaxMessageIdCacheKey other)
        => other != null && SessionId == other.SessionId;

    protected override bool EqualsCore(SessionMaxMessageIdCacheKey other)
        => SessionId == other.SessionId;

    protected override int GetKeyHashCode()
        => SessionId.GetHashCode();

    public override string ToString()
        => $"{nameof(SessionMaxMessageIdCacheKey)}-{nameof(SessionId)}:{SessionId}";
}
