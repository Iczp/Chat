using Castle.DynamicProxy;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections
{
    public abstract class MessageContentEntityBase : BaseEntity<Guid>, IContentEntity, IIsActive, IChatOwner<long?>
    {
        public virtual bool IsActive { get; protected set; }

        public virtual IList<Message> MessageList { get; protected set; } = new List<Message>();

        public virtual long? OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        protected MessageContentEntityBase() { }

        protected MessageContentEntityBase(Guid id) : base(id) { }

        public virtual string GetBody()
        {
            var currentContentType = ProxyUtil.GetUnproxiedType(this);
            var description = MessageTemplateAttribute.GetMessageType(currentContentType).GetDescription();
            return $"[{description}]";
        }

        public virtual string FormatString(string text, int length = 20)
        {
            return text.Length > 20 ? $"{text[..20]}…" : text;
        }

        public virtual long GetSize()
        {
            return 0;
        }
    }
}
