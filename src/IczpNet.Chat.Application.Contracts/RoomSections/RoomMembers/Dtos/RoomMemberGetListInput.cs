using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RoomSections.RoomMembers.Dtos;

public class RoomMemberGetListInput : BaseGetListInput
{
    public virtual Guid? RoomId { get; set; }

    public virtual Guid? OwnerId { get; set; }

    public virtual Guid? RoomRoleId { get; set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    public virtual JoinWayEnum? JoinWay { get; set; }

    /// <summary>
    /// 邀请人
    /// </summary>
    public virtual Guid? InviterId { get; set; }
}
