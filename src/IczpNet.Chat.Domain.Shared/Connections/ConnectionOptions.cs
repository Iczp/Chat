namespace IczpNet.Chat.Connections;

public class ConnectionOptions
{
    /// <summary>
    /// 
    /// </summary>
    public int InactiveSeconds { get; set; } = 120;

    /// <summary>
    /// 
    /// </summary>
    public int TimerPeriodSeconds { get; set; } = 10;

    /// <summary>
    /// 缓存Key
    /// </summary>
    public string ConnectionIdsCacheKey { get; set; } = $"{nameof(ConnectionIdsCacheKey)}_v0.1";

    /// <summary>
    /// 缓存过期时间(秒)
    /// </summary>
    public int ConnectionCacheExpirationSeconds { get; set; } = 86400;

}
