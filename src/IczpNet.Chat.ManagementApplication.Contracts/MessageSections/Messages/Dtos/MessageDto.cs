﻿using IczpNet.Chat.MessageSections.Messages;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.MessageSections.Messages.Dtos
{
    public class MessageDto : MessageInfo<dynamic>, IEntityDto<long>
    {
        /// <summary>
        /// 转发来源Id(转发才有)
        /// </summary>
        public virtual long? ForwardMessageId { get; set; }

        /// <summary>
        /// 转发层级 0:不是转发
        /// </summary>
        public virtual long ForwardDepth { get; set; }

        /// <summary>
        /// 引用来源Id(引用才有)
        /// </summary>
        public virtual long? QuoteMessageId { get; set; }

        /// <summary>
        /// 引用层级 0:不是引用
        /// </summary>
        public virtual long QuoteDepth { get; set; }
    }
}
