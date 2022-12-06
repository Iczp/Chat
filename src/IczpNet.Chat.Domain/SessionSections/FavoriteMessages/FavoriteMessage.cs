using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using System;

namespace IczpNet.Chat.SessionSections.Favorites
{
    public class FavoriteMessage : BaseEntity
    {
        public virtual Guid FavoriteId { get; set; }

        public virtual Favorite Favorite { get; set; }

        public virtual Guid MessageId { get; set; }

        public virtual Message Message { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { FavoriteId, MessageId };
        }
    }
}
