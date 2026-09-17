namespace IczpNet.Chat.Ai;

public enum AiRunStatus
{
    Queued = 0,
    Running = 1,
    Completed = 2,
    RetryScheduled = 3,
    Failed = 4,
    TimedOut = 5,
    Cancelled = 6
}
