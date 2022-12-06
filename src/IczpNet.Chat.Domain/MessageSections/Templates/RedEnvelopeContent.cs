using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class RedEnvelopeContent : BaseMessageContentEntity
    {
        /// <summary>
        /// 红包发放方式（0：随机金额;1:固定金额）
        /// </summary>
        //[Index]
        public virtual GrantModes GrantMode { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [Column(TypeName = "decimal(18, 8)")]
        //[Precision(18, 2)]
        public virtual decimal Amount { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public virtual int Count { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        [Column(TypeName = "decimal(18, 8)")]
        public virtual decimal TotalAmount { get; set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        [StringLength(256)]
        //[Index]
        public virtual string Text { get; set; }

        /// <summary>
        /// 创建人(发红包的人)
        /// </summary>
        //[StringLength(36)]
        public virtual Guid CreatorUserId { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public virtual DateTime ExpireTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<RedEnvelopeUnit> RedEnvelopeUnitList { get; set; } = new List<RedEnvelopeUnit>();
    }
}
