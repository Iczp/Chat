using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// URL 安全风控规则 DTO
/// </summary>
public class UrlSecurityRuleDto : EntityDto<Guid>
{
    public string Host { get; set; }
    public UrlSecurityLevel Level { get; set; }
    public UrlRiskCategory Category { get; set; }
    public string Reason { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? ExpireTime { get; set; }
    public long HitCount { get; set; }
    public DateTime CreationTime { get; set; }
}
