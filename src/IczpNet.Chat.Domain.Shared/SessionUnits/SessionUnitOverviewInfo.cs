using IczpNet.Chat.SessionBoxes;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitOverviewInfo
{
    public long OwnerId { get; set; }

    public long TotalUnreadCount { get; set; }

    public SessionUnitStatistic Stat { get; set; }

    public List<TypeBadgeInfo> Types { get; set; }

    public List<BoxBadgeInfo> Boxes { get; set; }
}


public class BadgeInfo<T>
{
    public virtual T Id { get; set; }

    public virtual string Name { get; set; }

    public virtual long? Count { get; set; }

    public virtual long? Badge { get; set; }
}
public class TypeBadgeInfo : BadgeInfo<string>
{
}
public class BoxBadgeInfo : BadgeInfo<Guid>
{
}
