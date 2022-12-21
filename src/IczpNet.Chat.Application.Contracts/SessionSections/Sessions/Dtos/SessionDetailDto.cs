using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTagDtos.Dtos;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.Sessions.Dtos
{
    public class SessionDetailDto : SessionDto
    {
        public virtual List<SessionTagDto> TagList { get; set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }
    }
}
