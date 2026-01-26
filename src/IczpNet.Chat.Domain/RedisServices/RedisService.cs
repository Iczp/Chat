using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.RedisMapping;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio.DataModel;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;


namespace IczpNet.Chat.RedisServices;

public abstract class RedisService : DomainService
{
    protected IOptions<AbpDistributedCacheOptions> Options => LazyServiceProvider.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>();
    protected IConnectionMultiplexer ConnectionMultiplexer => LazyServiceProvider.LazyGetRequiredService<IConnectionMultiplexer>();
    protected IDatabase Database => ConnectionMultiplexer.GetDatabase();

    protected virtual TimeSpan? CacheExpire { get; set; }

    /// <summary>
    /// Zset 仅在存在时修改
    /// </summary>
    protected static string ZsetUpdateIfExistsScript => @"
if redis.call('EXISTS', KEYS[1]) ~= 1 then
    return nil
end

-- 成员不存在，不处理
if not redis.call('ZSCORE', KEYS[1], ARGV[1]) then
    return nil
end

local score = tonumber(ARGV[2])
local deleteWhenZero = tonumber(ARGV[3]) == 1

if score == 0 and deleteWhenZero then
    redis.call('ZREM', KEYS[1], ARGV[1])
    return 0
end

-- 直接修改 score（不新增成员）
redis.call('ZADD', KEYS[1], 'XX', score, ARGV[1])

return score
";
    /// <summary>
    /// 1. key 不存在 : 返回 nil（不创建）
    /// 2. key 存在 : 执行 HINCRBY(key, field, increment)
    /// 3. 若结果 小于 0, 设置为 0
    /// </summary>
    protected static string HashIncrementIfExistsScript => @"
-- KEYS[1] = hash key
-- ARGV[1] = field
-- ARGV[2] = increment

local current = redis.call('HGET', KEYS[1], ARGV[1])
if not current then
    return nil
end

local newValue = redis.call(
    'HINCRBY',
    KEYS[1],
    ARGV[1],
    tonumber(ARGV[2])
)

if newValue < 0 then
    redis.call('HSET', KEYS[1], ARGV[1], 0)
    return 0
end

return newValue
";

    /// <summary>
    /// Key1 不存在 : 返回 nil（不创建）
    /// </summary>
    protected static string HIncrementIfGuardKeyExistScript => @"
if redis.call('EXISTS', KEYS[1]) ~= 1 then
    return nil
end

local newValue = redis.call('HINCRBY', KEYS[2], ARGV[1], ARGV[2])

if newValue < 0 then
    redis.call('HSET', KEYS[2], ARGV[1], 0)
    return 0
end

return newValue

";

    protected static string ZsetIncrementIfGuardKeyExistScript => @"
if redis.call('EXISTS', KEYS[1]) ~= 1 then
    return nil
end

-- ZSet 增量修改
local newValue = redis.call('ZINCRBY', KEYS[2], ARGV[2], ARGV[1])

-- 不允许小于 0
if tonumber(newValue) < 0 then
    redis.call('ZADD', KEYS[2], 0, ARGV[1])
    return 0
end

return tonumber(newValue)
";

    protected static string HashSetIfFieldExistsScript => @"
if redis.call('HEXISTS', KEYS[1], ARGV[1]) == 1 then
    redis.call('HSET', KEYS[1], ARGV[1], ARGV[2])
    return 1
end
return 0
";


    /// <summary>
    /// 存在才加，不创建
    /// key 不存在 → 返回 nil（不创建）
    /// key 存在 member 存在 → ZINCRBY
    /// 结果 小于 0 → 设为 0
    /// member 不存在 → 不创建，返回 nil
    /// </summary>
    protected static string ZsetIncrementIfExistsAndClampZeroScript => @"
-- KEYS[1] = zset key
-- ARGV[1] = member
-- ARGV[2] = increment

-- key 不存在 或 member 不存在 → nil
local current = redis.call('ZSCORE', KEYS[1], ARGV[1])
if not current then
    return nil
end

-- 加分（可能是负数 increment）
local newValue = redis.call(
    'ZINCRBY',
    KEYS[1],
    tonumber(ARGV[2]),
    ARGV[1]
)

-- clamp 到 0（防御性，保持与 Decrement 语义对称）
if tonumber(newValue) < 0 then
    redis.call('ZADD', KEYS[1], 0, ARGV[1])
    return 0
end

return tonumber(newValue)
";

