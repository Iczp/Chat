using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Security;

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
