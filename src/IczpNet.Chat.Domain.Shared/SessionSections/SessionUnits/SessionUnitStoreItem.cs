using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitStoreItem
{
    public double SetTopTick { get; set; }
    public long LastMessageId { get; set; }
    public long OwnerId { get; set; }
    public Guid SessionUnitId { get; set; }
    public long LastUpdateTime { get; set; }
}
