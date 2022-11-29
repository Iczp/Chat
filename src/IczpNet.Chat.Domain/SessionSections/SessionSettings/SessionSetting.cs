using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionSettings
{

    public class SessionSetting : BaseEntity
    {
        public virtual Guid OwnerId { get; set; }

        public virtual Guid SessionId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { OwnerId, SessionId };
        }
    }
}
