using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.Rooms.Dtos
{
    public class RoomDto : BaseDto
    {
        public virtual string Name { get; set; }

        public virtual string Code { get; set; }
    }
}
