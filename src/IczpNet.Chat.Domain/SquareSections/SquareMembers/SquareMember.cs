using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SquareSections.Squares;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SquareSections.SquareMembers
{
    public class SquareMember : BaseEntity<Guid>
    {

        public virtual Guid SquareId { get; set; }

        [ForeignKey(nameof(SquareId))]
        public virtual Square Square { get; set; }

        public virtual Guid ChatObjectId { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        /// <summary>
        /// 群里显示名称
        /// </summary>
        [StringLength(40)]
        public virtual string MemberName { get; set; }
        /// <summary>
        /// 群历史消息的读取起始时间 HistoryFirstTime
        /// </summary>
        public virtual DateTime HistoryFirstTime { get; protected set; }

    }
}
