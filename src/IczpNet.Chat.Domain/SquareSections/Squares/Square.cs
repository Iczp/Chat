using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareMembers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SquareSections.Squares
{
    public class Square : ChatObject
    {
        public virtual Guid? CategoryId { get; set; }

        public virtual SquareTypes Type { get; set; }

        /// <summary>
        /// 会话Id
        /// </summary>
        public virtual Guid? SessionId { get; protected set; }

        /// <summary>
        /// 会话
        /// </summary>
        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual SquareCategory Category { get; set; }

        [InverseProperty(nameof(SquareMember.Square))]
        public virtual IList<SquareMember> SquareMemberList { get; set; } = new List<SquareMember>();

        public int GetMemberCount()
        {
            return SquareMemberList.Count;
        }
    }
}
