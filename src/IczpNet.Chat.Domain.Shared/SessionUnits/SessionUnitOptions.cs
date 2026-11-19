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

    private int _maxPinnedCount = 30;

    /// <summary>
    /// 单个聊天对象最大置顶会话数（默认 30，允许配置范围 1 ~ 99）
    /// </summary>
    public int MaxPinnedCount
    {
        get => _maxPinnedCount;
        set => _maxPinnedCount = Math.Clamp(value, 1, 99);
    }

}
