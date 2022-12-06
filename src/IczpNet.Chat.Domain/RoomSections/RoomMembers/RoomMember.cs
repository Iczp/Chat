using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.Rooms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.RoomSections.RoomMembers
{
    public class RoomMember : BaseEntity<Guid>, IChatOwner<Guid>
    {
        /// <summary>
        /// 群Id
        /// </summary>
        public virtual Guid RoomId { get; protected set; }

        /// <summary>
        /// 成员Id
        /// </summary>
        public virtual Guid OwnerId { get; protected set; }

        /// <summary>
        /// 群角色（群主，管理员，成员）
        /// </summary>
        public virtual RoomRoleTypes RoomRoleType { get; set; }

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
        public virtual JoinWays JoinWay { get; set; }

        /// <summary>
        /// 邀请人
        /// </summary>
        public virtual Guid? InviterId { get; set; }

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
        /// 所在的群
        /// </summary>
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        [ForeignKey(nameof(InviterId))]
        public virtual ChatObject Inviter { get; set; }

        /// <summary>
        /// 成员群色
        /// </summary>
        [InverseProperty(nameof(RoomRoleRoomMember.RoomMember))]
        public virtual IList<RoomRoleRoomMember> MemberRoleList { get; set; } = new List<RoomRoleRoomMember>();


        protected RoomMember() { }

        public RoomMember(Guid id, Guid roomId, Guid ownerId, Guid? inviterId) : base(id)
        {
            RoomId = roomId;
            OwnerId = ownerId;
            InviterId = inviterId;
        }
        public virtual List<RoomRole> GetRoleList()
        {
            return MemberRoleList.Select(x => x.RoomRole).ToList();
        }
    }
}
