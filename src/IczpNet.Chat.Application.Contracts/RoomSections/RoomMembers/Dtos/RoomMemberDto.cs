using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.RoomSections.RoomMembers.Dtos;

public class RoomMemberDto : BaseDto<Guid>
{
    /// <summary>
    /// 群里显示名称
    /// </summary>
    public virtual string MemberName { get; set; }

    /// <summary>
    /// 群历史消息的读取起始时间 HistoryFirstTime
    /// </summary>
    public virtual DateTime HistoryFirstTime { get; protected set; }

    /// <summary>
    /// 加入方式
    /// </summary>
    public virtual JoinWayEnum JoinWay { get; set; }

    /// <summary>
    /// 邀请人
    /// </summary>
    public virtual Guid? InviterId { get; set; }

    public List<RoomRoleDto> RoleList { get; set; }
}
