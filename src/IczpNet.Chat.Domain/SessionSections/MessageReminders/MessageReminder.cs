using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.MessageReminders
{
    [Description("消息提醒器@我")]
    public class MessageReminder : BaseEntity
    {
        public virtual Guid SessionUnitId { get; protected set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; protected set; }

        public virtual long MessageId { get; protected set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; protected set; }

        protected MessageReminder() { }

        public MessageReminder(Message message, SessionUnit sessionUnit)
        {
            Message = message;
            SessionUnit = sessionUnit;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, MessageId };
        }
    }
}
