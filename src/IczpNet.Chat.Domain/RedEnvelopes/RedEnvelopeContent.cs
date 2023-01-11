using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;

namespace IczpNet.Chat.RedEnvelopes
{
    [MessageTemplate(MessageTypes.RedEnvelope)]
    [ContentOuput(typeof(RedEnvelopeContentOutput))]
    public class RedEnvelopeContent : MessageContentEntityBase, IChatOwner<Guid?>
    {
        /// <summary>
        /// 创建人(发红包的人)
        /// </summary>
        public override Guid? OwnerId { get; protected set; }

        /// <summary>
        /// 创建人(发红包的人)
        /// </summary>
        public override ChatObject Owner { get; protected set; }

        /// <summary>
        /// 红包发放方式（0：随机金额;1:固定金额）
        /// </summary>
        //[Index]
        public virtual GrantModes GrantMode { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        //[Precision(18, 2)]
        public virtual decimal Amount { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public virtual int Count { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public virtual decimal TotalAmount { get; set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        [StringLength(256)]
        //[Index]
        public virtual string Text { get; set; }

        /// <summary>
        /// 到期时间
        /// </summary>
        public virtual DateTime? ExpireTime { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<RedEnvelopeUnit> RedEnvelopeUnitList { get; protected set; } = new List<RedEnvelopeUnit>();

        public RedEnvelopeContent(Guid id, GrantModes grantMode, decimal amount, int count, decimal totalAmount, string text) : base(id)
        {
            GrantMode = grantMode;
            Amount = amount;
            Count = count;
            TotalAmount = totalAmount;
            Text = text;
        }

        public virtual void SetRedEnvelopeUnitList(IList<RedEnvelopeUnit> redEnvelopeUnits)
        {
            RedEnvelopeUnitList = redEnvelopeUnits;
        }
    }
}
