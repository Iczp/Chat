using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.FavoritedRecorders.Dtos
{
    public class FavoritedRecorderGetListInput : GetListInput
    {
        public long? OwnerId { get; set; }

        public long? DestinationId { get; set; }

        public virtual long? MinSize { get; set; }

        public virtual long? MaxSize { get; set; }

        public virtual MessageTypes? MessageType { get; set; }
    }
}
