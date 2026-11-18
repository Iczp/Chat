using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// 新增 URL 安全规则参数
/// </summary>
public class UrlSecurityRuleCreateInput
{
    [Required]
    [MaxLength(256)]
    public string Host { get; set; }

    public UrlSecurityLevel Level { get; set; } = UrlSecurityLevel.Warning;

    public UrlRiskCategory Category { get; set; } = UrlRiskCategory.UnknownThirdParty;

    [MaxLength(512)]
    public string Reason { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime? ExpireTime { get; set; }
}
