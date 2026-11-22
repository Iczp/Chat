using System;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话处置分布式事件传输对象
/// </summary>
[Serializable]
public class SessionUnitDisposedDistributedEto
{
    public string Command { get; set; } = CommandConsts.SessionUnitDisposed;

    public long DisposalRecordId { get; set; }

    public long OwnerId { get; set; }

    public Guid SessionUnitId { get; set; }

    public Guid? SessionId { get; set; }

    public long? DestinationId { get; set; }

    public SessionUnitDisposalType DisposalType { get; set; }

    public SessionUnitDisposalReason Reason { get; set; }

    public long? OperatorId { get; set; }

    public DateTime? ClearBeforeTime { get; set; }

    public long? ClearMaxMessageId { get; set; }

    public string Description { get; set; }

    public DateTime CreationTime { get; set; }
}
