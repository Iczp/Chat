using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Robots.Dtos;

public class RobotDto
{
    public virtual long Id { get; set; }

    //public virtual long? ParentId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Portrait { get; set; }

    public virtual Guid? AppUserId { get; set; }

    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    public virtual string Description { get; set; }
}
