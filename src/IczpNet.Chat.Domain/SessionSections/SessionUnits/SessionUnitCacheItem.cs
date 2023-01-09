using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    [Serializable]
    public class SessionUnitCacheItem
    {
        public virtual Guid SessionId { get; set; }

        public virtual ChatObjectTypes? DestinationObjectType { get; set; }

        public virtual Guid OwnerId { get; set; }

        public virtual Guid? DestinationId { get; set; }

        public virtual long ReadedMessageAutoId { get; set; }

        public virtual DateTime? HistoryFristTime { get; set; }

        public virtual DateTime? HistoryLastTime { get; set; }

        public virtual bool IsKilled { get; set; }

        public virtual KillTypes? KillType { get; set; }

        public virtual DateTime? KillTime { get; set; }

        public virtual Guid? KillerId { get; set; }

        public virtual DateTime? ClearTime { get; set; }

        public virtual DateTime? RemoveTime { get; set; }

        public virtual string Name { get; set; }

        public virtual JoinWays? JoinWay { get; set; }

        public virtual Guid? InviterId { get; set; }

        public virtual double Sorting { get; set; }
    }
}
