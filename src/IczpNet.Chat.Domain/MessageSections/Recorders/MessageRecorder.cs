using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.MessageSections.Recorders
{
    public abstract class MessageRecorder : Entity, IHasCreationTime, IHasModificationTime
    {
        public virtual long MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; }

        public virtual long Value { get; set; }

        public virtual DateTime CreationTime { get; set; }

        public virtual DateTime? LastModificationTime { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { MessageId };
        }
    }
}
