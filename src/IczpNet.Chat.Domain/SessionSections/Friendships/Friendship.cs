using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Friends
{
    public class Friendship : BaseEntity<Guid>, IIsActive
    {
        public virtual Guid OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        public virtual Guid? FriendId { get; set; }

        [ForeignKey(nameof(FriendId))]
        public virtual ChatObject Friend { get; set; }

        /// <summary>
        /// 备注名称
        /// </summary>
        [StringLength(50)]
        public virtual string Rename { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public virtual string Remarks { get; set; }

        /// <summary>
        /// 是否保存通讯录(群)
        /// </summary>
        public virtual bool IsCantacts { get; set; }

        /// <summary>
        /// 排序,消息置顶时间，不置顶则为空
        /// </summary>
        public virtual long? SortingNumber { get; set; }

        /// <summary>
        /// 消息免打扰，默认为 false
        /// </summary>
        public virtual bool IsImmersed { get; set; }

        /// <summary>
        /// 是否显示成员名称
        /// </summary>
        public virtual bool IsShowMemberName { get; set; }

        /// <summary>
        /// 是否显示已读
        /// </summary>
        public virtual bool IsShowRead { get; set; }

        /// <summary>
        /// 聊天背景，默认为 null
        /// </summary>
        [StringLength(500)]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 是否有效的
        /// </summary>
        public virtual bool IsActive { get; set; }
    }
}
