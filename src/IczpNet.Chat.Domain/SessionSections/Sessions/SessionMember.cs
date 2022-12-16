using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionMember : BaseEntity, IChatOwner<Guid>
    {

        public virtual Guid SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        public virtual Guid OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        public virtual string Name { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionId, OwnerId };
        }
    }
}
