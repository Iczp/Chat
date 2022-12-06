using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.Favorites
{
    public class Favorite : BaseEntity<Guid>, IChatOwner<Guid>
    {
        public virtual Guid OwnerId { get; set; }

        public virtual ChatObject Owner { get; set; }

        /// <summary>
        /// 收藏的消息
        /// </summary>
        public virtual IList<FavoriteMessage> FavoriteMessageList { get; set; }
    }
}
