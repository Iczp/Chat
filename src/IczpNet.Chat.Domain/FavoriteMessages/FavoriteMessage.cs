using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using System;

namespace IczpNet.Chat.FavoriteMessages
{
    public class FavoriteMessage : BaseEntity
    {
        public virtual Guid FavoriteId { get; set; }

        public virtual Favorite Favorite { get; set; }

        public virtual long MessageId { get; set; }

        public virtual Message Message { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { FavoriteId, MessageId };
        }
    }
}
