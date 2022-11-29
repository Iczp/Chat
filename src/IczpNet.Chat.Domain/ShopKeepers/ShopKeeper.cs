using IczpNet.Chat.ChatObjects;
using System;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Robots
{
    public class ShopKeeper : ChatObject
    {
        public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.ShopKeeper;

        public virtual Guid? ChatObjectId { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        [InverseProperty(nameof(ShopWaiter.ShopKeeper))]
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
