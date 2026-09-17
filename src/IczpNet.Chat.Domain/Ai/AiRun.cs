using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace IczpNet.Chat.Ai;

/// <summary>Durable lease-based AI execution; SQL is its source of truth.</summary>
public class AiRun : FullAuditedAggregateRoot<Guid>
{
    protected AiRun() { }

    public AiRun(Guid id, long sourceMessageId, Guid sessionId, Guid requesterSessionUnitId, string provider, DateTime now) : base(id)
    {
        SourceMessageId = sourceMessageId;
        SessionId = sessionId;
        RequesterSessionUnitId = requesterSessionUnitId;
        Provider = provider;
        Status = AiRunStatus.Queued;
        NextAttemptAt = now;
        MaxAttempts = 3;
    }

    public long SourceMessageId { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid RequesterSessionUnitId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public AiRunStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTime NextAttemptAt { get; private set; }
    public string? LeaseOwner { get; private set; }
    public DateTime? LeaseUntilTime { get; private set; }
    public DateTime? LastHeartbeatTime { get; private set; }
    public DateTime? StartedTime { get; private set; }
    public DateTime? CompletedTime { get; private set; }
    public DateTime? DeadlineTime { get; private set; }
    public long? OutputMessageId { get; private set; }
    public string? LastErrorCode { get; private set; }
    public string? LastErrorMessage { get; private set; }

    public void Claim(string workerId, DateTime now, DateTime leaseUntil, DateTime deadlineAt)
    {
        Status = AiRunStatus.Running; AttemptCount++; LeaseOwner = workerId;
        LeaseUntilTime = leaseUntil; LastHeartbeatTime = now; StartedTime ??= now; DeadlineTime = deadlineAt;
    }

    public void Heartbeat(string workerId, DateTime now, DateTime leaseUntil)
    {
        if (Status == AiRunStatus.Running && LeaseOwner == workerId) { LastHeartbeatTime = now; LeaseUntilTime = leaseUntil; }
    }

    public void Complete(string workerId, long outputMessageId, DateTime now)
    {
        EnsureLeaseOwner(workerId); Status = AiRunStatus.Completed; OutputMessageId = outputMessageId;
        CompletedTime = now; LeaseOwner = null; LeaseUntilTime = null;
    }

    public void RetryOrFail(string workerId, string errorCode, string errorMessage, DateTime now, TimeSpan retryDelay)
    {
        EnsureLeaseOwner(workerId); LastErrorCode = errorCode; LastErrorMessage = errorMessage; LeaseOwner = null; LeaseUntilTime = null;
        if (AttemptCount >= MaxAttempts) { Status = AiRunStatus.Failed; CompletedTime = now; return; }
        Status = AiRunStatus.RetryScheduled; NextAttemptAt = now.Add(retryDelay);
    }

    public void RequeueExpiredLease(DateTime now)
    {
        if (Status != AiRunStatus.Running || LeaseUntilTime is null || LeaseUntilTime > now) return;
        LeaseOwner = null; LeaseUntilTime = null; LastErrorCode = "lease-expired";
        LastErrorMessage = "AI worker lease expired before the run completed.";
        Status = AttemptCount >= MaxAttempts ? AiRunStatus.Failed : AiRunStatus.RetryScheduled; NextAttemptAt = now;
        if (Status == AiRunStatus.Failed) CompletedTime = now;
    }

    private void EnsureLeaseOwner(string workerId)
    {
        if (Status != AiRunStatus.Running || !string.Equals(LeaseOwner, workerId, StringComparison.Ordinal))
            throw new InvalidOperationException("The AI run is no longer leased by this worker.");
    }
}
