using System.Collections.Generic;

namespace IczpNet.Chat.RoomSections.Rooms
{
    public class InviteInput
    {
        public virtual long RoomId { get; set; }

        public virtual List<long> MemberIdList { get; set; }

        public virtual long? InviterId { get; set; }
    }
}
