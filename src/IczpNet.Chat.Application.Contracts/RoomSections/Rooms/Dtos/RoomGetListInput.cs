using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.Rooms.Dtos;

public class RoomGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

    public virtual RoomTypeEnum? Type { get; set; }


}
