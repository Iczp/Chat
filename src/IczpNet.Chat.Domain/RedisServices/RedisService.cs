using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
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
}
