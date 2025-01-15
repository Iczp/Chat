using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.RedEnvelopes.Dtos;

/// <summary>
/// 用户的领红包记录
/// </summary>
public class UserRedEnvelopeDto
{
    /// <summary>
    ///红包Id
    /// </summary>
    public Guid RedEnvelopeId { get; set; }
    /// <summary>
    /// 发红包的人
    /// </summary>
    public ChatObjectSimpleDto ProviderUser { get; set; }
    /// <summary>
    /// 红包发放方式（0：随机金额;1:固定金额）
    /// </summary>
    public GrantModes ProviderMode { get; set; }
    /// <summary>
    /// 发红包的金额
    /// </summary>
    public decimal ProviderAmount { get; set; }
    ///// <summary>
    ///// 领红包的人
    ///// </summary>
    //public ChatObjectSimpleDto OwnerUser { get; set; }
    /// <summary>
    /// 领红包的金额
    /// </summary>
    public decimal? OwnerAmount { get; set; }
    /// <summary>
    /// 领红包的时间
    /// </summary>
    public DateTime? OwnerTime { get; set; }
    /// <summary>
    /// 手气最佳
    /// </summary>
    public bool IsTop { get;  set; }
}
