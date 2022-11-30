using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Rooms.Dtos
{
    public class RoomDto : BaseDto<Guid>
    {
        public virtual long AutoId { get; set; }

        public virtual string Name { get; set; }

        public virtual string Code { get; set; }

        public virtual int MemberCount { get; set; }
    }
}
