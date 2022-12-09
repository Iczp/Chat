using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections
{
    public abstract class MessageContentEntityBase : BaseEntity<Guid>, IMessageContentEntity, IIsActive, IChatOwner<Guid?>
    {
        public virtual bool IsActive { get; protected set; }

        public virtual IList<Message> MessageList { get; protected set; } = new List<Message>();

        public virtual Guid? OwnerId { get; protected set; }

        public virtual ChatObject Owner { get; protected set; }

        protected MessageContentEntityBase() { }

        protected MessageContentEntityBase(Guid id) : base(id) { }
    }
}
