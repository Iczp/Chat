using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.AiRuns;

public class AiRunManager(
    IRepository<AiRun, Guid> aiRunRepository,
    IAbpDistributedLock distributedLock) : DomainService, IAiRunManager
{
    public IRepository<AiRun, Guid> AiRunRepository { get; } = aiRunRepository;
    public IAbpDistributedLock DistributedLock { get; } = distributedLock;

    [UnitOfWork]
    public async Task<AiRun> CreateAsync(
        long sourceMessageId,
        Guid sessionId,
        Guid requesterSessionUnitId,
        string provider,
        int maxAttempts = 3)
    {
        var existing = await (await AiRunRepository.GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.SourceMessageId == sourceMessageId);

        if (existing != null)
        {
            return existing;
        }

        var run = new AiRun(GuidGenerator.Create(), sourceMessageId, sessionId, requesterSessionUnitId, provider, Clock.Now);
        return await AiRunRepository.InsertAsync(run, autoSave: true);
    }

    [UnitOfWork]
    public async Task<List<AiRun>> ClaimNextRunsAsync(
        string workerId,
        int count,
        DateTime now,
        TimeSpan leaseDuration,
        TimeSpan executionTimeout)
    {
        if (count <= 0) return [];

        const string lockKey = "Chat:AiRun:ClaimLock";
        await using var handle = await DistributedLock.TryAcquireAsync(lockKey, timeout: TimeSpan.FromSeconds(5));
        if (handle == null)
        {
            Logger.LogWarning("AiRunManager could not acquire claim lock within timeout.");
            return [];
        }

        var queryable = await AiRunRepository.GetQueryableAsync();

        // 1. 获取所有当前正在运行且租约未过期的 SessionId 集合 (这些 Session 目前不能再领取新任务，保证同一 Session 串行)
        var activeSessionIds = await queryable
            .Where(x => x.Status == AiRunStatus.Running && x.LeaseUntilTime != null && x.LeaseUntilTime > now)
            .Select(x => x.SessionId)
            .Distinct()
            .ToListAsync();

        var activeSessionSet = new HashSet<Guid>(activeSessionIds);

        // 2. 候选任务：状态为 Queued 或 RetryScheduled，且 NextAttemptAt <= now
        // 按 CreationTime 升序排序 (严格保证会话内 FIFO)
        var candidates = await queryable
            .Where(x => (x.Status == AiRunStatus.Queued || x.Status == AiRunStatus.RetryScheduled) && x.NextAttemptAt <= now)
            .OrderBy(x => x.CreationTime)
            .Take(count * 5)
            .ToListAsync();

        var claimedRuns = new List<AiRun>();
        var newlyClaimedSessions = new HashSet<Guid>();

        foreach (var candidate in candidates)
        {
            if (claimedRuns.Count >= count) break;

            // 排除同一 Session 已有运行中任务的候选
            if (activeSessionSet.Contains(candidate.SessionId))
            {
                continue;
            }

            // 候选集合中同一批次只允许每个 Session 领取最早的一条
            if (newlyClaimedSessions.Contains(candidate.SessionId))
            {
                continue;
            }

            candidate.Claim(workerId, now, now.Add(leaseDuration), now.Add(executionTimeout));
            claimedRuns.Add(candidate);
            newlyClaimedSessions.Add(candidate.SessionId);
        }

        if (claimedRuns.Count > 0)
        {
            await AiRunRepository.UpdateManyAsync(claimedRuns, autoSave: true);
            Logger.LogInformation("Worker {WorkerId} claimed {Count} AI runs: {RunIds}", workerId, claimedRuns.Count, string.Join(", ", claimedRuns.Select(x => x.Id)));
        }

        return claimedRuns;
    }

    [UnitOfWork]
    public async Task<int> ReclaimExpiredLeasesAsync(DateTime now)
    {
        var queryable = await AiRunRepository.GetQueryableAsync();
        var expiredRuns = await queryable
            .Where(x => x.Status == AiRunStatus.Running && x.LeaseUntilTime != null && x.LeaseUntilTime < now)
            .Take(50)
            .ToListAsync();

        if (expiredRuns.Count == 0) return 0;

        foreach (var run in expiredRuns)
        {
            run.RequeueExpiredLease(now);
        }

        await AiRunRepository.UpdateManyAsync(expiredRuns, autoSave: true);
        Logger.LogWarning("Reclaimed {Count} expired AI runs.", expiredRuns.Count);
        return expiredRuns.Count;
    }

    [UnitOfWork]
    public async Task HeartbeatAsync(Guid runId, string workerId, DateTime now, TimeSpan leaseDuration)
    {
        var run = await AiRunRepository.FindAsync(runId);
        if (run != null && run.Status == AiRunStatus.Running && run.LeaseOwner == workerId)
        {
            run.Heartbeat(workerId, now, now.Add(leaseDuration));
            await AiRunRepository.UpdateAsync(run, autoSave: true);
        }
    }

    [UnitOfWork]
    public async Task CompleteAsync(Guid runId, string workerId, long outputMessageId, DateTime now)
    {
        var run = await AiRunRepository.FindAsync(runId);
        if (run == null) return;

        try
        {
            run.Complete(workerId, outputMessageId, now);
            await AiRunRepository.UpdateAsync(run, autoSave: true);
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "AiRun {RunId} completion skipped: worker lease mismatch or run no longer active.", runId);
        }
    }

    [UnitOfWork]
    public async Task RetryOrFailAsync(
        Guid runId,
        string workerId,
        string errorCode,
        string errorMessage,
        DateTime now,
        TimeSpan retryDelay)
    {
        var run = await AiRunRepository.FindAsync(runId);
        if (run == null) return;

        try
        {
            run.RetryOrFail(workerId, errorCode, errorMessage, now, retryDelay);
            await AiRunRepository.UpdateAsync(run, autoSave: true);
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "AiRun {RunId} failure update skipped: worker lease mismatch or run no longer active.", runId);
        }
    }

    [UnitOfWork]
    public async Task<AiRun> RetryAsync(Guid runId, DateTime now)
    {
        var run = await AiRunRepository.GetAsync(runId);
        run.ManualRetry(now);
        return await AiRunRepository.UpdateAsync(run, autoSave: true);
    }

    [UnitOfWork]
    public async Task<AiRun> CancelAsync(Guid runId, DateTime now, string reason = null)
    {
        var run = await AiRunRepository.GetAsync(runId);
        run.Cancel(now, reason);
        return await AiRunRepository.UpdateAsync(run, autoSave: true);
    }

    public async Task<AiRun> FindBySourceMessageIdAsync(long sourceMessageId)
    {
        return await (await AiRunRepository.GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.SourceMessageId == sourceMessageId);
    }

    public async Task<AiRun> GetAsync(Guid runId)
    {
        return await AiRunRepository.GetAsync(runId);
    }

    public virtual async Task<AiRunAdminListResult> GetAdminListAsync(
        string keyword = null,
        AiRunStatus? status = null,
        string provider = null,
        Guid? sessionId = null,
        Guid? requesterSessionUnitId = null,
        long? sourceMessageId = null,
        long? outputMessageId = null,
        string leaseOwner = null,
        DateTime? startCreationTime = null,
        DateTime? endCreationTime = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await AiRunRepository.GetQueryableAsync())
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                x.Provider.Contains(keyword.Trim()) ||
                (x.LastErrorCode != null && x.LastErrorCode.Contains(keyword.Trim())) ||
                (x.LastErrorMessage != null && x.LastErrorMessage.Contains(keyword.Trim())) ||
                (x.LeaseOwner != null && x.LeaseOwner.Contains(keyword.Trim())))
            .WhereIf(status.HasValue, x => x.Status == status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(provider), x => x.Provider == provider.Trim())
            .WhereIf(sessionId.HasValue, x => x.SessionId == sessionId.Value)
            .WhereIf(requesterSessionUnitId.HasValue, x => x.RequesterSessionUnitId == requesterSessionUnitId.Value)
            .WhereIf(sourceMessageId.HasValue, x => x.SourceMessageId == sourceMessageId.Value)
            .WhereIf(outputMessageId.HasValue, x => x.OutputMessageId == outputMessageId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(leaseOwner), x => x.LeaseOwner == leaseOwner.Trim())
            .WhereIf(startCreationTime.HasValue, x => x.CreationTime >= startCreationTime.Value)
            .WhereIf(endCreationTime.HasValue, x => x.CreationTime <= endCreationTime.Value);

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new AiRunAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new AiRunAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<AiRun> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await AiRunRepository.FindAsync(id, cancellationToken: cancellationToken);
        return entity ?? throw new EntityNotFoundException(typeof(AiRun), id);
    }

    public virtual async Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await AiRunRepository.FindAsync(id, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException(typeof(AiRun), id);

        if (entity.Status == AiRunStatus.Running)
        {
            throw new BusinessException("ChatAdmin:CannotDeleteRunningAiRun", "执行中的 AI 任务不允许删除，请先取消。");
        }

        await AiRunRepository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task DeleteManyAdminAsync(
        List<Guid> idList,
        CancellationToken cancellationToken = default)
    {
        if (idList == null || idList.Count == 0)
        {
            return;
        }

        var distinctIds = idList.Distinct().ToList();
        if (distinctIds.Count > 100)
        {
            throw new BusinessException("ChatAdmin:AiRunBatchLimitExceeded", "单次批量删除不能超过 100 条记录。");
        }

        var runs = await AiRunRepository.GetListAsync(x => distinctIds.Contains(x.Id), cancellationToken: cancellationToken);

        if (runs.Any(x => x.Status == AiRunStatus.Running))
        {
            throw new BusinessException("ChatAdmin:CannotDeleteRunningAiRun", "执行中的 AI 任务不允许删除，请先取消。");
        }

        if (runs.Count > 0)
        {
            await AiRunRepository.DeleteManyAsync(runs, autoSave: true, cancellationToken: cancellationToken);
        }
    }

    private static IQueryable<AiRun> ApplySorting(IQueryable<AiRun> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "status" => desc ? queryable.OrderByDescending(x => x.Status) : queryable.OrderBy(x => x.Status),
            "provider" => desc ? queryable.OrderByDescending(x => x.Provider) : queryable.OrderBy(x => x.Provider),
            "attemptcount" => desc ? queryable.OrderByDescending(x => x.AttemptCount) : queryable.OrderBy(x => x.AttemptCount),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }
}
