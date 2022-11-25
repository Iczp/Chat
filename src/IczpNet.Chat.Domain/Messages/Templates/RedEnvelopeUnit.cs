using IczpNet.Chat.BaseEntitys;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Messages.Templates
{
    public class RedEnvelopeUnit : BaseEntity<Guid>
    {
        public virtual Guid RedEnvelopeContentId { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, (double)decimal.MaxValue)]
        public virtual decimal Amount { get; set; }

        /// <summary>
        /// 最佳
        /// </summary>
        public virtual bool IsTop { get; set; }

        /// <summary>
        /// 是否有归属
        /// </summary>
        public virtual bool IsOwned { get; set; }

        /// <summary>
        /// 归属人
        /// </summary>
        //[StringLength(36)]
        public virtual Guid? OwnerUserId { get; set; }

        /// <summary>
        /// 得到时间
        /// </summary>
        public virtual DateTime? OwnerTime { get; set; }

        /// <summary>
        /// 退回时间
        /// </summary>
        public virtual DateTime? RollbackTime { get; set; }

        /// <summary>
        /// RedEnvelopeContent
        /// </summary>
        [ForeignKey(nameof(RedEnvelopeContentId))]
        public virtual RedEnvelopeContent RedEnvelopeContent { get; set; }

        /// <summary>
        /// 领取红包
        /// </summary>
        /// <param name="ownerUserId">领取人UserId</param>
        public void SetGrabed(Guid ownerUserId)
        {
            OwnerUserId = ownerUserId;
            //OwnerTime = Clock.Now;
            IsOwned = true;
        }
    }
}
