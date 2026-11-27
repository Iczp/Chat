using System;
using IczpNet.Chat.AiRuns;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.AiRuns.Dtos;

public class AiRunAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public AiRunStatus? Status { get; set; }

    public string Provider { get; set; }

    public Guid? SessionId { get; set; }

    public Guid? RequesterSessionUnitId { get; set; }

    public long? SourceMessageId { get; set; }

    public long? OutputMessageId { get; set; }

    public string LeaseOwner { get; set; }

    public DateTime? StartCreationTime { get; set; }

    public DateTime? EndCreationTime { get; set; }
}

public class AiRunAdminDto : EntityDto<Guid>
{
    public long SourceMessageId { get; set; }

    public Guid SessionId { get; set; }

    public Guid RequesterSessionUnitId { get; set; }

    public string Provider { get; set; }

    public AiRunStatus Status { get; set; }

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; }

    public DateTime NextAttemptAt { get; set; }

    public string LeaseOwner { get; set; }

    public DateTime? LeaseUntilTime { get; set; }

    public DateTime? LastHeartbeatTime { get; set; }

    public DateTime? StartedTime { get; set; }

    public DateTime? CompletedTime { get; set; }

    public DateTime? DeadlineTime { get; set; }

    public long? OutputMessageId { get; set; }

    public string LastErrorCode { get; set; }

    public string LastErrorMessage { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class AiRunAdminCancelInput
{
    public string Reason { get; set; }
}

public class AiRunAdminCreateInput
{
    public long SourceMessageId { get; set; }

    public Guid SessionId { get; set; }

    public Guid RequesterSessionUnitId { get; set; }

    public string Provider { get; set; }

    public int MaxAttempts { get; set; } = 3;
}
