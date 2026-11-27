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

public class ArticleScheduleAdminScope
{
    public Guid? TenantId { get; set; }

    public bool HasManageAll { get; set; } = true;

    public IReadOnlyCollection<long> AllowedOwnerIds { get; set; }
}

public interface IArticleScheduleAdminManager
{
    Task<ArticleScheduleAdminListResult> GetAdminListAsync(
        Guid? senderSessionUnitId,
        long? ownerId,
        ArticleScheduleStatus? status,
        int skipCount,
        int maxResultCount,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> CreateAdminAsync(
        Guid id,
        Guid senderSessionUnitId,
        DateTimeOffset scheduledAt,
        MessageInput<ArticleContentInput> message,
        Guid requestedBy,
        Guid? tenantId,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> RescheduleAdminAsync(
        Guid id,
        DateTimeOffset scheduledAt,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> CancelAdminAsync(
        Guid id,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default);

    Task<ScheduledArticleMessage> ExecuteNowAdminAsync(
        Guid id,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default);
}
