using System;

namespace IczpNet.Chat.RoomSections.RoomRoles.Dtos;

public class RoomRoleCreateInput 
{
    public virtual Guid RoomId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
