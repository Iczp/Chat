using AutoMapper;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareMembers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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
