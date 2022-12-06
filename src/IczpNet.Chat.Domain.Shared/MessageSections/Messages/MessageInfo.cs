using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageInfo
    {
       
        public virtual string SessionId { get; protected set; }

        /// <summary>
        /// 发送者
        /// </summary>
        public virtual Guid? SenderId { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        public virtual Guid? ReceiverId { get; set; }

        /// <summary>
        /// 消息通道
        /// </summary>
        public virtual MessageChannelEnum MessageChannel { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public virtual MessageTypeEnum MessageType { get; set; }

        public virtual string ContentJson { get; protected set; }

        /// <summary>
        /// 扩展（键名）根据业务自义，如:"courseId"、"course-userId"、"erp-userId"
        /// </summary>
        public virtual string KeyName { get; set; }

        /// <summary>
        /// 扩展（键值）根据业务自义,如："123456789"、"02b7d668-02ca-428f-b88c-b8adac2c5044"、"admin"
        /// </summary>
        public virtual string KeyValue { get; set; }

        /// <summary>
        /// 撤回消息时间
        /// </summary>
        public virtual DateTime? RollbackTime { get; set; }

        public virtual ChatObjectSimpleInfo Sender { get; set; }

        public virtual ChatObjectSimpleInfo Receiver { get; set; }
    }
}
