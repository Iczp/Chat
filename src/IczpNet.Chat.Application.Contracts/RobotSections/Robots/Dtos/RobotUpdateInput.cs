using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.RobotSections.Robots.Dtos;

public class RobotUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
