using IczpNet.Chat.ChatObjects;
using System;
using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.Robots
{
    public class ShopKeeper : ChatObject
    {
        public const ChatObjectType ChatObjectTypeValue = ChatObjectType.ShopKeeper;

        public virtual IList<ShopWaiter> ShopWaiterList { get; set; }

        protected ShopKeeper()
        {
            ChatObjectType = ChatObjectTypeValue;
        }
        protected ShopKeeper(Guid id) : base(id, ChatObjectTypeValue)
        {

        }
    }
}
