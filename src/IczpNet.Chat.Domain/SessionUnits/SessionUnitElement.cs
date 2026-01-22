using IczpNet.Chat.Enums;
using StackExchange.Redis;
using System;

namespace IczpNet.Chat.SessionUnits;

public readonly record struct SessionUnitElement(long OwnerId, ChatObjectTypeEnums? OwnerObjectType, long DestinationId, ChatObjectTypeEnums? DestinationObjectType, Guid SessionUnitId, Guid SessionId)
{
    public override string ToString()
        => $"{OwnerId}:{OwnerObjectType}:{DestinationId}:{DestinationObjectType}:{SessionUnitId}:{SessionId}";

    public static SessionUnitElement Create(long ownerId, ChatObjectTypeEnums? ownerObjectType, long destinationId, ChatObjectTypeEnums? destinationObjectType, Guid sessionUnitId, Guid sessionId)
    {
        return new SessionUnitElement(ownerId, ownerObjectType, destinationId, destinationObjectType, sessionUnitId, sessionId);
    }

    /// <summary>
    /// 隐式转换，便于 Redis API 使用
    /// </summary>
    public static implicit operator RedisValue(SessionUnitElement element) => element.ToString();

    public static SessionUnitElement Parse(RedisValue element)
    {
        if (!TryParse(element, out var field))
        {
            throw new FormatException($"Invalid {nameof(SessionUnitElement)}: {element}");
        }

        return field;
    }

    public static bool TryParse(RedisValue element, out SessionUnitElement field)
    {
        field = default;

        if (!element.HasValue)
        {
            return false;
        }

        var parts = element.ToString().Split(':');

        if (parts.Length != 6)
        {
            return false;
        }

        // 0. ownerId
        if (!long.TryParse(parts[0], out var ownerId))
        {
            return false;
        }

        // 1. ownerObjectType
        ChatObjectTypeEnums? ownerObjectType = Enum.TryParse<ChatObjectTypeEnums>(parts[1], out var _ownerObjectType) ? _ownerObjectType : null;

        // 2. destinationId
        if (!long.TryParse(parts[2], out var destinationId))
        {
            return false;
        }

        // 3. destinationObjectType
        ChatObjectTypeEnums? destinationObjectType = Enum.TryParse<ChatObjectTypeEnums>(parts[3], out var _destinationObjectType) ? _destinationObjectType : null;

        // 4. unitId
        if (!Guid.TryParse(parts[4], out var unitId))
        {
            return false;
        }
        // 5. sessionId
        if (!Guid.TryParse(parts[5], out var sessionId))
        {
            return false;
        }

        field = Create(ownerId, ownerObjectType, destinationId, destinationObjectType, unitId, sessionId);

        return true;
    }
}
