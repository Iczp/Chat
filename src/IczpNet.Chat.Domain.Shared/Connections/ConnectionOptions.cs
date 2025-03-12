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
    /// 
    /// </summary>
    public string ConnectionIdsCacheKey { get; set; } = $"{nameof(ConnectionIdsCacheKey)}_v0.1";

}
