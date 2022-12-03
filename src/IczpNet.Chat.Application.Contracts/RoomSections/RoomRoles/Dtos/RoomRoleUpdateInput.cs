using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.RoomSections.RoomRoles.Dtos;

public class RoomRoleUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
