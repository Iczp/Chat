using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageInput<T> : MessageInput //where T : class, IMessageContentInfo
    {
        public virtual T Content { get; set; }
    }

    public class MessageInput : IMessageInput 
    {
        //public virtual IMessageContentInfo Content { get; set; }

        /// <summary>
        /// 发送者
        /// </summary>
        public virtual Guid SenderId { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        public virtual Guid ReceiverId { get; set; }

        ///// <summary>
        ///// 消息类型
        ///// </summary>
        //public virtual MessageTypes MessageType { get; set; }

        /// <summary>
        /// 扩展（键名）根据业务自义，如:"courseId"、"course-userId"、"erp-userId"
        /// </summary>
        public virtual string KeyName { get; set; }

        /// <summary>
        /// 扩展（键值）根据业务自义,如："123456789"、"02b7d668-02ca-428f-b88c-b8adac2c5044"、"admin"
        /// </summary>
        public virtual string KeyValue { get; set; }

        public virtual Guid? QuoteMessageId { get; set; }

        public virtual List<string> IgnoreConnections { get; set; }
    }
}
