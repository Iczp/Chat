using System;

namespace IczpNet.Chat.MessageSections.Templates;

public class RedEnvelopeDetailResult
{
    public virtual Guid Id { get; set; }

    /// <summary>
    /// 最佳
    /// </summary>
    public virtual bool IsTop { get; set; }

    /// <summary>
    /// 金额
    /// </summary>
    public virtual double Amount { get; set; }

    /// <summary>
    /// 是否有归属
    /// </summary>
    public virtual bool IsOwned { get; set; }

    /// <summary>
    /// 归属人
    /// </summary>
    public virtual Guid? OwnerUserId { get; set; }

    /// <summary>
    /// 得到时间
    /// </summary>
    public virtual DateTime? OwnerTime { get; set; }

    ///// <summary>
    ///// 退回时间
    ///// </summary>
    //public virtual DateTime? RollbackTime { get; set; }
}
