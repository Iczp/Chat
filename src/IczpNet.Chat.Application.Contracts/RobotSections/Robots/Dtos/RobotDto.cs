using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RobotSections.Robots.Dtos;

public class RobotDto : BaseDto<Guid>
{
    public virtual long AutoId { get; set; }

    public virtual string Name { get; set; }

}
