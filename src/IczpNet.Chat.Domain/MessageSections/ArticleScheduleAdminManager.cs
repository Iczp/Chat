using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections;

public class ArticleScheduleAdminManager(
    IRepository<ScheduledArticleMessage, Guid> repository,
    ArticleScheduleManager articleScheduleManager,
    ISessionUnitManager sessionUnitManager,
    IAbpDistributedLock distributedLock,
    IUnitOfWorkManager unitOfWorkManager) : DomainService, IArticleScheduleAdminManager, ITransientDependency
{
    protected IRepository<ScheduledArticleMessage, Guid> Repository { get; } = repository;
    protected ArticleScheduleManager ArticleScheduleManager { get; } = articleScheduleManager;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IAbpDistributedLock DistributedLock { get; } = distributedLock;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    private static void EnsureOwnerAuthorized(long ownerId, ArticleScheduleAdminScope accessScope)
    {
        if (accessScope == null || accessScope.HasManageAll)
        {
            return;
        }

        if (accessScope.AllowedOwnerIds == null || !accessScope.AllowedOwnerIds.Contains(ownerId))
        {
            throw new AbpAuthorizationException("无权操作该发送主体的定时文章任务。");
        }
    }

    private static void EnsureTaskScope(ScheduledArticleMessage task, ArticleScheduleAdminScope accessScope)
    {
        if (accessScope == null)
        {
            return;
        }

        if (task.RequestedByTenantId != accessScope.TenantId)
        {
            throw new EntityNotFoundException(typeof(ScheduledArticleMessage), task.Id);
        }

        EnsureOwnerAuthorized(task.OwnerId, accessScope);
    }

    private async Task<long> ValidateSenderUnitAsync(Guid senderSessionUnitId, bool requireSendable, ArticleScheduleAdminScope accessScope)
    {
        ArticleScheduleManager.CheckEnabled();
        var unit = await SessionUnitManager.GetCacheAsync(senderSessionUnitId);
        if ((requireSendable && (!unit.IsEnabled || !unit.IsInputEnabled)) ||
            (unit.OwnerObjectType != ChatObjectTypeEnums.Official && unit.OwnerObjectType != ChatObjectTypeEnums.Subscription))
        {
            throw new UserFriendlyException("请选择已启用的公众号或服务号会话。");
        }

        EnsureOwnerAuthorized(unit.OwnerId, accessScope);
        return unit.OwnerId;
    }

    private DateTime ValidateScheduledTime(DateTimeOffset value)
    {
        var time = value.UtcDateTime;
        if (time <= Clock.Now.ToUniversalTime())
        {
            throw new UserFriendlyException("计划时间必须晚于当前时间。");
        }
        return time;
    }

    public virtual async Task<ArticleScheduleAdminListResult> GetAdminListAsync(
        Guid? senderSessionUnitId,
        long? ownerId,
        ArticleScheduleStatus? status,
        int skipCount,
        int maxResultCount,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .WhereIf(senderSessionUnitId.HasValue, x => x.SenderSessionUnitId == senderSessionUnitId.Value)
            .WhereIf(ownerId.HasValue, x => x.OwnerId == ownerId.Value)
            .WhereIf(status.HasValue, x => x.Status == status.Value);

        if (accessScope != null)
        {
            query = query.Where(x => x.RequestedByTenantId == accessScope.TenantId);
            if (!accessScope.HasManageAll)
            {
                var allowedIds = accessScope.AllowedOwnerIds?.ToList() ?? [];
                query = query.Where(x => allowedIds.Contains(x.OwnerId));
            }
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new ArticleScheduleAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .ThenByDescending(x => x.Id)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 100)),
            cancellationToken);

        return new ArticleScheduleAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<ScheduledArticleMessage> CreateAdminAsync(
        Guid id,
        Guid senderSessionUnitId,
        DateTimeOffset scheduledAt,
        MessageInput<ArticleContentInput> message,
        Guid requestedBy,
        Guid? tenantId,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await ValidateSenderUnitAsync(senderSessionUnitId, requireSendable: true, accessScope);
        if (id == Guid.Empty)
        {
            throw new UserFriendlyException("请提供稳定的任务标识。");
        }

        ArticleContent.Create(message?.Content);
        var json = JsonSerializer.Serialize(message);
        if (json.Length > 32768)
        {
            throw new UserFriendlyException("内容过长，请减少消息附加参数。");
        }

        await using var handle = await DistributedLock.TryAcquireAsync(
            $"Chat:ArticleSchedule:{id:N}",
            TimeSpan.FromSeconds(10),
            cancellationToken);
        if (handle == null)
        {
            throw new UserFriendlyException("任务正在处理，请稍后重试。");
        }

        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var existing = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (existing != null)
        {
            EnsureTaskScope(existing, accessScope);
            if (existing.RequestedBy != requestedBy ||
                existing.SenderSessionUnitId != senderSessionUnitId ||
                existing.InputJson != json ||
                existing.ScheduledAt != scheduledAt.UtcDateTime)
            {
                throw new UserFriendlyException("任务标识已被使用，请刷新草稿后重试。");
            }
            return existing;
        }

        var task = new ScheduledArticleMessage(
            id,
            senderSessionUnitId,
            ownerId,
            requestedBy,
            json,
            ValidateScheduledTime(scheduledAt),
            tenantId);

        await Repository.InsertAsync(task, autoSave: true, cancellationToken: cancellationToken);
        await uow.CompleteAsync(cancellationToken);
        await ArticleScheduleManager.EnqueueAsync(task);
        return task;
    }

    public virtual Task<ScheduledArticleMessage> RescheduleAdminAsync(
        Guid id,
        DateTimeOffset scheduledAt,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        return ChangeAsync(id, ValidateScheduledTime(scheduledAt), accessScope, cancellationToken);
    }

    public virtual Task<ScheduledArticleMessage> CancelAdminAsync(
        Guid id,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        return ChangeAsync(id, null, accessScope, cancellationToken);
    }

    public virtual Task<ScheduledArticleMessage> ExecuteNowAdminAsync(
        Guid id,
        ArticleScheduleAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        return ChangeAsync(id, Clock.Now, accessScope, cancellationToken);
    }

    private async Task<ScheduledArticleMessage> ChangeAsync(
        Guid id,
        DateTime? time,
        ArticleScheduleAdminScope accessScope,
        CancellationToken cancellationToken)
    {
        ArticleScheduleManager.CheckEnabled();
        await using var handle = await DistributedLock.TryAcquireAsync(
            $"Chat:ArticleSchedule:{id:N}",
            TimeSpan.FromSeconds(10),
            cancellationToken);
        if (handle == null)
        {
            throw new UserFriendlyException("任务正在执行，请稍后刷新状态。");
        }

        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var task = await Repository.GetAsync(id, cancellationToken: cancellationToken);
        EnsureTaskScope(task, accessScope);
        await ValidateSenderUnitAsync(task.SenderSessionUnitId, requireSendable: time.HasValue, accessScope);
        if (time.HasValue)
        {
            task.Reschedule(time.Value);
        }
        else
        {
            task.Cancel();
        }

        await Repository.UpdateAsync(task, autoSave: true, cancellationToken: cancellationToken);
        await uow.CompleteAsync(cancellationToken);
        if (time.HasValue)
        {
            await ArticleScheduleManager.EnqueueAsync(task);
        }
        return task;
    }
}
