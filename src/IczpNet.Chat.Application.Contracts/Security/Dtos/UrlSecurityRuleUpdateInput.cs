using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// 更新 URL 安全规则参数
/// </summary>
public class UrlSecurityRuleUpdateInput
{
    [Required]
    [MaxLength(256)]
    public string Host { get; set; }

    public UrlSecurityLevel Level { get; set; }

    public UrlRiskCategory Category { get; set; }

    [MaxLength(512)]
    public string Reason { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? ExpireTime { get; set; }
}
