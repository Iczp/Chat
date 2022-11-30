using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Messages.Dtos
{
    public class MessageDto : BaseDto<Guid>
    {
        public virtual long AutoId { get; set; }

        public virtual string SessionId { get;  set; }

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

        //public virtual string Content { get;  set; }

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

        /// <summary>
        /// 转发来源Id(转发才有)
        /// </summary>
        public virtual Guid? ForwardMessageId { get; set; }

        /// <summary>
        /// 转发层级 0:不是转发
        /// </summary>
        public virtual long ForwardDepth { get; set; }

        /// <summary>
        /// 引用来源Id(引用才有)
        /// </summary>
        public virtual Guid? QuoteMessageId { get; set; }

        /// <summary>
        /// 引用层级 0:不是引用
        /// </summary>
        public virtual long QuoteDepth { get; set; }
    }
}
