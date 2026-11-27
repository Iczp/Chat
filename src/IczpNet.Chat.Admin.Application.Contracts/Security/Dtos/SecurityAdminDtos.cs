using System;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.Security.Dtos;

public class UrlSecurityRuleAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public UrlSecurityLevel? Level { get; set; }

    public UrlRiskCategory? Category { get; set; }

    public bool? IsEnabled { get; set; }
}

public class UrlSecurityRuleAdminDto : EntityDto<Guid>
{
    public string Host { get; set; }

    public UrlSecurityLevel Level { get; set; }

    public UrlRiskCategory Category { get; set; }

    public string Reason { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? ExpireTime { get; set; }

    public long HitCount { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class UrlSecurityRuleAdminCreateInput
{
    [Required]
    [MaxLength(256)]
    public string Host { get; set; }

    public UrlSecurityLevel Level { get; set; }

    public UrlRiskCategory Category { get; set; }

    [MaxLength(512)]
    public string Reason { get; set; }

    public bool IsEnabled { get; set; } = true;

    public DateTime? ExpireTime { get; set; }
}

public class UrlSecurityRuleAdminUpdateInput : UrlSecurityRuleAdminCreateInput
{
}

public class UrlSecurityRuleAdminSetEnabledInput
{
    public bool IsEnabled { get; set; }
}

public class UrlSecurityAdminCheckInput
{
    [Required]
    public string Url { get; set; }
}

public class UrlSecurityAdminCheckOutput
{
    public string Url { get; set; }

    public string Domain { get; set; }

    public UrlSecurityLevel Level { get; set; }

    public UrlRiskCategory Category { get; set; }

    public string Message { get; set; }

    public string InterceptReason { get; set; }

    public string Source { get; set; }
}

public class SecurityReportAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public int? Status { get; set; }

    public string TargetType { get; set; }
}

public class SecurityReportAdminDto : EntityDto<Guid>
{
    public long? ReporterChatObjectId { get; set; }

    public Guid? ReporterSessionUnitId { get; set; }

    public string TargetType { get; set; }

    public string TargetContent { get; set; }

    public long? MessageId { get; set; }

    public string Reason { get; set; }

    public string Description { get; set; }

    public int Status { get; set; }

    public string AuditRemark { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class SecurityReportAdminHandleInput
{
    public int Status { get; set; }

    [MaxLength(512)]
    public string AuditRemark { get; set; }

    public bool AddBlacklist { get; set; }
}
