using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Ai;

public class AiRunStateDto
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
    public List<AiRunTimelineItemDto> Timeline { get; set; } = [];
}

public class AiRunTimelineItemDto
{
    public DateTime OccurredAt { get; set; }
    public string EventType { get; set; }
    public string Status { get; set; }
    public int Sequence { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public string Detail { get; set; }
}
