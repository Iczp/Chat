using System;

namespace IczpNet.Chat.SessionUnits;

public class MemberModel: UnitModelBase
{
    public DateTime CreationTime { get; set; }
    public bool IsCreator { get; set; }
}
