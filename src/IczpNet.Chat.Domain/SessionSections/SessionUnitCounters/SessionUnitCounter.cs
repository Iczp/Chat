using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters
{
    public class SessionUnitCounter : Entity, IHasCreationTime, IHasModificationTime
    {
        //protected SessionUnitCounter() { }

        public virtual Guid SessionUnitId { get; set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; set; }

        public virtual long? LastMessageId { get; set; }

        [ForeignKey(nameof(LastMessageId))]
        public virtual Message LastMessage { get; set; }

        public virtual int PublicBadge { get; protected set; }

        public virtual int PrivateBadge { get; protected set; }

        public virtual int RemindAllCount { get; protected set; }

        public virtual int RemindMeCount { get; protected set; }

        public virtual int FollowingCount { get; protected set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual DateTime? LastModificationTime { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId };
        }
    }
}
