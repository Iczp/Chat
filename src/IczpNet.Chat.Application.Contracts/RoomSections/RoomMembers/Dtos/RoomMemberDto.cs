using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.RoomSections.RoomMembers.Dtos;

public class RoomMemberDto : BaseDto<Guid>
{
    public List<RoomRoleDto> RoleList { get; set; }
}
