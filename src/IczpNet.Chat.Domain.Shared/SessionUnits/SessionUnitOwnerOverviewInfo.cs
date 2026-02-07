using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitOwnerOverviewInfo
{
    public long OwnerId { get; set; }

    public long TotalUnreadCount { get; set; }

    public long TotalFriendsCount { get; set; }

    public SessionUnitStatistic Stat { get; set; }

    public List<SessionUnitStatInfo> Types { get; set; }

    public List<SessionUnitStatInfo> Boxes { get; set; }
}