    protected static string ZsetDecrementIfExistsAndClampZeroScript => @"
-- KEYS[1] = zset key
-- ARGV[1] = member
-- ARGV[2] = decrement
-- ARGV[3] = removeWhenZero (1 | 0)

local current = redis.call('ZSCORE', KEYS[1], ARGV[1])
if not current then
    return nil
end

-- 原子减分（注意参数顺序）
local newValue = redis.call(
    'ZINCRBY',
    KEYS[1],
    -tonumber(ARGV[2]),
    ARGV[1]
)

if tonumber(newValue) <= 0 then
    if tonumber(ARGV[3]) == 1 then
        redis.call('ZREM', KEYS[1], ARGV[1])
    else
        redis.call('ZADD', KEYS[1], 0, ARGV[1])
    end
    return 0
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
    protected virtual void Expire(IBatch batch, string key, TimeSpan? expiry = null, ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        if (CacheExpire.HasValue)
        {
            _ = batch.KeyExpireAsync(key, expiry ?? CacheExpire, when, flags);
        }
    }
    protected virtual void ExpireIf(bool condition, Func<RedisKey> redisKeyFunc, IBatch batch, ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        if (!condition)
        {
            return;
        }
        if (CacheExpire.HasValue)
        {
            _ = batch.KeyExpireAsync(redisKeyFunc(), CacheExpire, when, flags);
        }
    }

    protected void HashSetIf(bool condition, Func<RedisKey> redisKeyFunc, RedisValue field, RedisValue value, IBatch batch, TimeSpan? expire = null)
    {
        if (!condition)
        {
            return;
        }
        var redisKey = redisKeyFunc();
        _ = batch.HashSetAsync(redisKey, field, value);
        _ = batch.KeyExpireAsync(redisKey, expire ?? CacheExpire);
    }
    protected void HashRemoveIf(bool condition, Func<RedisKey> redisKeyFunc, RedisValue field, IBatch batch, bool refreshExpire = false)
    {
        if (!condition)
        {
            return;
        }
        var redisKey = redisKeyFunc();
        _ = batch.HashDeleteAsync(redisKey, field);
        if (refreshExpire)
        {
            _ = batch.KeyExpireAsync(redisKey, CacheExpire);
        }
    }

    protected void SortedSetIf(bool condition, Func<RedisKey> redisKeyFunc, RedisValue field, double score, IBatch batch, TimeSpan? expiry = null, TimeSpan expire = default)
    {
        if (!condition)
        {
            return;
        }
        var redisKey = redisKeyFunc();
        _ = batch.SortedSetAddAsync(redisKey, field, score);
        _ = batch.KeyExpireAsync(redisKey, expiry ?? CacheExpire);
    }

    protected void SortedRemoveIf(bool condition, Func<RedisKey> redisKeyFunc, RedisValue member, IBatch batch, bool refreshExpire = false)
    {
        if (!condition)
        {
            return;
        }
        var redisKey = redisKeyFunc();
        _ = batch.SortedSetRemoveAsync(redisKey, member);
        if (refreshExpire)
        {
            _ = batch.KeyExpireAsync(redisKey, CacheExpire);
        }
    }

