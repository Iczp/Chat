using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class Session : BaseEntity<Guid>, IChatOwner<Guid?>
    {
        [StringLength(80)]
        public virtual string SessionKey { get; protected set; }

        [StringLength(36)]
        public virtual string Channel { get; protected set; }

        [StringLength(50)]
        public virtual string Title { get; set; }

        [StringLength(100)]
        public virtual string Description { get; set; }

        public virtual Guid? OwnerId { get; protected set; }

        public virtual ChatObject Owner { get; protected set; }

        public virtual List<Message> MessageList { get; set; } = new List<Message>();

        public virtual IList<SessionUnit> UnitList { get; set; } = new List<SessionUnit>();

        protected Session() { }

        public Session(Guid id, string sessionKey) : base(id)
        {
            SessionKey = sessionKey;
        }

        public int GetMemberCount()
        {
            return UnitList.Count;
        }

        public int GetUnreadCount()
        {
            return MessageList.Count(x => x.Session.UnitList.Any(d => d.OwnerId != x.SenderId));
        }

        public int GetBadge()
        {
            return MessageList.Count(x => x.Session.UnitList.Any(d => d.OwnerId != x.SenderId));
        }

        public Message GetLastMessage()
        {
            return MessageList.FirstOrDefault(x => x.AutoId == MessageList.Max(d => d.AutoId));
        }
    }
}
