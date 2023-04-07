using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTagDtos.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDetailDto : SessionUnitDto
    {
        public virtual DateTime? HistoryFristTime { get;  set; }

        public virtual DateTime? HistoryLastTime { get;  set; }

        public virtual DateTime? ClearTime { get;  set; }

        public virtual DateTime? RemoveTime { get;  set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }

        public virtual List<SessionTagDto> TagList { get; set; }
    }
}
