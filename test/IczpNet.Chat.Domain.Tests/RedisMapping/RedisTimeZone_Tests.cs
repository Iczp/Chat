using System;
using StackExchange.Redis;
using Xunit;

namespace IczpNet.Chat.RedisMapping;

public class RedisTimeZone_Tests
{
    public class Activity
    {
        public DateTime ActiveTime { get; set; }
    }

    [Fact]
    public void Redis_unix_ISO_and_explicit_offset_restore_the_same_UTC_instant()
    {
        var instant = new DateTime(2026, 10, 11, 16, 0, 0, DateTimeKind.Utc);
        var milliseconds = new DateTimeOffset(instant).ToUnixTimeMilliseconds();
        RedisValue[] representations = [milliseconds, "2026-10-11T16:00:00.0000000Z", "2026-10-12T00:00:00.0000000+08:00"];

        foreach (var value in representations)
        {
            var result = new[] { new HashEntry(nameof(Activity.ActiveTime), value) }.ToObject<Activity>();
            Assert.Equal(instant, result.ActiveTime);
            Assert.Equal(DateTimeKind.Utc, result.ActiveTime.Kind);
            Assert.Equal(milliseconds, new DateTimeOffset(result.ActiveTime).ToUnixTimeMilliseconds());
        }

        Assert.Equal(instant, ((double)milliseconds).ToUtcDateTime());
        Assert.Equal(DateTimeKind.Utc, ((double)milliseconds).ToUtcDateTime().Value.Kind);
    }
}
