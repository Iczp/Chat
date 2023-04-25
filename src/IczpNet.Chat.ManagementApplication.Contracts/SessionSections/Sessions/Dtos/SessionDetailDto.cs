using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using System.Collections.Generic;

namespace IczpNet.Chat.Management.SessionSections.Sessions.Dtos
{
    public class SessionDetailDto : SessionDto
    {
        public virtual List<SessionTagDto> TagList { get; set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }
    }
}
