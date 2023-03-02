using IczpNet.Chat.Enums;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageWithQuoteInfo<T> : MessageInfo<T>
    {
        public virtual MessageInfo QuoteMessage { get; set; }
    }

    public class MessageWithQuoteInfo : MessageInfo
    {
        public virtual MessageInfo QuoteMessage { get; set; }
    }

    public class MessageInfo<T> : MessageInfo
    {
        public T Content { get; set; }
    }

    [Command("Chat")]
    public class MessageInfo
    {
        public virtual long Id { get; set; }

        //public virtual Guid SessionId { get; set; }

        public virtual long? SenderId { get; set; }

        //public virtual ChatObjectInfo Sender { get; set; }

        public virtual long? ReceiverId { get; set; }

        public virtual MessageTypes MessageType { get; set; }

        public virtual string KeyName { get; set; }

        public virtual string KeyValue { get; set; }

        /// <summary>
        /// 撤回消息时间
        /// </summary>
        public virtual DateTime? RollbackTime { get; set; }

        public virtual DateTime CreationTime { get; set; }
    }
}
