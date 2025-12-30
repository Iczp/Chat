using System;

namespace IczpNet.Chat.SessionUnits;

public class MemberModel: UnitModelBase
{
    public DateTime JoinedTime { get; set; }
    public bool IsCreator { get; set; }
}
