using IczpNet.Chat.Connections;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RedisServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Json;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionStatManager : RedisService, IConnectionStatManager
{
    public IOptions<ConnectionOptions> ConnectionOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<ConnectionOptions>>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();

    protected virtual string Prefix => $"{Options.Value.KeyPrefix}{ConnectionOptions.Value.AllConnectionsCacheKey}:";

    #region Redis Keys

    private RedisKey MinuteIndexKey() => $"{Prefix}Statistic:Minute:Index";
    private RedisKey MinuteCountKey() => $"{Prefix}Statistic:Minute:Count";
    private RedisKey HourIndexKey() => $"{Prefix}Statistic:Hour:Index";
    private RedisKey HourCountKey() => $"{Prefix}Statistic:Hour:Count";
    private RedisKey DayIndexKey() => $"{Prefix}Statistic:Day:Index";
    private RedisKey DayCountKey() => $"{Prefix}Statistic:Day:Count";
    private RedisKey OnlinePeakKey() => $"{Prefix}Statistic:OnlinePeak";

    #endregion

    #region Lua

    /// <summary>
    /// 通用聚合脚本（分钟 / 小时 / 天共用）
    /// </summary>
    private static string AggregateScript => @"
-- KEYS[1] = index zset
-- KEYS[2] = count hash
-- ARGV[1] = member
-- ARGV[2] = timestamp(ms)
-- ARGV[3] = increment
-- ARGV[4] = cleanup before(ms)

redis.call('ZADD', KEYS[1], ARGV[2], ARGV[1])
redis.call('HINCRBY', KEYS[2], ARGV[1], ARGV[3])

local expired = redis.call(
    'ZRANGEBYSCORE',
    KEYS[1],
    '-inf',
    ARGV[4]
)

if #expired > 0 then
    redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', ARGV[4])
    redis.call('HDEL', KEYS[2], unpack(expired))
end

return 1
        ";

    /// <summary>
    /// 峰值 Lua（保证并发安全 + 详情）
    /// </summary>
    private static string PeakScript => @"

-- KEYS[1] = peak key
-- ARGV[1] = current online count
-- ARGV[2] = json detail

local current = redis.call('GET', KEYS[1])

if not current then
    redis.call('SET', KEYS[1], ARGV[2])
    return 1
end

local decoded = cjson.decode(current)

if tonumber(ARGV[1]) > tonumber(decoded.count) then
    redis.call('SET', KEYS[1], ARGV[2])
    return 1
end

return 0
";

    #endregion

    #region Record Connection

    public async Task RecordConnectionAsync()
    {
        var now = Clock.Now;

        await RecordAsync(MinuteIndexKey(), MinuteCountKey(), now.ToString("yyyyMMddHHmm"), now, 1, now.AddDays(-7));

        await RecordAsync(HourIndexKey(), HourCountKey(), now.ToString("yyyyMMddHH"), now, 1, now.AddDays(-30));

        await RecordAsync(DayIndexKey(), DayCountKey(), now.ToString("yyyyMMdd"), now, 1, now.AddDays(-180));
    }

    private Task<RedisResult> RecordAsync(RedisKey indexKey, RedisKey countKey, string member, DateTimeOffset now, long count, DateTimeOffset cleanupBefore)
    {
        Logger.LogInformation($"StatRecordAsync indexKey={indexKey},countKey={countKey}", indexKey, countKey);
        return Database.ScriptEvaluateAsync(
            AggregateScript,
            [indexKey, countKey],
            [
                member,
                now.ToUnixTimeMilliseconds(),
                count,
                cleanupBefore.ToUnixTimeMilliseconds()
            ]);
    }

    #endregion

    #region Peak

    public async Task RecordOnlinePeakAsync(long onlineCount, string reason)
    {
        var detail = new OnlinePeakInfo
        {
            Count = onlineCount,
            Time = Clock.Now,
            Host = Environment.MachineName,
            Reason = reason
        };

        var json = JsonSerializer.Serialize(detail);

        await Database.ScriptEvaluateAsync(
            PeakScript,
            [OnlinePeakKey()],
            [
                onlineCount,
                json
            ]);
    }

    public async Task<OnlinePeakInfo> GetOnlinePeakAsync()
    {
        var value = await Database.StringGetAsync(OnlinePeakKey());
        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<OnlinePeakInfo>(value!);
    }

    #endregion

    #region Query

    public async Task<long> SumAsync(StatGranularity granularity, DateTimeOffset start, DateTimeOffset end)
    {
        RedisKey indexKey;
        RedisKey countKey;

        switch (granularity)
        {
            case StatGranularity.Minute:
                indexKey = MinuteIndexKey();
                countKey = MinuteCountKey();
                break;
            case StatGranularity.Hour:
                indexKey = HourIndexKey();
                countKey = HourCountKey();
                break;
            case StatGranularity.Day:
                indexKey = DayIndexKey();
                countKey = DayCountKey();
                break;
            default:
                throw new NotImplementedException();
        }

        var members = await Database.SortedSetRangeByScoreAsync(
            indexKey,
            start.ToUnixTimeMilliseconds(),
            end.ToUnixTimeMilliseconds());

        if (members.Length == 0)
            return 0;

        var values = await Database.HashGetAsync(countKey, members);

        return values.Sum(v => (long)v);
    }

    #endregion
}
