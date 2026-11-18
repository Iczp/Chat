using System;

namespace IczpNet.Chat.CommandPayloads;

/// <summary>
/// Non-persistent AI response events. These must never be treated as chat
/// messages: they are visual state only. They are sent through the standard
/// Chat distributed-event and SignalR route.
/// </summary>
public abstract class AiStreamCommandPayload
{
    public virtual string RunId { get; set; }
    public virtual Guid SessionId { get; set; }
    public virtual Guid RequesterSessionUnitId { get; set; }
    public virtual long SourceMessageId { get; set; }
    public virtual int Sequence { get; set; }
}

[Serializable]
public class AiStreamStartedCommandPayload : AiStreamCommandPayload
{
    public virtual string Status { get; set; } = "started";
}

[Serializable]
public class AiStreamDeltaCommandPayload : AiStreamCommandPayload
{
    public virtual string Delta { get; set; }
}

[Serializable]
public class AiStreamCompletedCommandPayload : AiStreamCommandPayload
{
    public virtual long FinalMessageId { get; set; }
}

[Serializable]
public class AiStreamFailedCommandPayload : AiStreamCommandPayload
{
    public virtual string Error { get; set; }
}
