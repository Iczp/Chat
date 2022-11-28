using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Messages;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class HistoryMessage : BaseEntity
    {
        public virtual Guid MessageId { set; get; }

        public virtual Guid HistoryContentId { set; get; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { set; get; }

        [ForeignKey(nameof(HistoryContentId))]
        public virtual HistoryContent HistoryContent { set; get; }

        public override object[] GetKeys()
        {
            return new object[] { MessageId, HistoryContentId };
        }
    }
}
