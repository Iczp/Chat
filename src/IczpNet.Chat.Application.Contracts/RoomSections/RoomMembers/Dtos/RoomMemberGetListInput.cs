using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.RoomMembers.Dtos;

public class RoomMemberGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

    public virtual Guid? RoomId { get; set; }

    public virtual Guid? RoomRoleId { get; set; }
}
