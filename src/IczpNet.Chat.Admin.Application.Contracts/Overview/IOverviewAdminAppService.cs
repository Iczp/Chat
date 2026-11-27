using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.Overview;

public class ChatOverviewAdminSummaryDto
{
    public long ChatObjectCount { get; set; }

    public long ActiveChatObjectCount { get; set; }

    public long SessionCount { get; set; }

    public long MessageCount { get; set; }

    public long PendingSecurityReportCount { get; set; }

    public long TotalSecurityReportCount { get; set; }

    public long RunningAiRunCount { get; set; }

    public long FailedAiRunCount { get; set; }

    public long BlobCount { get; set; }

    public long TemplateCount { get; set; }

    public long OnlineConnectionCount { get; set; }

    public DateTime ObservedAt { get; set; }
}

public interface IOverviewAdminAppService : IApplicationService
{
    Task<ChatOverviewAdminSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
