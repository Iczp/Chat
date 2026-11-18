using System;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// A non-persistent AI response lifecycle event sent to the originating
/// session unit through the normal Chat distributed-event to SignalR path.
/// The completed AI answer remains a regular persisted <see cref="Message"/>.
/// </summary>
[Serializable]
public class AiStreamToClientDistributedEto
{
    public virtual string Command { get; set; }

    public virtual string RunId { get; set; }

    public virtual Guid SessionId { get; set; }

    /// <summary>
    /// The session unit that sent the source message. Only this unit's owner
    /// (on all of their active devices) receives the transient event.
    /// </summary>
    public virtual Guid RequesterSessionUnitId { get; set; }

    public virtual long SourceMessageId { get; set; }

    /// <summary>When the user message was persisted.</summary>
    public virtual DateTime RequestedAt { get; set; }

    /// <summary>When the AI worker actually began this run.</summary>
    public virtual DateTime StartedAt { get; set; }

    /// <summary>When this lifecycle event was emitted.</summary>
    public virtual DateTime OccurredAt { get; set; }

    /// <summary>Time from persisted user message to worker execution.</summary>
    public virtual long QueueMilliseconds { get; set; }

    /// <summary>Elapsed time since <see cref="StartedAt"/>.</summary>
    public virtual long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Monotonically increases inside one AI run; clients discard duplicates
    /// and out-of-order deliveries.
    /// </summary>
    public virtual int Sequence { get; set; }

    public virtual string Status { get; set; }

    public virtual string Delta { get; set; }

    public virtual long? FinalMessageId { get; set; }

    public virtual string Error { get; set; }
}
