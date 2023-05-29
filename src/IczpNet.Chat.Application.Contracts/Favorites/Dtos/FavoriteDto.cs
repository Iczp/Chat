using System;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages.Dtos;

namespace IczpNet.Chat.FavoriteRecorders.Dtos
{
    public class FavoriteDto
    {
        //public virtual long MessageId { get; set; }

        public virtual long Size { get; set; }

        public virtual MessageTypes MessageType { get; set; }

        public virtual MessageFavoriteDto Message { get; set; }

        public virtual Guid SessionUnitId { get; set; }

        public virtual long? OwnerId { get; set; }

        public virtual long? DestinationId { get; set; }

        public virtual string DeviceId { get; set; }

        public virtual DateTime CreationTime { get; set; }
    }
}
