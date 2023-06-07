using IczpNet.Chat.Entrys.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDestinationDetailDto : SessionUnitDestinationDto
    {
        //public virtual DateTime? HistoryFristTime { get;  set; }

        //public virtual DateTime? HistoryLastTime { get;  set; }

        //public virtual DateTime? ClearTime { get;  set; }

        //public virtual DateTime? RemoveTime { get;  set; }

        public virtual List<EntryObjectDto> Entries { get; set; }
    }
}
