using StackExchange.Redis;
using System;

namespace IczpNet.Chat.SessionUnits;

public readonly record struct SessionUnitElement(long OwnerId, long FriendId, Guid SessionUnitId, Guid SessionId)
{
    public override string ToString()
        => $"{OwnerId}:{FriendId}:{SessionUnitId}:{SessionId}";

    public static SessionUnitElement Create(long OwnerId, long FriendId, Guid SessionUnitId, Guid SessionId)
    {
        return new SessionUnitElement(OwnerId, FriendId, SessionUnitId, SessionId);
    }

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator RedisValue(SessionUnitElement element)
        => element.ToString();

    public static SessionUnitElement Parse(RedisValue element)
    {
        if (!TryParse(element, out var field))
        {
            throw new FormatException($"Invalid SessionUnitField: {element}");
        }

        return field;
    }

    public static bool TryParse(RedisValue value, out SessionUnitElement field)
    {
        field = default;

        if (!value.HasValue)
        {
            return false;
        }

        var parts = value.ToString().Split(':');
        if (parts.Length != 4)
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

        if (!Guid.TryParse(parts[2], out var unitId))
        {
            return false;
        }
        if (!Guid.TryParse(parts[3], out var sessionId))
        {
            return false;
        }

        field = Create(ownerId, friendId, unitId, sessionId);
        return true;
    }
}
