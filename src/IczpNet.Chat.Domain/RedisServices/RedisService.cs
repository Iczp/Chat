using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;


namespace IczpNet.Chat.RedisServices;

public abstract class RedisService : DomainService
{
    protected IOptions<AbpDistributedCacheOptions> Options => LazyServiceProvider.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>();
    protected IConnectionMultiplexer ConnectionMultiplexer => LazyServiceProvider.LazyGetRequiredService<IConnectionMultiplexer>();
    protected IDatabase Database => ConnectionMultiplexer.GetDatabase();


    /// <summary>
    /// 1. key 不存在 : 返回 nil（不创建）
    /// 2. key 存在 : 执行 HINCRBY(key, field, increment)
    /// 3. 若结果 小于 0, 设置为 0
    /// </summary>
    protected static string IncrementIfExistsScript => @"
if redis.call('EXISTS', KEYS[1]) == 1 then
    local newValue = redis.call('HINCRBY', KEYS[1], ARGV[1], ARGV[2])
    if newValue < 0 then
        redis.call('HSET', KEYS[1], ARGV[1], 0)
        return 0
    else
        return newValue
    end
else
    return nil
end
";
    /// <summary>
    /// key 不存在 → 返回 nil（不创建）
    /// key 存在 & member 存在 → ZINCRBY
    /// 结果< 0 → 设为 0
    /// member 不存在 → 不创建，返回 nil
    /// </summary>
    protected static string IncrementIfExistsZsetScript => @"
if redis.call('EXISTS', KEYS[1]) == 1 then
    local current = redis.call('ZSCORE', KEYS[1], ARGV[1])
    if not current then
        return nil
    end

    local newValue = redis.call('ZINCRBY', KEYS[1], ARGV[1], ARGV[2])
    if tonumber(newValue) < 0 then
        redis.call('ZADD', KEYS[1], 0, ARGV[1])
        return 0
    else
        return tonumber(newValue)
    end
else
    return nil
end
";

    protected static string DecrementOrDeleteZsetScript => @"
if redis.call('EXISTS', KEYS[1]) ~= 1 then
    return nil
end

local current = redis.call('ZSCORE', KEYS[1], ARGV[1])
if not current then
    return nil
end

-- 减分
local newValue = redis.call('ZINCRBY', KEYS[1], ARGV[1], -tonumber(ARGV[2]))
local deleteWhenZero = tonumber(ARGV[3]) == 1

if tonumber(newValue) <= 0 then
    if deleteWhenZero then
        redis.call('ZREM', KEYS[1], ARGV[1])
        return 0
    else
        -- 保留成员，强制设为 0
        redis.call('ZADD', KEYS[1], 0, ARGV[1])
        return 0
    end
end

return tonumber(newValue)
";


    protected virtual async Task<T> MeasureAsync<T>(string name, Func<Task<T>> func)
    {
        var sw = Stopwatch.StartNew();
        var result = await func();
        Logger.LogInformation("[{fullname}] [{name}] Elapsed Time: {Elapsed} ms",
            GetType().FullName,
            name,
            sw.ElapsedMilliseconds);
        sw.Stop();
        return result;
    }

    /// <summary>
    /// 批量检查 Key 是否存在
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    protected virtual async Task<Dictionary<string, bool>> BatchKeyExistsAsync(IEnumerable<string> keys)
    {
        var stopwatch = Stopwatch.StartNew();

        var keyList = keys as IList<string> ?? keys.ToList();
        if (keyList.Count == 0)
        {
            return [];
        }

        var batch = Database.CreateBatch();
        var tasks = new Task<bool>[keyList.Count];

        for (int i = 0; i < keyList.Count; i++)
        {
            tasks[i] = batch.KeyExistsAsync(keyList[i]);
        }

        batch.Execute();

        var results = await Task.WhenAll(tasks);

        var dict = new Dictionary<string, bool>(keyList.Count);
        for (int i = 0; i < keyList.Count; i++)
        {
            dict[keyList[i]] = results[i];
        }

        Logger.LogInformation(
            "[{Method}] keys:{Count}, Elapsed:{Elapsed}ms",
            nameof(BatchKeyExistsAsync),
            keyList.Count,
            stopwatch.ElapsedMilliseconds);

        return dict;
    }

    protected static void IncrementIfExists(IBatch batch, string key, string member, double increment)
    {
        _ = batch.ScriptEvaluateAsync(IncrementIfExistsScript,
            [key],
            [member, increment]);
    }
    protected Task<RedisResult> IncrementIfExistsAsync(string key, string member, double increment)
    {
        return Database.ScriptEvaluateAsync(IncrementIfExistsScript,
            [key],
            [member, increment]);
    }

    protected static void DecrementOrDeleteZset(IBatch batch, string key, string member, double decrement, bool deleteWhenZero = true)
    {
        if (decrement == 0 && deleteWhenZero)
        {
            _ = batch.SortedSetRemoveAsync(key, member);
        }
        else
        {
            _ = batch.ScriptEvaluateAsync(IncrementIfExistsZsetScript, [key], [member, -decrement]);
        }
    }
    protected static void DecrementOrDeleteZset1(IBatch batch, string key, string member, double decrement, bool deleteWhenZero = true)
    {
        _ = batch.ScriptEvaluateAsync(DecrementOrDeleteZsetScript,
            [key],
            [member, decrement, deleteWhenZero ? 1 : 0]);
    }

}
