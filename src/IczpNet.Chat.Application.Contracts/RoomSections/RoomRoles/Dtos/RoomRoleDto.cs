using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.RoomSections.RoomRoles.Dtos;

public class RoomRoleDto : BaseDto<Guid>
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual int MemberCount { get; set; }

}
