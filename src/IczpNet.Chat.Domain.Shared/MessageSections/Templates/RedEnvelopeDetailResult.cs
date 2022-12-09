using System;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 
    /// </summary>
    public class RedEnvelopeDetailResult 
    {
        public Guid Id { get; set; }

        /// <summary>
        /// 最佳
        /// </summary>
        public bool IsTop { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// 是否有归属
        /// </summary>
        public bool IsOwned { get; set; }

        /// <summary>
        /// 归属人
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// 得到时间
        /// </summary>
        public DateTime? OwnerTime { get; set; }

        ///// <summary>
        ///// 退回时间
        ///// </summary>
        //public DateTime? RollbackTime { get; set; }
    }
}
