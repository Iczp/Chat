namespace IczpNet.Chat.SessionUnits;

public class SessionUnitSearchCacheKey(long ownerId, string keyword)
{
    public long OwnerId { get; set; } = ownerId;

    public string Keyword { get; set; } = keyword;

    public override string ToString()
    {
        return $"SessionUnits:Owners:OwnerId-{OwnerId}:Keywords:{Keyword}";
    }
}
