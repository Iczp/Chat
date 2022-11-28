using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.RoomRoleRoomMembers
{
    public class RoomRoleRoomMember : BaseEntity
    {
        public virtual Guid RoomMemberId { get; set; }

        public virtual Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual RoomRole RoomRole { get; set; }

        [ForeignKey(nameof(RoomMemberId))]
        public virtual RoomMember RoomMember { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { RoomMemberId, RoleId };
        }

        protected RoomRoleRoomMember()
        {

        }

        public RoomRoleRoomMember(Guid roomMemberId, Guid roleId)
        {
            RoomMemberId = roomMemberId;
            RoleId = roleId;
        }
    }
}
