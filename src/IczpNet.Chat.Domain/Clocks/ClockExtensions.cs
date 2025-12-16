using System;
using System.Threading;
using Volo.Abp.Timing;

namespace IczpNet.Chat.Clocks;

/// <summary>
/// 
/// 使用：
/// long ts = _clock.ToUnixTimeMilliseconds();
/// DateTime dt = _clock.FromUnixSeconds(ts);
/// 
/// var utcTime = _clock.Now.ToUniversalTime();
/// </summary>
public static class ClockExtensions
{

    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

    public static long ToUnixTimeSeconds(this DateTime dateTime)
        => new DateTimeOffset(dateTime).ToUnixTimeSeconds();

    public static long ToUnixTimeMilliseconds(this IClock clock)
        => new DateTimeOffset(clock.Now).ToUnixTimeMilliseconds();

    public static long ToUnixTimeSeconds(this IClock clock)
        => new DateTimeOffset(clock.Now).ToUnixTimeSeconds();

    //public static DateTime FromUnixMilliseconds(this IClock clock, long ms)
    //    => DateTimeOffset.FromUnixTimeMilliseconds(ms).LocalDateTime;

    //public static DateTime FromUnixSeconds(this IClock clock, long sec)
    //    => DateTimeOffset.FromUnixTimeSeconds(sec).LocalDateTime;
}