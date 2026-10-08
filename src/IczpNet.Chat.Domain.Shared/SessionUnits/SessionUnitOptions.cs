using System;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitOptions
{
    public bool AutoFlushDirtyEnabled { get; set; } = true;
    public int FlushDirtyPeriodSeconds { get; set; } = 10;
    /// <summary>
    /// 批量加载大小
    /// </summary>
    public int BatchLoadSize { get; set; } = 200;

    /// <summary>
    /// 缓存过期时间
    /// </summary>
    public TimeSpan? CacheExpire { get; set; } = TimeSpan.FromDays(1);

}
