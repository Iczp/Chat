﻿namespace IczpNet.Chat.Connections;

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
    public string ConnectionIdsCacheKey { get; set; } = $"{nameof(ConnectionIdsCacheKey)}_v0.1";

    /// <summary>
    /// 缓存过期时间(秒)
    /// </summary>
    public int ConnectionCacheExpirationSeconds { get; set; } = 86400 * 30;

}
