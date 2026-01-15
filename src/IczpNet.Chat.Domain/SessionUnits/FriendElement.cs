using StackExchange.Redis;
using System;
using System.Xml.Linq;

namespace IczpNet.Chat.SessionUnits;

public readonly record struct FriendElement(long OwnerId, long FriendId, Guid SessionId)
{
    public override string ToString()
        => $"{OwnerId}:{FriendId}:{SessionId}";

    public static FriendElement Create(long OwnerId, long FriendId, Guid SessionId)
    {
        return new FriendElement(OwnerId, FriendId, SessionId);
    }

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator RedisValue(FriendElement element)
        => element.ToString();

    public static FriendElement Parse(RedisValue element)
    {
        if (!TryParse(element, out var field))
        {
            throw new FormatException($"Invalid FriendElement Field: {element}");
        }

        return field;
    }

    public static FriendElement Parse(SessionUnitElement element)
    {
        return Create(element.OwnerId, element.FriendId, element.SessionUnitId);
    }

    

    public static bool TryParse(RedisValue value, out FriendElement field)
    {
        field = default;

        if (!value.HasValue)
        {
            return false;
        }

        var parts = value.ToString().Split(':');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!long.TryParse(parts[0], out var ownerId))
        {
            return false;
        }

        if (!long.TryParse(parts[1], out var friendId))
        {
            return false;
        }

        if (!Guid.TryParse(parts[2], out var sessionId))
        {
            return false;
        }


        field = Create(ownerId, friendId, sessionId);
        return true;
    }
}
