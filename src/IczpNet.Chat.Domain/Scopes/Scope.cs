using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;

namespace IczpNet.Chat.Scopeds
{
    public class Scoped : BaseEntity
    {
        protected Scoped() { }

        public Guid SessionUnitId { get; set; }

        public SessionUnit SessionUnit { get; set; }

        public long MessageId { get; set; }

        public Message Message { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, MessageId };
        }
    }
}
