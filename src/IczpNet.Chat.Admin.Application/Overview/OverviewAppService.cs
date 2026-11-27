using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.Overview;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.Overview;

/// <summary>
/// Chat 运营总览服务
/// </summary>
[Authorize(ChatAdminPermissions.Overview.Default)]
[ExposeServices(typeof(OverviewAppService), typeof(IOverviewAdminAppService))]
public class OverviewAppService(IOverviewAdminManager overviewAdminManager)
    : ApplicationService, IOverviewAdminAppService
{
    protected IOverviewAdminManager OverviewAdminManager { get; } = overviewAdminManager;

    public virtual async Task<ChatOverviewAdminSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await OverviewAdminManager.GetSummaryAsync(cancellationToken);
        return new ChatOverviewAdminSummaryDto
        {
            ChatObjectCount = result.ChatObjectCount,
            ActiveChatObjectCount = result.ActiveChatObjectCount,
            SessionCount = result.SessionCount,
            MessageCount = result.MessageCount,
            PendingSecurityReportCount = result.PendingSecurityReportCount,
            TotalSecurityReportCount = result.TotalSecurityReportCount,
            RunningAiRunCount = result.RunningAiRunCount,
            FailedAiRunCount = result.FailedAiRunCount,
            BlobCount = result.BlobCount,
            TemplateCount = result.TemplateCount,
            OnlineConnectionCount = result.OnlineConnectionCount,
            ObservedAt = result.ObservedAt
        };
    }
}
