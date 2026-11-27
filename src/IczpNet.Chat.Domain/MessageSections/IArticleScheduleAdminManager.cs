using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Templates;

namespace IczpNet.Chat.MessageSections;

public class ArticleScheduleAdminListResult
{
    public long TotalCount { get; set; }

    public List<ScheduledArticleMessage> Items { get; set; } = new();
}

public interface IArticleScheduleAdminManager
{
    Task<ArticleScheduleAdminListResult> GetAdminListAsync(
        Guid? senderSessionUnitId,
        long? ownerId,
        ArticleScheduleStatus? status,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> CreateAdminAsync(
        Guid id,
        Guid senderSessionUnitId,
        DateTimeOffset scheduledAt,
        MessageInput<ArticleContentInput> message,
        Guid requestedBy,
        Guid? tenantId,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> RescheduleAdminAsync(
        Guid id,
        DateTimeOffset scheduledAt,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> CancelAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> ExecuteNowAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
