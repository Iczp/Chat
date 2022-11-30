using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
