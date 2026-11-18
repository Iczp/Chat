using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitGetMemberOptions
{
    /// <summary>
    /// 访问者 SessionUnitId
    /// </summary>
    public virtual Guid VisitorId { get; set; }
}
