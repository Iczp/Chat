using System;

namespace IczpNet.Chat.Locations;

public class UserLocationCacheKey
{
    public Guid UserId { get; set; }

    public UserLocationCacheKey(Guid userId)
    {
        UserId = userId;
    }

    public override string ToString()
    {
        return $"{nameof(UserLocationCacheKey)}_{UserId}";
    }
}
