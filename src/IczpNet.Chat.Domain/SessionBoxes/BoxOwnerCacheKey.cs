namespace IczpNet.Chat.SessionBoxes;

public class BoxOwnerCacheKey(long  ownerId)
{
    public long OwnerId { get; set; } = ownerId;

    public static BoxOwnerCacheKey Create(long ownerId)
    {
        return new BoxOwnerCacheKey(ownerId);
    }
    public override string ToString()
    {

        return $"{nameof(BoxOwnerCacheKey)}-{nameof(OwnerId)}:{OwnerId}";
    }
}