    /// <summary>
    /// 批量检查 Key 是否存在
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    protected virtual async Task<Dictionary<string, bool>> BatchKeyExistsAsync(IEnumerable<RedisKey> keys)
    {
        var stopwatch = Stopwatch.StartNew();

        var keyList = keys as IList<RedisKey> ?? keys.ToList();
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

    protected static void HashIncrementIfExist(IBatch batch, string key, string member, double increment)
    {
        _ = batch.ScriptEvaluateAsync(HashIncrementIfExistsScript,
            [key],
            [member, increment]);
    }

    protected Task<RedisResult> HashIncrementIfExistsAsync(string key, string member, double increment)
    {
        return Database.ScriptEvaluateAsync(HashIncrementIfExistsScript,
            [key],
            [member, increment]);
    }

    protected static Task<RedisResult> HashSetIfFieldExistsAsync(IBatch batch, string key, RedisValue field, RedisValue value)
    {
        return batch.ScriptEvaluateAsync(HashSetIfFieldExistsScript,
            [key],
            [field, value]);
    }

    protected static void ZsetIncrementIfGuardKeyExist(IBatch batch, string guardKey, string key, RedisValue member, double increment)
    {
        _ = batch.ScriptEvaluateAsync(ZsetIncrementIfGuardKeyExistScript,
            [guardKey, key],
            [member, increment]);
    }

    protected static Task<RedisResult> ZsetUpdateIfExistsAsync(IDatabaseAsync batch, string key, RedisValue member, double score, bool removeWhenZero = false)
    {
        return batch.ScriptEvaluateAsync(ZsetUpdateIfExistsScript,
            [key],
            [member, score, removeWhenZero]);
    }

    protected static Task<RedisResult> ZsetDecrementIfExistsAndClampZeroAsync(IBatch batch, string key, string member, double decrement, bool removeWhenZero = true)
    {
        return batch.ScriptEvaluateAsync(
            ZsetDecrementIfExistsAndClampZeroScript,
            [key],
            [member, decrement, removeWhenZero ? 1 : 0]);
    }

    protected static Task<RedisResult> ZsetIncrementIfExistsAndClampZeroAsync(IBatch batch, string key, string member, double increment)
    {
        return batch.ScriptEvaluateAsync(
            ZsetIncrementIfExistsAndClampZeroScript,
            [key],
            [member, increment]);
    }

    protected async Task<double?[]> GetZsetScoresAsync(string key, RedisValue[] members, IBatch newBatch = null)
    {
        var batch = newBatch ?? Database.CreateBatch();
        var tasks = new Task<double?>[members.Length];

        for (int i = 0; i < members.Length; i++)
        {
            tasks[i] = batch.SortedSetScoreAsync(key, members[i]);
        }

        batch.Execute();
        await Task.WhenAll(tasks);

        return tasks.Select(t => t.Result).ToArray();
    }

    protected async Task<KeyValuePair<TKey, TCacheItem>[]> GetManyHashSet1Async<TKey, TCacheItem>(IEnumerable<TKey> ids, Func<TKey, RedisKey> keyFunc) where TCacheItem : new()
    {
        var batch = Database.CreateBatch();
        var tasks = ids.Select(x => new KeyValuePair<TKey, Task<HashEntry[]>>(x, batch.HashGetAllAsync(keyFunc(x))));
        batch.Execute();
        await Task.WhenAll(tasks.Select(x => x.Value));
        var result = tasks.Select(x => new KeyValuePair<TKey, TCacheItem>(x.Key, x.Value.Result.ToObject<TCacheItem>())).ToArray();
        return result;
    }

    protected async Task<KeyValuePair<TKey, TCacheItem>[]> GetManyHashSetAsync<TKey, TCacheItem>(IEnumerable<TKey> ids, Func<TKey, RedisKey> hashSetKeyFunc, CancellationToken token = default) where TCacheItem : new()
    {
        token.ThrowIfCancellationRequested();

        if (ids == null) return [];

        var idList = ids as IList<TKey> ?? ids.ToList();

        if (idList.Count == 0) return [];

        var batch = Database.CreateBatch();

        var tasks = new Task<HashEntry[]>[idList.Count];

        for (int i = 0; i < idList.Count; i++)
        {
            tasks[i] = batch.HashGetAllAsync(hashSetKeyFunc(idList[i]));
        }

        batch.Execute();

        // 等待 Redis 返回（此处无法真正取消 IO，只能取消等待）
        var entriesList = await Task.WhenAll(tasks);

        var result = new KeyValuePair<TKey, TCacheItem>[idList.Count];

        for (int i = 0; i < idList.Count; i++)
        {
            var entries = entriesList[i];
            var item = (entries == null || entries.Length == 0)
                ? default
                : RedisMapper.ToObject<TCacheItem>(entries);

            result[i] = new KeyValuePair<TKey, TCacheItem>(idList[i], item);
        }

        return result;
    }
}
