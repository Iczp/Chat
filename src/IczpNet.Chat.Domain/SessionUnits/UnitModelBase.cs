using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnits;

public class UnitModelBase
{
    public Guid Id { get; set; }

    public long OwnerId { get; set; }

    public ChatObjectTypeEnums? OwnerObjectType { get; set; }

    public long DestinationId { get; set; }

    public ChatObjectTypeEnums? DestinationObjectType { get; set; }

    public Guid SessionId { get; set; }
}
