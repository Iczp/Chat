using System;

namespace IczpNet.Chat.Locations;

public class SessionLocationCacheKey
{
    public virtual Guid SessionId { get; }

    public SessionLocationCacheKey(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public override string ToString()
    {
        return $"{nameof(SessionLocationCacheKey)}_{SessionId}";
    }
}
