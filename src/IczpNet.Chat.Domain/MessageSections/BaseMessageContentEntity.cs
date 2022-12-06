using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections
{
    public abstract class BaseMessageContentEntity : BaseEntity<Guid>, IMessageContent, IIsActive, IChatOwner<Guid?>
    {
        public virtual bool IsActive { get; set; }

        public virtual IList<Message> MessageList { get; set; }

        public virtual Guid? OwnerId { get; set; }

        public virtual ChatObject Owner { get; set; }
    }
}
