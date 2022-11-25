using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Messages;
using IczpNet.Chat.Robots;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObject : BaseEntity<Guid>, IChatObject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; set; }

        [StringLength(50)]
        public virtual string Name { get; set; }

        [StringLength(50)]
        public virtual string Code { get; set; }

        public virtual ChatObjectType ChatObjectType { get; protected set; }

        [InverseProperty(nameof(Message.Sender))]
        public virtual IList<Message> SenderMessageList { get; set; }

        [InverseProperty(nameof(Message.Receiver))]
        public virtual IList<Message> ReceiverMessageList { get; set; }

        /// <summary>
        /// 兼职店小二
        /// </summary>
        [InverseProperty(nameof(ShopWaiter.ChatObject))]
        public virtual IList<ShopWaiter> PartShopWaiterList { get; set; }

        /// <summary>
        /// 兼职掌柜
        /// </summary>
        [InverseProperty(nameof(ShopKeeper.ChatObject))]
        public virtual IList<ShopKeeper> PartShopKeeperList { get; set; }

        protected ChatObject() { }

        protected ChatObject(Guid id, ChatObjectType chatObjectType) : base(id)
        {
            ChatObjectType = chatObjectType;
        }
    }
}
