using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DistributedLocking;
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
}
