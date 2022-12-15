using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.MessageReminders
{
    [Description("消息提醒器@我")]
    public class MessageReminder : BaseEntity, IChatOwner<Guid>
    {
        public virtual Guid OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual Guid MessageId { get; protected set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; protected set; }

        protected MessageReminder() { }

        public MessageReminder(Message message, ChatObject owner)
        {
            Message = message;
            Owner = owner;
        }

        public override object[] GetKeys()
        {
            return new object[] { OwnerId, MessageId };
        }
    }
}
