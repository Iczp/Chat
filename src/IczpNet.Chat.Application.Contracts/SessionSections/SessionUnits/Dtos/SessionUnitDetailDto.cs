using System;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitDetailDto : SessionUnitDto
    {
        public virtual long ReadedMessageAutoId { get;  set; }

        public virtual DateTime? HistoryFristTime { get;  set; }

        public virtual DateTime? HistoryLastTime { get;  set; }

        public virtual bool IsKilled { get;  set; }

        public virtual DateTime? ClearTime { get;  set; }

        public virtual DateTime? RemoveTime { get;  set; }

        public virtual double Sorting { get; set; }
    }
}
