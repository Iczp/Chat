using System;

namespace IczpNet.Chat.SessionUnits;

public class UnitModelBase
{
    public Guid Id { get; set; }

    public long OwnerId { get; set; }

    public Guid SessionId { get; set; }
}
