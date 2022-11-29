using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Robots
{
    public class ShopWaiter : ChatObject
    {
        public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.ShopWaiter;

        [StringLength(20)]
        public virtual string NickName { get; set; }

        public virtual Guid? ShopKeeperId { get; set; }

        public virtual Guid? ChatObjectId { get; set; }

        [ForeignKey(nameof(ShopKeeperId))]
        public virtual ShopKeeper ShopKeeper { get; set; }

        [ForeignKey(nameof(ChatObjectId))]
        public virtual ChatObject ChatObject { get; set; }

        protected ShopWaiter()
        {
            ChatObjectType = ChatObjectTypeValue;
        }

        protected ShopWaiter(Guid id, ShopKeeper shopKeeper, ChatObject chatObject) : base(id, ChatObjectTypeValue)
        {
            Assert.If(chatObject.ChatObjectType == ChatObjectTypeEnum.ShopWaiter, "");
            ShopKeeper = shopKeeper;
            ChatObject = chatObject;
        }
    }
}
