using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.MessageSections.Counters
{
    public abstract class MessageCounter : Entity, IHasCreationTime, IHasModificationTime, ISoftDelete
    {
        public virtual long MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; }

        public virtual long Count { get; set; }

        public virtual DateTime CreationTime { get; protected set; }

        public virtual DateTime? LastModificationTime { get; set; }

        public virtual bool IsDeleted { get; protected set; }

        public override object[] GetKeys()
        {
            return new object[] { MessageId };
        }
    }
}
