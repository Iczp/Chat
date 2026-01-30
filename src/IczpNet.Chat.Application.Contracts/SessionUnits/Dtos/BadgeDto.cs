using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionBoxes;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class BadgeDto
{
    public virtual Guid? AppUserId { get; set; }

    public virtual long ChatObjectId { get; set; }

    public virtual long? Badge { get; set; }

    public virtual SessionUnitStatistic Statistic { get; set; }

    public virtual Dictionary<ChatObjectTypeEnums, long> BadgeMap { get; set; }

    public virtual Dictionary<ChatObjectTypeEnums, long> CountMap { get; set; }

    public virtual List<BoxCountDto> Boxes { get; set; }
}
