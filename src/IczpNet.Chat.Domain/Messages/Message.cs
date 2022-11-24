using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Messages
{
    public class Message : BaseEntity<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; set; }

        //[Required]
        public virtual Guid? SenderId { get; set; }

        //[Required]
        public virtual Guid? ReceiverId { get; set; }

        public virtual MessageChannel MessageChannel { get; set; }

        public virtual MessageType MessageType { get; set; }

        /// <summary>
        /// 转发来源Id(转发才有)
        /// </summary>
        public virtual Guid? ForwardMessageId { get; protected set; }
        /// <summary>
        /// 引用来源Id(引用才有)
        /// </summary>
        public virtual Guid? QuoteMessageId { get; set; }

        /// <summary>
        /// 引用层级 0:不是引用
        /// </summary>
        public virtual long QuoteDepth { get; protected set; }
        /// <summary>
        /// 转发层级 0:不是转发
        /// </summary>
        public virtual long ForwardDepth { get; protected set; }

        /// <summary>
        /// 扩展（键名）根据业务自义，如:"courseId"、"course-userId"、"erp-userId"
        /// </summary>
        [StringLength(100)]
        public virtual string KeyName { get; set; }
        /// <summary>
        /// 扩展（键值）根据业务自义,如："123456789"、"02b7d668-02ca-428f-b88c-b8adac2c5044"、"admin"
        /// </summary>
        [StringLength(5000)]
        public virtual string KeyValue { get; set; }

        /// <summary>
        /// 撤回消息时间
        /// </summary>
        public virtual DateTime? RollbackTime { get; set; }


        [ForeignKey(nameof(SenderId))]
        public virtual ChatObject Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual ChatObject Receiver { get; set; }

        #region 转发
        /// <summary>
        /// 转发自...
        /// </summary>
        [ForeignKey(nameof(ForwardMessageId))]
        public virtual Message ForwardMessage { get; set; }

        /// <summary>
        /// 被转发列表
        /// </summary>
        [InverseProperty(nameof(ForwardMessage))]
        public virtual IList<Message> ForwardedMessageList { get; set; }
        #endregion

        #region 引用
        /// <summary>
        /// 引用自...
        /// </summary>
        [ForeignKey(nameof(QuoteMessageId))]
        public virtual Message QuoteMessage { get; set; }

        /// <summary>
        /// 被引用列表
        /// </summary>
        [InverseProperty(nameof(QuoteMessage))]
        public virtual IList<Message> QuotedMessageList { get; set; }
        #endregion

    }
}
