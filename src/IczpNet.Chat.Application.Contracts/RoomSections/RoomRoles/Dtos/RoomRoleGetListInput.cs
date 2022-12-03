using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.RoomRoles.Dtos;

public class RoomRoleGetListInput : BaseGetListInput
{

    public virtual Guid? RoomId { get; set; }

    public virtual bool? IsNullRoomId { get; set; }

}
