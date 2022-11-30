using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Messages
{
    public partial class Message
    {
        /// <summary>
        /// 转发来源Id(转发才有)
        /// </summary>
        public virtual Guid? ForwardMessageId { get; protected set; }

        /// <summary>
        /// 转发层级 0:不是转发
        /// </summary>
        public virtual long ForwardDepth { get; protected set; }

        [StringLength(100)]
        public virtual string ForwardPath { get; protected set; }

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
    }
}
