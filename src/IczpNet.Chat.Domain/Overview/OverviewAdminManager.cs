using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.AiRuns;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.Security;
using IczpNet.Chat.Sessions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Overview;

public class OverviewAdminManager(
    IChatObjectRepository chatObjectRepository,
    ISessionRepository sessionRepository,
    IMessageRepository messageRepository,
    IRepository<SecurityReport, Guid> securityReportRepository,
    IRepository<AiRun, Guid> aiRunRepository,
    IRepository<Blob, Guid> blobRepository,
    IRepository<MessageTemplateDefinition, Guid> templateRepository,
    IOnlineManager onlineManager) : DomainService, IOverviewAdminManager, ITransientDependency
{
    public virtual async Task<ChatOverviewAdminSummaryResult> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var chatObjectQuery = await chatObjectRepository.GetQueryableAsync();
        var sessionQuery = await sessionRepository.GetQueryableAsync();
        var messageQuery = await messageRepository.GetQueryableAsync();
        var reportQuery = await securityReportRepository.GetQueryableAsync();
        var aiRunQuery = await aiRunRepository.GetQueryableAsync();
        var blobQuery = await blobRepository.GetQueryableAsync();
        var templateQuery = await templateRepository.GetQueryableAsync();

        var chatObjectCount = await AsyncExecuter.LongCountAsync(chatObjectQuery, cancellationToken);
        var activeChatObjectCount = await AsyncExecuter.LongCountAsync(chatObjectQuery.Where(x => x.IsEnabled), cancellationToken);
        var sessionCount = await AsyncExecuter.LongCountAsync(sessionQuery, cancellationToken);
        var messageCount = await AsyncExecuter.LongCountAsync(messageQuery, cancellationToken);
        var totalSecurityReportCount = await AsyncExecuter.LongCountAsync(reportQuery, cancellationToken);
        var pendingSecurityReportCount = await AsyncExecuter.LongCountAsync(reportQuery.Where(x => x.Status == 0), cancellationToken);
        var runningAiRunCount = await AsyncExecuter.LongCountAsync(aiRunQuery.Where(x => x.Status == AiRunStatus.Running), cancellationToken);
        var failedAiRunCount = await AsyncExecuter.LongCountAsync(aiRunQuery.Where(x => x.Status == AiRunStatus.Failed), cancellationToken);
        var blobCount = await AsyncExecuter.LongCountAsync(blobQuery, cancellationToken);
        var templateCount = await AsyncExecuter.LongCountAsync(templateQuery, cancellationToken);

        long onlineCount = 0;
        try
        {
            onlineCount = await onlineManager.GetTotalCountAsync();
        }
        catch
        {
            onlineCount = 0;
        }

        return new ChatOverviewAdminSummaryResult
        {
            ChatObjectCount = chatObjectCount,
            ActiveChatObjectCount = activeChatObjectCount,
            SessionCount = sessionCount,
            MessageCount = messageCount,
            PendingSecurityReportCount = pendingSecurityReportCount,
            TotalSecurityReportCount = totalSecurityReportCount,
            RunningAiRunCount = runningAiRunCount,
            FailedAiRunCount = failedAiRunCount,
            BlobCount = blobCount,
            TemplateCount = templateCount,
            OnlineConnectionCount = onlineCount,
            ObservedAt = Clock.Now
        };
    }
}
