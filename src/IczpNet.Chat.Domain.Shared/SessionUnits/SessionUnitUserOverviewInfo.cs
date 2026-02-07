using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitUserOverviewInfo
{
    public Guid UserId { get; set; }

    public long TotalUnreadCount { get; set; }

    public long TotalOwnersCount { get; set; }

    public List<SessionUnitOwnerOverviewInfo> Overviews { get; set; }

}