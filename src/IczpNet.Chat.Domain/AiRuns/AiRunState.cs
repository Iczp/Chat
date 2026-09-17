using System;
using System.Collections.Generic;

namespace IczpNet.Chat.AiRuns;

[Serializable]
public class AiRunState
{
    public string RunId { get; set; }
    public Guid SessionId { get; set; }
    public Guid RequesterSessionUnitId { get; set; }
    public long SourceMessageId { get; set; }
    public string Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long QueueMilliseconds { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public int Sequence { get; set; }
    public string PreviewText { get; set; }
    public long? FinalMessageId { get; set; }
    public string Error { get; set; }
    public List<AiRunTimelineItem> Timeline { get; set; } = [];
}

[Serializable]
public class AiRunTimelineItem
{
    public DateTime OccurredAt { get; set; }
    public string EventType { get; set; }
    public string Status { get; set; }
    public int Sequence { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string Detail { get; set; }
}

/// <summary>Immutable input for atomic AI Run state transitions.</summary>
[Serializable]
public class AiRunStateEvent
{
    public string RunId { get; set; }
    public Guid SessionId { get; set; }
    public Guid RequesterSessionUnitId { get; set; }
    public long SourceMessageId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime OccurredAt { get; set; }
    public long QueueMilliseconds { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public int Sequence { get; set; }
    public string Status { get; set; }
    public string EventType { get; set; }
    public string Delta { get; set; }
    public long? FinalMessageId { get; set; }
    public string Error { get; set; }
}
