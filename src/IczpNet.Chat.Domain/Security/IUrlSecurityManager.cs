using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Security;

public class UrlSecurityRuleAdminListResult
{
    public long TotalCount { get; set; }

    public List<UrlSecurityRule> Items { get; set; } = new();
}

public class SecurityReportAdminListResult
{
    public long TotalCount { get; set; }

    public List<SecurityReport> Items { get; set; } = new();
}

/// <summary>
/// URL 安全风控与违规投诉领域管理器
/// </summary>
public interface IUrlSecurityManager : IDomainService
{
    /// <summary>
    /// 评估 URL 安全等级（结合 Redis 缓存与黑白名单匹配）
    /// </summary>
    Task<UrlSecurityEvaluationResult> CheckUrlAsync(string url);

    /// <summary>
    /// 提交违规举报并落库
    /// </summary>
    Task<SecurityReport> SubmitReportAsync(
        string target,
        string targetType,
        string reason,
        string description = null,
        long? messageId = null,
        long? reporterChatObjectId = null,
        Guid? reporterSessionUnitId = null);

    /// <summary>
    /// 将数据库中的黑白名单同步刷新至 Redis 缓存
    /// </summary>
    Task RefreshCacheAsync();
}

public interface IUrlSecurityAdminManager : IUrlSecurityManager
{
    Task<UrlSecurityRuleAdminListResult> GetRulesAdminListAsync(
        string keyword = null,
        UrlSecurityLevel? level = null,
        UrlRiskCategory? category = null,
        bool? isEnabled = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRule> GetRuleAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRule> CreateRuleAdminAsync(
        string host,
        UrlSecurityLevel level,
        UrlRiskCategory category,
        string reason,
        bool isEnabled,
        DateTime? expireTime,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRule> UpdateRuleAdminAsync(
        Guid id,
        string host,
        UrlSecurityLevel level,
        UrlRiskCategory category,
        string reason,
        bool isEnabled,
        DateTime? expireTime,
        CancellationToken cancellationToken = default);

    Task<UrlSecurityRule> SetRuleEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task DeleteRuleAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SecurityReportAdminListResult> GetReportsAdminListAsync(
        string keyword = null,
        int? status = null,
        string targetType = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<SecurityReport> GetReportAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SecurityReport> HandleReportAdminAsync(
        Guid id,
        int status,
        string auditRemark,
        bool addBlacklist,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// URL 安全风控评估结果
/// </summary>
public class UrlSecurityEvaluationResult
{
    public string Url { get; set; }
    public string Domain { get; set; }
    public UrlSecurityLevel Level { get; set; }
    public UrlRiskCategory Category { get; set; }
    public string Message { get; set; }
    public string InterceptReason { get; set; }
    public string Source { get; set; }
}
