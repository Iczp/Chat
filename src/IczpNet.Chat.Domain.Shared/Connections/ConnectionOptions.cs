namespace IczpNet.Chat.Connections;

public class ConnectionOptions
{
    /// <summary>
    /// 活跃时间(秒)
    /// </summary>
    public int InactiveSeconds { get; set; } = 120;

    /// <summary>
    /// 在线检查定时器周期(秒)
    /// </summary>
    public int TimerPeriodSeconds { get; set; } = 10;

    /// <summary>
    /// 缓存Key
    /// </summary>
    public string AllConnectionsCacheKey { get; set; } = $"Online";

    /// <summary>
    /// 缓存过期时间(秒)
    /// </summary>
    public int ConnectionCacheExpirationSeconds { get; set; } = 60 * 5;

    /// <summary>
    /// 是否启用 单独 好友在线状态统计（单独Key）
    /// </summary>
    public bool IsEnableExclusiveStatFriends { get; set; } = false;

}
