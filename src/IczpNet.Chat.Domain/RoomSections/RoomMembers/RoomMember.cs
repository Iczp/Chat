using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.RoomSections.Rooms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.RoomMembers
{
    public class RoomMember : BaseEntity<Guid>, IOwner<Guid>
    {
        /// <summary>
        /// 群Id
        /// </summary>
        public virtual Guid RoomId { get; protected set; }

        public virtual Guid OwnerId { get; set; }

        /// <summary>
        /// 成员Id
        /// </summary>
        public virtual Guid OwnerUserId { get; protected set; }

        /// <summary>
        /// 群角色（群主，管理员，成员）
        /// </summary>
        public virtual RoomRoleEnum RoomRole { get; set; }
        /// <summary>
        /// 群里显示名称
        /// </summary>
        [StringLength(40)]
        public virtual string MemberName { get; set; }
        /// <summary>
        /// 群历史消息的读取起始时间 HistoryFirstTime
        /// </summary>
        public virtual DateTime HistoryFirstTime { get; protected set; }
        /// <summary>
        /// 加入方式
        /// </summary>
        public virtual JoinWayEnum JoinWay { get; set; }
        /// <summary>
        /// 邀请人
        /// </summary>
        public virtual Guid? InviterUserId { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(500)]
        public virtual string Description { get; set; }
        /// <summary>
        /// 禁言过期时间
        /// </summary>
        public virtual DateTime? InputForbiddenExpireTime { get; set; }
        /// <summary>
        /// 职务id 
        /// </summary>
        [StringLength(36)]
        public virtual string PositionId { get; set; }
        /// <summary>
        ///  职级
        /// </summary>
        [StringLength(36)]
        public virtual string Grade { get; set; }
        /// <summary>
        /// 部门id
        /// </summary>
        [StringLength(36)]
        public virtual string DepartmentId { get; set; }

        /// <summary>
        /// 所在的群
        /// </summary>
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        ///// <summary>
        ///// 成员角色
        ///// </summary>
        //public virtual IList<RoomRole> RoomRoleList { get; set; }
        /// <summary>
        /// 成员群色
        /// </summary>
        [InverseProperty(nameof(RoomRoleRoomMember.RoomMember))]
        public virtual IList<RoomRoleRoomMember> RoleList { get; set; } = new List<RoomRoleRoomMember>();

    }
}
