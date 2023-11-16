using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Entries.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    public class SessionUnitDestinationDetailDto : SessionUnitDestinationDto
    {
        //public virtual DateTime? HistoryFristTime { get;  set; }

        //public virtual DateTime? HistoryLastTime { get;  set; }

        //public virtual DateTime? ClearTime { get;  set; }

        //public virtual DateTime? RemoveTime { get;  set; }

        public virtual List<SessionRoleDto> RoleList { get; set; }

        public virtual List<SessionTagDto> TagList { get; set; }

        public virtual ChatObjectDto Destination { get; set; }

        public virtual List<EntryObjectDto> Entries { get; set; }
    }
}
