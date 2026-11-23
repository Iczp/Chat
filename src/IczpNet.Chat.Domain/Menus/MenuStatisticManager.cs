using IczpNet.Chat.RedisServices;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Menus;

public class MenuStatisticManager(
    IRepository<Menu, Guid> menuRepository,
    IUnitOfWorkManager unitOfWorkManager,
    IAbpDistributedLock distributedLock) : RedisService, IMenuStatisticManager, ITransientDependency
{
    protected IRepository<Menu, Guid> MenuRepository { get; } = menuRepository;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected IAbpDistributedLock DistributedLock { get; } = distributedLock;

    private static string GetTotalPvKey(Guid menuId) => $"Chat:Menu:Stat:PV:Total:{menuId:N}";
    private static string GetTotalUvKey(Guid menuId) => $"Chat:Menu:Stat:UV:Total:{menuId:N}";
    private static string GetDailyPvKey(Guid menuId, string date) => $"Chat:Menu:Stat:PV:Daily:{menuId:N}:{date}";
    private static string GetDailyUvKey(Guid menuId, string date) => $"Chat:Menu:Stat:UV:Daily:{menuId:N}:{date}";
    private static string GetDirtyPvKey() => "Chat:Menu:Stat:Dirty:PV";
    private static string GetDirtyUvKey() => "Chat:Menu:Stat:Dirty:UV";
    private const string FlushLockKey = "Chat:Menu:Stat:FlushLock";

    public virtual async Task RecordClickAsync(Guid menuId, long chatObjectId, long? senderId = null)
    {
        try
        {
            var date = Clock.Now.ToString("yyyyMMdd");
            var totalPvKey = GetTotalPvKey(menuId);
            var totalUvKey = GetTotalUvKey(menuId);
            var dailyPvKey = GetDailyPvKey(menuId, date);
            var dailyUvKey = GetDailyUvKey(menuId, date);
            var menuIdStr = (RedisValue)menuId.ToString("N");

            var batch = Database.CreateBatch();

            // 1. PV 原子累加（包含总计、今日以及未落库增量记录）
            var t1 = batch.StringIncrementAsync(totalPvKey);
            var t2 = batch.StringIncrementAsync(dailyPvKey);
            _ = batch.HashIncrementAsync(GetDirtyPvKey(), menuIdStr, 1);
            _ = batch.KeyExpireAsync(dailyPvKey, TimeSpan.FromDays(7));

            // 2. UV 去重记录（HyperLogLog 总计、今日以及脏 UV 集合）
            if (senderId.HasValue && senderId.Value > 0)
            {
                var userValue = (RedisValue)senderId.Value.ToString();
                _ = batch.HyperLogLogAddAsync(totalUvKey, userValue);
                _ = batch.HyperLogLogAddAsync(dailyUvKey, userValue);
                _ = batch.KeyExpireAsync(dailyUvKey, TimeSpan.FromDays(7));
                _ = batch.SetAddAsync(GetDirtyUvKey(), menuIdStr);
            }

            batch.Execute();
            await Task.WhenAll(t1, t2);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, $"[MenuStatisticManager] RecordClickAsync failed for menuId={menuId}");
        }
    }

    public virtual async Task<(long TotalClicks, long UniqueUsers, long TodayClicks, long TodayUsers)> GetMenuStatsAsync(Guid menuId)
    {
        var dict = await GetBatchMenuStatsAsync([menuId]);
        return dict.TryGetValue(menuId, out var stats) ? stats : (0, 0, 0, 0);
    }

    public virtual async Task<Dictionary<Guid, (long TotalClicks, long UniqueUsers, long TodayClicks, long TodayUsers)>> GetBatchMenuStatsAsync(IEnumerable<Guid> menuIds)
    {
        var list = menuIds?.Distinct().ToList() ?? [];
        if (list.Count == 0) return [];

        var date = Clock.Now.ToString("yyyyMMdd");
        var batch = Database.CreateBatch();

        var tasks = list.Select(id => new
        {
            MenuId = id,
            TotalPv = batch.StringGetAsync(GetTotalPvKey(id)),
            TotalUv = batch.HyperLogLogLengthAsync(GetTotalUvKey(id)),
            DailyPv = batch.StringGetAsync(GetDailyPvKey(id, date)),
            DailyUv = batch.HyperLogLogLengthAsync(GetDailyUvKey(id, date))
        }).ToList();

        batch.Execute();

        var result = new Dictionary<Guid, (long TotalClicks, long UniqueUsers, long TodayClicks, long TodayUsers)>(list.Count);
        var missingPvIds = new List<Guid>();

        foreach (var t in tasks)
        {
            var totalPvRedis = await t.TotalPv;
            var totalUvVal = await t.TotalUv;
            var dailyPvVal = (long)await t.DailyPv;
            var dailyUvVal = await t.DailyUv;

            if (!totalPvRedis.HasValue)
            {
                // Redis 中无此键（可能发生缓存清除或未预热），登记需要从数据库回源
                missingPvIds.Add(t.MenuId);
                result[t.MenuId] = (0, totalUvVal, dailyPvVal, dailyUvVal);
            }
            else
            {
                result[t.MenuId] = ((long)totalPvRedis, totalUvVal, dailyPvVal, dailyUvVal);
            }
        }

        // 缓存失效回源防丢保障：若 Redis 键不存在，从数据库查询基数并预热回 Redis，绝不出现清缓存后显示为 0
        if (missingPvIds.Count > 0)
        {
            try
            {
                var dbMenus = await MenuRepository.GetListAsync(x => missingPvIds.Contains(x.Id));
                var warmupBatch = Database.CreateBatch();
                foreach (var dbMenu in dbMenus)
                {
                    var current = result[dbMenu.Id];
                    var recoveredPv = Math.Max(current.TotalClicks, dbMenu.ClickCount);
                    var recoveredUv = Math.Max(current.UniqueUsers, dbMenu.ClickUsersCount);
                    result[dbMenu.Id] = (recoveredPv, recoveredUv, current.TodayClicks, current.TodayUsers);

                    _ = warmupBatch.StringSetAsync(GetTotalPvKey(dbMenu.Id), recoveredPv);
                }
                warmupBatch.Execute();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[MenuStatisticManager] Fallback from database failed for missing menus");
            }
        }

        return result;
    }

    public virtual async Task<int> FlushDirtyAsync()
    {
        if (DistributedLock is Volo.Abp.DistributedLocking.NullAbpDistributedLock)
        {
            return await ExecuteFlushInternalAsync();
        }

        await using var handle = await DistributedLock.TryAcquireAsync(FlushLockKey, TimeSpan.FromSeconds(5));
        if (handle == null)
        {
            Logger.LogDebug("[MenuStatisticManager] FlushDirtyAsync skipped: another instance is flushing.");
            return 0;
        }

        return await ExecuteFlushInternalAsync();
    }

    protected virtual async Task<int> ExecuteFlushInternalAsync()
    {
        var db = Database;
        var dirtyPvKey = GetDirtyPvKey();
        var dirtyUvKey = GetDirtyUvKey();

        var batchToken = Guid.NewGuid().ToString("N");
        var procPvKey = $"Chat:Menu:Stat:Proc:PV:{batchToken}";
        var procUvKey = $"Chat:Menu:Stat:Proc:UV:{batchToken}";

        bool hasPv = false;
        bool hasUv = false;

        try
        {
            if (await db.KeyExistsAsync(dirtyPvKey))
            {
                await db.KeyRenameAsync(dirtyPvKey, procPvKey);
                hasPv = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "[MenuStatisticManager] Rename dirty PV key skipped or failed.");
        }

        try
        {
            if (await db.KeyExistsAsync(dirtyUvKey))
            {
                await db.KeyRenameAsync(dirtyUvKey, procUvKey);
                hasUv = true;
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "[MenuStatisticManager] Rename dirty UV key skipped or failed.");
        }

        if (!hasPv && !hasUv)
        {
            return 0;
        }

        var pvDeltas = new Dictionary<Guid, long>();
        if (hasPv)
        {
            var hashEntries = await db.HashGetAllAsync(procPvKey);
            foreach (var entry in hashEntries)
            {
                if (Guid.TryParse(entry.Name.ToString(), out var menuId) && long.TryParse(entry.Value.ToString(), out var delta) && delta > 0)
                {
                    pvDeltas[menuId] = delta;
                }
            }
        }

        var uvMenuIds = new HashSet<Guid>();
        if (hasUv)
        {
            var members = await db.SetMembersAsync(procUvKey);
            foreach (var m in members)
            {
                if (Guid.TryParse(m.ToString(), out var menuId))
                {
                    uvMenuIds.Add(menuId);
                }
            }
        }

        var allMenuIds = pvDeltas.Keys.Union(uvMenuIds).Distinct().ToList();
        if (allMenuIds.Count == 0)
        {
            if (hasPv) await db.KeyDeleteAsync(procPvKey);
            if (hasUv) await db.KeyDeleteAsync(procUvKey);
            return 0;
        }

        // UV 统计从 HyperLogLog 读取最新统计
        var uvCounts = new Dictionary<Guid, long>();
        if (uvMenuIds.Count > 0)
        {
            var uvBatch = db.CreateBatch();
            var uvTasks = uvMenuIds.Select(id => new
            {
                Id = id,
                Task = uvBatch.HyperLogLogLengthAsync(GetTotalUvKey(id))
            }).ToList();
            uvBatch.Execute();

            foreach (var t in uvTasks)
            {
                uvCounts[t.Id] = await t.Task;
            }
        }

        int affected = 0;
        using (var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var entities = await MenuRepository.GetListAsync(x => allMenuIds.Contains(x.Id));
            foreach (var entity in entities)
            {
                bool changed = false;
                if (pvDeltas.TryGetValue(entity.Id, out var delta))
                {
                    entity.ClickCount += delta;
                    changed = true;
                }
                if (uvCounts.TryGetValue(entity.Id, out var uvLength) && uvLength > entity.ClickUsersCount)
                {
                    entity.ClickUsersCount = uvLength;
                    changed = true;
                }

                if (changed)
                {
                    await MenuRepository.UpdateAsync(entity);
                    affected++;
                }
            }
            await uow.CompleteAsync();
        }

        // 数据库落库成功后，删除临时处理 key
        if (hasPv) await db.KeyDeleteAsync(procPvKey);
        if (hasUv) await db.KeyDeleteAsync(procUvKey);

        return affected;
    }
}
