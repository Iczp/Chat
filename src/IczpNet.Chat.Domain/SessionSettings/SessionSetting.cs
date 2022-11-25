using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Sessions;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSettings
{
    public class SessionSetting : BaseEntity
    {
        public virtual Guid ChatObjectId { get; set; }

        public virtual Guid SessionId { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { ChatObjectId, SessionId };
        }
    }
}
