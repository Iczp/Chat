using System;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitOptions
{
    /// <summary>
    /// 批量加载大小
    /// </summary>
    public int BatchLoadSize { get; set; } = 200;

    /// <summary>
    /// 缓存过期时间
    /// </summary>
    public TimeSpan? CacheExpire { get; set; } = TimeSpan.FromDays(1);
}
