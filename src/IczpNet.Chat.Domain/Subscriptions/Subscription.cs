using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Subscriptions
{
    public class Subscription : ChatObject
    {
        /// <summary>
        /// 会话Id
        /// </summary>
        public virtual Guid? SessionId { get; protected set; }

        /// <summary>
        /// 会话
        /// </summary>
        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        [NotMapped]
        public int? MemberCount=> Session?.UnitList.Count;
    }
}
