namespace IczpNet.Chat.AiRuns;

public class AiRunDispatcherOptions
{
    /// <summary>
    /// 是否启用 AI 任务调度器
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 轮询间隔（毫秒）
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 1000;

    /// <summary>
    /// 全局最大并发运行任务数
    /// </summary>
    public int MaxConcurrentRuns { get; set; } = 8;

    /// <summary>
    /// 单次租约时长（秒）
    /// </summary>
    public int LeaseSeconds { get; set; } = 45;

    /// <summary>
    /// 心跳续约周期（秒），必须小于 LeaseSeconds / 3
    /// </summary>
    public int HeartbeatSeconds { get; set; } = 10;

    /// <summary>
    /// 单次执行超时时间预算（秒）
    /// </summary>
    public int ExecutionTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// 最大尝试次数
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// 首次重试延迟（秒）
    /// </summary>
    public int RetryFirstDelaySeconds { get; set; } = 15;

    /// <summary>
    /// 重试指数退避乘数
    /// </summary>
    public double RetryBackoffFactor { get; set; } = 2.0;
}
