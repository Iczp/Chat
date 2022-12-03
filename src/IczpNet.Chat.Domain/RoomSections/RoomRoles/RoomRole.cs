using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.RoomSections.RoomPermissionGrants;
using IczpNet.Chat.RoomSections.RoomRoleRoomMembers;
using IczpNet.Chat.RoomSections.Rooms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.RoomRoles
{
    public class RoomRole : BaseEntity<Guid>, ISorting
    {
        /// <summary>
        /// 群Id
        /// </summary>
        //[Required]
        public virtual Guid? RoomId { get; protected set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [StringLength(40)]
        [Required]
        public virtual string Name { get; protected set; }

        /// <summary>
        /// 编码
        /// </summary>
        [StringLength(40)]
        public virtual string Code { get; set; }

        [StringLength(500)]
        public virtual string Description { get; set; }

        /// <summary>
        /// 排序（越大越前面） DESC
        /// </summary>
        public virtual double Sorting { get; set; }

        /// <summary>
        /// 群
        /// </summary>
        [ForeignKey(nameof(RoomId))]
        public virtual Room Room { get; set; }

        /// <summary>
        /// 群角色
        /// </summary>
        [InverseProperty(nameof(RoomRoleRoomMember.RoomRole))]
        public virtual IList<RoomRoleRoomMember> MemberList { get; set; } = new List<RoomRoleRoomMember>();

        /// <summary>
        /// 被设置为默认角色的群列表（一般只有一个）
        /// </summary>
        [InverseProperty(nameof(Rooms.Room.DefaultRole))]
        public virtual IList<Room> DefaultRoomList { get; set; }

        /// <summary>
        /// 授权列表
        /// </summary>
        public virtual IList<RoomPermissionGrant> GrantList { get; set; }

        protected RoomRole() { }

        public RoomRole(Guid id, Guid roomId, string name, string code, string description) : base(id)
        {
            SetName(name);
            Code = code;
            Description = description;
            RoomId = roomId;
        }

        public int GetMemberCount()
        {
            return MemberList.Count;
        }

        public virtual void SetName(string name)
        {
            Name = name;
        }
    }
}
