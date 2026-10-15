using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security;

/// <summary>
/// URL 域名安全风控规则表
/// </summary>
[Comment("URL 域名安全风控规则表")]
[Index(nameof(Host))]
[Index(nameof(Level))]
[Index(nameof(IsEnabled))]
public class UrlSecurityRule : BaseEntity<Guid>
{
    /// <summary>
    /// 域名（支持精确匹配如 gotoim.com 或通配符如 *.scam.xyz）
    /// </summary>
    [Comment("域名模式")]
    [Required]
    [MaxLength(256)]
    public virtual string Host { get; set; }

    /// <summary>
    /// 安全级别：0=Safe(白名单), 1=Warning(灰名单提示), 2=Blocked(黑名单阻断)
    /// </summary>
    [Comment("安全级别")]
    public virtual UrlSecurityLevel Level { get; set; }

    /// <summary>
    /// 风险分类
    /// </summary>
    [Comment("风险分类")]
    public virtual UrlRiskCategory Category { get; set; }

    /// <summary>
    /// 拦截或放行原因说明
    /// </summary>
    [Comment("拦截或放行原因说明")]
    [MaxLength(512)]
    public virtual string Reason { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Comment("是否启用")]
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 规则有效期（null 为永久有效）
    /// </summary>
    [Comment("规则有效期至")]
    public virtual DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 累计命中计数
    /// </summary>
    [Comment("命中计数")]
    public virtual long HitCount { get; set; }

    protected UrlSecurityRule() { }

    public UrlSecurityRule(
        Guid id,
        string host,
        UrlSecurityLevel level,
        UrlRiskCategory category,
        string reason = null)
        : base(id)
    {
        Host = host?.Trim().ToLower();
        Level = level;
        Category = category;
        Reason = reason;
        IsEnabled = true;
    }
}
