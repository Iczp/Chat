using IczpNet.Chat.Attributes;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.RoomRoleRoomMembers
{

    [HasKey(nameof(RoomMemberId), nameof(RoomRoleId))]
    public class RoomRoleRoomMember : BaseEntity
    {
        public virtual Guid RoomMemberId { get; set; }

        public virtual Guid RoomRoleId { get; set; }

        [ForeignKey(nameof(RoomRoleId))]
        public virtual RoomRole RoomRole { get; set; }

        [ForeignKey(nameof(RoomMemberId))]
        public virtual RoomMember RoomMember { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { RoomMemberId, RoomRoleId };
        }

        protected RoomRoleRoomMember()
        {

        }

        public RoomRoleRoomMember(Guid roomMemberId, Guid roomRoleId)
        {
            RoomMemberId = roomMemberId;
            RoomRoleId = roomRoleId;
        }
    }
}
