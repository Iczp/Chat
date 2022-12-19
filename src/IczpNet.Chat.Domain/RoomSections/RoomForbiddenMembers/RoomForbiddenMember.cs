using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.RoomSections.Rooms;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.RoomForbiddenMembers
{
    public class RoomForbiddenMember : BaseEntity<Guid>, IChatOwner<Guid>
    {
        /// <summary>
        /// 群Id
        /// </summary>
        public virtual Guid RoomId { get; set; }

        /// <summary>
        /// 禁言的群成员
        /// </summary>
        //[StringLength(36)]
        public virtual Guid OwnerId { get; set; }

        /// <summary>
        /// 操作员
        /// </summary>
        [StringLength(36)]
        public virtual Guid OperatorChatObjectId { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(500)]
        public virtual string Description { get; set; }

        /// <summary>
        /// 激活状态
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// 禁言过期时间
        /// </summary>
        public virtual DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 所在的群
        /// </summary>
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }

        /// <summary>
        /// 所在的群
        /// </summary>
        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        /// <summary>
        /// 构造器
        /// </summary>
        protected RoomForbiddenMember()
        {
            IsActive = true;
        }
    }
}
