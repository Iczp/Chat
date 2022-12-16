using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class Session : BaseEntity<Guid>
    {
        [StringLength(80)]
        public virtual string SessionKey { get; set; }

        [StringLength(50)]
        public virtual string Title { get; set; }

        [StringLength(100)]
        public virtual string Description { get; set; }

        //public virtual Guid MessageId { get; set; }

        public virtual List<Message> MessageList { get; set; }

        public virtual List<SessionMember> MemberList { get; set; } = new List<SessionMember>();

        protected Session() { }

        public Session(Guid id, string sessionKey) : base(id)
        {
            SessionKey = sessionKey;
        }

        public int GetMemberCount()
        {
            return MemberList.Count;
        }

        public int GetUnreadCount()
        {
            return MemberList.Count;
        }
    }
}
