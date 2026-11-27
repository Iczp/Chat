using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.AiRuns;

public class AiRunAdminListResult
{
    public long TotalCount { get; set; }

    public List<AiRun> Items { get; set; } = new();
}

public interface IAiRunManager
{
    /// <summary>
    /// 幂等创建或获取 AI 运行任务
    /// </summary>
    Task<AiRun> CreateAsync(long sourceMessageId, Guid sessionId, Guid requesterSessionUnitId, string provider, int maxAttempts = 3);

    /// <summary>
    /// 领取下一批待运行的任务（严格保证同 Session 串行、跨 Session 并行）
    /// </summary>
    Task<List<AiRun>> ClaimNextRunsAsync(string workerId, int count, DateTime now, TimeSpan leaseDuration, TimeSpan executionTimeout);

    /// <summary>
    /// 回收超期租约的任务
    /// </summary>
    Task<int> ReclaimExpiredLeasesAsync(DateTime now);

    /// <summary>
    /// 心跳续约
    /// </summary>
    Task HeartbeatAsync(Guid runId, string workerId, DateTime now, TimeSpan leaseDuration);

    /// <summary>
    /// 标记任务完成
    /// </summary>
    Task CompleteAsync(Guid runId, string workerId, long outputMessageId, DateTime now);

    /// <summary>
    /// 标记重试或最终失败
    /// </summary>
    Task RetryOrFailAsync(Guid runId, string workerId, string errorCode, string errorMessage, DateTime now, TimeSpan retryDelay);

    /// <summary>
    /// 人工重新排队执行任务
    /// </summary>
    Task<AiRun> RetryAsync(Guid runId, DateTime now);

    /// <summary>
    /// 取消任务
    /// </summary>
    Task<AiRun> CancelAsync(Guid runId, DateTime now, string reason = null);

    /// <summary>
    /// 根据源消息 ID 获取任务
    /// </summary>
    Task<AiRun> FindBySourceMessageIdAsync(long sourceMessageId);

    /// <summary>
    /// 获取指定任务
    /// </summary>
    Task<AiRun> GetAsync(Guid runId);

    Task<AiRunAdminListResult> GetAdminListAsync(
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
        CancellationToken cancellationToken = default);

    Task<AiRun> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task DeleteManyAdminAsync(
        List<Guid> idList,
        CancellationToken cancellationToken = default);
}
