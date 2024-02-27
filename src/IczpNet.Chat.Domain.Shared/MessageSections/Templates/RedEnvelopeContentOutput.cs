using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Templates;

public class RedEnvelopeContentOutput : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// 文本内容
    /// </summary>
    public virtual string Text { get; set; }

    /// <summary>
    /// 创建人(发红包的人)
    /// </summary>
    public virtual long? OwnerId { get; protected set; }

    /// <summary>
    /// 红包发放方式（0：随机金额;1:固定金额）
    /// </summary>
    //[Index]
    public virtual GrantModes GrantMode { get; set; }

    /// <summary>
    /// 金额
    /// </summary>

    //[Precision(18, 2)]
    public virtual decimal Amount { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public virtual int Count { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public virtual decimal TotalAmount { get; set; }

    /// <summary>
    /// 到期时间
    /// </summary>
    public virtual DateTime? ExpireTime { get; protected set; }

    ///// <summary>
    ///// 登录人的领取详细
    ///// </summary>
    //public virtual RedEnvelopeDetailResult Detail { get; set; }

}
