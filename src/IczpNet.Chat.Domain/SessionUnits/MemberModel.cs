using System;

namespace IczpNet.Chat.SessionUnits;

public class MemberModel: UnitModelBase
{
    //public Guid Id { get; set; }

    //public long OwnerId { get; set; }

    //public Guid SessionId { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsCreator { get; set; }
}
