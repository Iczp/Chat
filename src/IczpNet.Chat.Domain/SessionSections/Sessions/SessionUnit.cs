using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnit : BaseEntity, IChatOwner<Guid>
    {
        public virtual Guid SessionId { get; protected set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; protected set; }

        public virtual Guid OwnerId { get; protected set; }
        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        public virtual Guid? DestinationId { get; protected set; }

        [ForeignKey(nameof(DestinationId))]
        public virtual ChatObject Destination { get; protected set; }

        public virtual long ReadedMessageAutoId { get; protected set; }

        public virtual DateTime HistoryFristTime { get; protected set; }

        public virtual string Name { get; set; }

        protected SessionUnit() { }

        public SessionUnit(Guid sessionId, Guid ownerId, Guid destinationId)
        {
            SessionId = sessionId;
            OwnerId = ownerId;
            DestinationId = destinationId;
        }

        internal void SetReaded(long messageAutoId)
        {
            ReadedMessageAutoId = messageAutoId;
        }

        internal void SetHistoryFristTime(DateTime historyFristTime)
        {
            HistoryFristTime = historyFristTime;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionId, OwnerId };
        }

        public virtual int GetBadge()
        {
            return Session.MessageList.Count(x => x.AutoId > ReadedMessageAutoId && x.SenderId != OwnerId && x.CreationTime > HistoryFristTime);
        }

        public virtual Message GetLastMessage()
        {
            return Session.MessageList.FirstOrDefault(x => x.AutoId == Session.MessageList.Max(d => d.AutoId));
        }
    }
}
