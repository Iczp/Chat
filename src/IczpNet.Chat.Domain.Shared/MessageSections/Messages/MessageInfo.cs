using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Messages
{

    public class MessageInfo<T> : MessageInfo
    {
        public T Content { get; set; }
    }

    public class MessageInfo
    {
        public virtual Guid Id { get; set; }

        public virtual long AutoId { get; set; }

        public virtual string SessionId { get; set; }

        /// <summary>
        /// 发送者
        /// </summary>
        //public virtual Guid? SenderId { get; set; }
        public virtual ChatObjectInfo Sender { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        public virtual Guid? ReceiverId { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public virtual MessageTypes MessageType { get; set; }

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
    }
}
