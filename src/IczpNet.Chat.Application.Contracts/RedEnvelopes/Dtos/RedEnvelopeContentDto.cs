using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RedEnvelopes.Dtos;

/// <summary>
/// 
/// </summary>
//[AutoMapFrom(typeof(RedEnvelopeContent))]
public class RedEnvelopeContentDto : EntityDto<Guid>
{
    /// <summary>
    /// 红包发放方式（0：随机金额;1:固定金额）
    /// </summary>
    public GrantModes Mode { get; set; }
    /// <summary>
    /// 金额
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// 数量
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// 文本内容
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatorUserId { get; set; }
    /// <summary>
    /// 到期时间
    /// </summary>
    public DateTime ExpireTime { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public virtual IList<RedEnvelopeDetailResult> RedEnvelopeDetailList { get; set; }
}
