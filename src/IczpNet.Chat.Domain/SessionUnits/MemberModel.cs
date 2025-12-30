using System;

namespace IczpNet.Chat.SessionUnits;

public class MemberModel
{
    public Guid SessionId { get; set; }
    public long OwnerId { get; set; }
    public Guid SessionUnitId { get; set; }
    public DateTime JoinedTime { get; set; }
    public bool IsCreator { get; set; }
}
