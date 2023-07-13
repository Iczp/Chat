using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters
{
    [Index(nameof(LastMessageId), AllDescending = true, Name = $"IX_Chat_{nameof(SessionUnitCounter)}_{nameof(LastMessageId)}_Desc")]
    [Index(nameof(LastMessageId), AllDescending = false)]
    [Index(nameof(SessionUnitId), nameof(LastMessageId), AllDescending = true)]

    [Index(nameof(SessionUnitId), IsUnique = true)]
    public class SessionUnitCounter : BaseEntity, IHasCreationTime, IHasModificationTime, ISoftDelete
    {
        //protected Counters() { }

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

        [Comment("创建时间")]
        public override DateTime CreationTime { get; protected set; }

        [Comment("修改时间")]
        public override DateTime? LastModificationTime { get; set; }

        public virtual bool IsDeleted { get; protected set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId };
        }
    }
}
