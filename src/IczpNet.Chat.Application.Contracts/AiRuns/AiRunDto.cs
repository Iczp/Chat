using IczpNet.Chat.Ai;
using IczpNet.Chat.BaseDtos;
using System;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.AiRuns;

[Serializable]
public class AiRunDto : BaseDto<Guid>, IHasModificationTime
{
    public virtual long SourceMessageId { get; set; }
    public virtual Guid SessionId { get; set; }
    public virtual Guid RequesterSessionUnitId { get; set; }
    public virtual string Provider { get; set; }
    public virtual AiRunStatus Status { get; set; }
    public virtual int AttemptCount { get; set; }
    public virtual int MaxAttempts { get; set; }
    public virtual DateTime NextAttemptAt { get; set; }
    public virtual string LeaseOwner { get; set; }
    public virtual DateTime? LeaseUntilTime { get; set; }
    public virtual DateTime? LastHeartbeatTime { get; set; }
    public virtual DateTime? StartedTime { get; set; }
    public virtual DateTime? CompletedTime { get; set; }
    public virtual DateTime? DeadlineTime { get; set; }
    public virtual long? OutputMessageId { get; set; }
    public virtual string LastErrorCode { get; set; }
    public virtual string LastErrorMessage { get; set; }
    public virtual DateTime? LastModificationTime { get; set; }
}
