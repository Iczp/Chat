using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class BadgeDto
{
    public virtual Guid? AppUserId { get; set; }

    public virtual long ChatObjectId { get; set; }

    public virtual int Badge { get; set; }

}
