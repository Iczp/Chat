using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public class Room : ChatObject
    {
        public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.Room;

        /// <summary>
        /// 全称(不可修改)
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string FullName { get; set; }

        /// <summary>
        /// 原始名称(不可修改)
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string OriginalName { get; set; }

        /// <summary>
        /// 群类型（自由群、职位群）
        /// </summary>
        public virtual RoomTypeEnum Type { get; set; }

        /// <summary>
        /// 成员名称的显示方式
        /// </summary>
        public virtual MemberNameDisplayModeEnum MemberNameDisplayMode { get; protected set; }

        /// <summary>
        /// 头像的显示方式
        /// </summary>
        public virtual PortraitDisplayModeEnum PortraitDisplayMode { get; protected set; }

        ///// <summary>
        ///// 扩展Id
        ///// </summary>
        //public virtual Guid? ExtendId { get; set; }
        /// <summary>
        /// 群聊头像
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string Portrait { get; protected set; }

        /// <summary>
        /// 聊天背景，默认为 null
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 群说明
        /// </summary>
        [StringLength(500)]
        [MaxLength(500)]
        public virtual string Description { get; set; }

        /// <summary>
        /// 管理员(可以多个，用“,”分隔)  (废除，--从成员列表里找群主)
        /// </summary>
        [StringLength(500)]
        [MaxLength(500)]
        public virtual string ManagerUserIdList { get; set; }

        /// <summary>
        /// 群拥有者 OwnerUserId (群主)
        /// </summary>
        //[MaxLength(36)]
        //[StringLength(36)]
        //[Column("CreatorUserId")]
        public virtual Guid? OwnerChatObjectId { get; protected set; }

        /// <summary>
        /// 默认群角色
        /// </summary>
        public virtual Guid? DefaultRoleId { get; protected set; }

        /// <summary>
        /// 入群邀请方式
        /// </summary>
        public virtual InvitationMethodEnum InvitationMethod { get; set; }

        /// <summary>
        /// 是否可以设置消息免打扰
        /// </summary>
        public virtual bool IsCanSetImmersed { get; set; }

        /// <summary>
        /// 是否可以设置聊天背景
        /// </summary>
        public virtual bool IsCanSetBackground { get; set; }

        /// <summary>
        /// 全体禁言（除群主及管理员）
        /// </summary>
        public virtual bool IsForbiddenInput { get; set; } = false;

        /// <summary>
        /// 是否允许自动加入群（当不是群成员的时） Allow to automatically join the room(when not a member of the room)
        /// 在进群聊天时，还不是群成的时，允许自动加入群（课程群有这个需求）
        /// </summary>
        public virtual bool IsAllowAutoJoin { get; set; } = false;

        /// <summary>
        /// 群主
        /// </summary>
        [ForeignKey(nameof(OwnerChatObjectId))]
        public virtual ChatObject OwnerChatObject { get; set; }

        /// <summary>
        /// 群成员
        /// </summary>
        public virtual IList<RoomMember> RoomMemberList { get; protected set; } = new List<RoomMember>();

        /// <summary>
        /// 被禁言的成员
        /// </summary>
        public virtual IList<RoomForbiddenMember> RoomForbiddenMemberList { get; set; }

        /// <summary>
        /// 默认群角色
        /// </summary>
        [ForeignKey(nameof(DefaultRoleId))]
        public virtual RoomRole DefaultRole { get; set; }

        /// <summary>
        /// 群角色
        /// </summary>
        public virtual IList<RoomRole> RoleList { get; set; }



        protected Room()
        {
            ObjectType = ChatObjectTypeValue;
        }
        protected Room(Guid id) : base(id, ChatObjectTypeValue)
        {

        }

        public int GetMemberCount()
        {
            return RoomMemberList.Count;
        }
    }
}
