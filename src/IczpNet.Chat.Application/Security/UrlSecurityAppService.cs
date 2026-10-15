using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Security.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace IczpNet.Chat.Security;

/// <summary>
/// 链接安全与违规投诉应用服务
/// </summary>
public class UrlSecurityAppService : ChatAppService, IUrlSecurityAppService
{
    protected IUrlSecurityManager UrlSecurityManager { get; }

    public UrlSecurityAppService(IUrlSecurityManager urlSecurityManager)
    {
        UrlSecurityManager = urlSecurityManager;
    }

    /// <summary>
    /// 检测 URL 安全状态 (支持匿名与登录用户，结合 Redis 缓存)
    /// </summary>
    [AllowAnonymous]
    public virtual async Task<UrlCheckOutput> CheckUrlAsync(UrlCheckInput input)
    {
        var eval = await UrlSecurityManager.CheckUrlAsync(input.Url);
        return new UrlCheckOutput
        {
            Url = eval.Url,
            Domain = eval.Domain,
            Level = eval.Level,
            Category = eval.Category,
            Message = eval.Message,
            InterceptReason = eval.InterceptReason,
            Source = eval.Source
        };
    }

    /// <summary>
    /// 提交违规 URL 或消息举报
    /// </summary>
    public virtual async Task<SecurityReportDto> ReportAsync(SecurityReportCreateInput input)
    {
        var report = await UrlSecurityManager.SubmitReportAsync(
            target: input.Target,
            targetType: input.TargetType ?? "url",
            reason: input.Reason,
            description: input.Description,
            messageId: input.MessageId,
            reporterChatObjectId: input.ReporterChatObjectId,
            reporterSessionUnitId: input.ReporterSessionUnitId
        );

        return new SecurityReportDto
        {
            Id = report.Id,
            TargetType = report.TargetType,
            TargetContent = report.TargetContent,
            Reason = report.Reason,
            Description = report.Description,
            MessageId = report.MessageId,
            ReporterChatObjectId = report.ReporterChatObjectId,
            ReporterSessionUnitId = report.ReporterSessionUnitId,
            Status = report.Status,
            CreationTime = report.CreationTime
        };
    }

    /// <summary>
    /// 管理员指令：刷新风控规则至 Redis 缓存
    /// </summary>
    public virtual async Task RefreshCacheAsync()
    {
        await UrlSecurityManager.RefreshCacheAsync();
    }
}
