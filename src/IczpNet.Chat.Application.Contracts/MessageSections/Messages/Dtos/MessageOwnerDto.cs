using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos
{
    public class MessageOwnerDto : MessageDto, IEntityDto<long>
    {
        //public virtual string SenderName { get; set; }

        /// <summary>
        /// Sender SessionUnitId
        /// </summary>
        public virtual Guid SessionUnitId { get; set; }

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

        public virtual bool? IsOpened { get; set; }

        public virtual bool? IsReaded { get; set; }

        public virtual bool? IsFollowing { get; set; }

        public virtual long SessionUnitCount { get; set; }

        public virtual long ReadedCount { get; set; }

        public virtual long OpenedCount { get; set; }

        public virtual long FavoritedCount { get; set; }
    }
}
