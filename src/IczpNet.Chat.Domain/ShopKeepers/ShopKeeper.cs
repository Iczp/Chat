using IczpNet.Chat.ChatObjects;
using System;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using IczpNet.Chat.DataFilters;

namespace IczpNet.Chat.Robots
{
    public class ShopKeeper : ChatObject, IChatOwner<Guid?>
    {
        public const ChatObjectTypeEnums ChatObjectTypeValue = ChatObjectTypeEnums.ShopKeeper;

        public virtual Guid? OwnerId { get; set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; set; }

        [InverseProperty(nameof(ShopWaiter.ShopKeeper))]
        public virtual IList<ShopWaiter> ShopWaiterList { get; set; }

        protected ShopKeeper()
        {
            ObjectType = ChatObjectTypeValue;
        }
        protected ShopKeeper(Guid id) : base(id, ChatObjectTypeValue)
        {

        }
    }
}
