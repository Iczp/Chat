using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Messages;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections
{
    public abstract class MessageContent : BaseEntity<Guid>, IMessageContent, IIsActive
    {
        public virtual bool IsActive { get; set; }

        public virtual IList<Message> MessageList { get; set; }
    }
}
