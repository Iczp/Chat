using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitWithDestinationDto
{
    public virtual Guid Id { get; set; }

    public virtual Guid SessionId { get; set; }

    public virtual long OwnerId { get; set; }

    public virtual ChatObjectDto Destination { get; set; }

    public virtual List<SessionRoleDto> RoleList { get; set; }

    public virtual List<SessionTagDto> TagList { get; set; }
}
