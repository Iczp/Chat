using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.SquareMembers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SquareSections.Squares
{
    public class Square : ChatObject
    {
        public virtual Guid? SquareCategoryId { get; set; }

        [ForeignKey(nameof(SquareCategoryId))]
        public virtual SquareCategory SquareCategory { get; set; }

        [InverseProperty(nameof(SquareMember.Square))]
        public virtual IList<SquareMember> SquareMemberList { get; set; }
    }
}
