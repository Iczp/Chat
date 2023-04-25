using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitWithDestinationDto
    {
        public virtual Guid Id { get; set; }

        public virtual Guid SessionId { get; set; }

        public virtual long OwnerId { get; set; }

        public virtual ChatObjectDto Destination { get; set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }

        public virtual List<SessionTagDto> TagList { get; set; }
    }
}
