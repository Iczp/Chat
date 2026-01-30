using System;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitStoreItem
{
    public double Sorting { get; set; }
    public long LastMessageId { get; set; }
    public long OwnerId { get; set; }
    public Guid SessionUnitId { get; set; }
    public long LastUpdateTime { get; set; }
}
