using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Messages;
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

        protected ChatObject() { }

        protected ChatObject(Guid id, ChatObjectType chatObjectType) : base(id)
        {
            ChatObjectType = chatObjectType;
        }
    }
}
