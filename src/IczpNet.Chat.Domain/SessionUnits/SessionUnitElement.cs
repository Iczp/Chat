using StackExchange.Redis;
using System;

namespace IczpNet.Chat.SessionUnits;

public readonly record struct SessionUnitElement(
    Guid SessionId,
    long OwnerId,
    Guid SessionUnitId)
{
    public override string ToString()
        => $"{SessionId}:{OwnerId}:{SessionUnitId}";

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
        if (parts.Length != 3)
        {
            return false;
        }

        if (!Guid.TryParse(parts[0], out var sessionId))
        {
            return false;
        }

        if (!long.TryParse(parts[1], out var ownerId))
        {
            return false;
        }

        if (!Guid.TryParse(parts[2], out var unitId))
        {
            return false;
        }

        field = new SessionUnitElement(sessionId, ownerId, unitId);
        return true;
    }
}
