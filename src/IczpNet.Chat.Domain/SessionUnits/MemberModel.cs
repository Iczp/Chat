using System;

namespace IczpNet.Chat.SessionUnits;

public class MemberModel: UnitModelBase
{
    //public Guid Id { get; set; }

    //public long OwnerId { get; set; }

    //public Guid SessionId { get; set; }

    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsCreator { get; set; }

    /// <summary>
    /// Member Score
    /// </summary>
    public double Score { get; internal set; }
}
