using IczpNet.Chat.CacheKeys;
using System;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitSearchCacheKey(long ownerId, string keyword) : CacheKey<SessionUnitSearchCacheKey>
{
    public long OwnerId { get; set; } = ownerId;

    public string Keyword { get; set; } = keyword;

    public override string ToString()
    {
        return $"SessionUnits:Owners:OwnerId-{OwnerId}:Keywords:{Keyword}";
    }

    protected override bool EqualsCore(SessionUnitSearchCacheKey other)
    {
        return OwnerId == other.OwnerId && Keyword == other.Keyword;
    }

    protected override int GetKeyHashCode()
    {
        return HashCode.Combine(OwnerId, Keyword);
    }
}
