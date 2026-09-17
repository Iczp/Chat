using System;

namespace IczpNet.Chat.Ai;

public class AiRunContext
{
    public Guid RunId { get; set; }
    public long SourceMessageId { get; set; }
    public Guid SessionId { get; set; }
    public Guid RequesterSessionUnitId { get; set; }
    public string Provider { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? DeadlineTime { get; set; }
}
