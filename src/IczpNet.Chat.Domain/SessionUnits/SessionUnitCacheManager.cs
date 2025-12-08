using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitCacheManager(
    IOptions<AbpDistributedCacheOptions> options,
    IConnectionMultiplexer connection) : DomainService, ISessionUnitCacheManager
{
    protected readonly IDatabase Database = connection.GetDatabase();

    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public IOptions<AbpDistributedCacheOptions> Options { get; } = options;

    private readonly TimeSpan? _cacheExpire = TimeSpan.FromDays(7);

    protected string Prefix => $"{Options.Value.KeyPrefix}SessionUnits:";
    // unitKey builders
    private string UnitKey(Guid unitId)
        => $"{Prefix}Units:UnitId-{unitId}";
    private string SessionSetKey(Guid sessionId)
        => $"{Prefix}Sessions:SessionId-{sessionId}";
    private string OwnerSortedSetKey(long ownerId)
        => $"{Prefix}ChatObjects:Sorted:OwnerId-{ownerId}";
    private string OwnerExistsSetKey(long ownerId)
        => $"{Prefix}ChatObjects:Exists:OwnerId-{ownerId}";
    private string OwnerTotalBadgeSetKey(long ownerId)
        => $"{Prefix}ChatObjects:TotalBadges:OwnerId-{ownerId}";

    // composite score multiplier (sorting * MULT + lastMessageId)
    private const double OWNER_SCORE_MULT = 1_000_000_000_000d; // 1e12

    private static readonly string IncrementTotalBadgeIfExistsScript = @"
if redis.call('EXISTS', KEYS[1]) == 1 then
    return redis.call('HINCRBY', KEYS[1], ARGV[1], ARGV[2])
else
    return nil
end
";

    #region Field names
    private static string F_Id => nameof(SessionUnitCacheItem.Id);
    private static string F_SessionId => nameof(SessionUnitCacheItem.SessionId);
    private static string F_OwnerId => nameof(SessionUnitCacheItem.OwnerId);
    private static string F_OwnerObjectType => nameof(SessionUnitCacheItem.OwnerObjectType);
    private static string F_DestinationId => nameof(SessionUnitCacheItem.DestinationId);
    private static string F_DestinationObjectType => nameof(SessionUnitCacheItem.DestinationObjectType);
    private static string F_IsStatic => nameof(SessionUnitCacheItem.IsStatic);
    private static string F_IsPublic => nameof(SessionUnitCacheItem.IsPublic);
    private static string F_IsVisible => nameof(SessionUnitCacheItem.IsVisible);
    private static string F_IsEnabled => nameof(SessionUnitCacheItem.IsEnabled);
    //private static string F_ReadedMessageId => nameof(SessionUnitCacheItem.ReadedMessageId);
    private static string F_LastMessageId => nameof(SessionUnitCacheItem.LastMessageId);
    private static string F_PublicBadge => nameof(SessionUnitCacheItem.PublicBadge);
    private static string F_PrivateBadge => nameof(SessionUnitCacheItem.PrivateBadge);
    private static string F_RemindAllCount => nameof(SessionUnitCacheItem.RemindAllCount);
    private static string F_RemindMeCount => nameof(SessionUnitCacheItem.RemindMeCount);
    private static string F_FollowingCount => nameof(SessionUnitCacheItem.FollowingCount);
    private static string F_Ticks => nameof(SessionUnitCacheItem.Ticks);
    private static string F_Sorting => nameof(SessionUnitCacheItem.Sorting);


    private static string F_Setting_ReadedMessageId => $"{nameof(SessionUnitCacheItem.Setting)}.{nameof(SessionUnitCacheItem.Setting.ReadedMessageId)}";

    private static string F_TotalBadge => "TotalBadge";

    #endregion

    #region Initialize / Ensure helpers

    private static HashEntry[] MapToHashEntries(SessionUnitCacheItem unit)
    {
        return RedisMapper.ToHashEntries(unit);
    }
    public async Task<IEnumerable<SessionUnitCacheItem>> SetListBySessionAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        var unitList = units.ToList();
        if (unitList.Count == 0) return [];

        var sessionSetKey = SessionSetKey(sessionId);
        //var lastMsgKey = LastMessageSetKey(sessionId);

        var batch = Database.CreateBatch();

        // clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(sessionSetKey);

        foreach (var unit in unitList)
        {
            var unitId = unit.Id.ToString();

            _ = batch.HashSetAsync(sessionSetKey, unit.OwnerId, unitId);

            var unitKey = UnitKey(unit.Id);

            var entries = MapToHashEntries(unit);

            _ = batch.HashSetAsync(unitKey, entries);

            //if (unit.LastMessageId.HasValue)
            //{
            //    zsetAddTasks.Add(batch.SortedSetAddAsync(lastMsgKey, unitId, unit.LastMessageId.Value));
            //}

            _ = batch.KeyExpireAsync(unitKey, _cacheExpire);
        }
        _ = batch.KeyExpireAsync(sessionSetKey, _cacheExpire);
        //_ = batch.KeyExpireAsync(lastMsgKey, _cacheExpire);

        batch.Execute();

        return units;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetListBySessionAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        ArgumentNullException.ThrowIfNull(fetchTask);
        var units = (await fetchTask(sessionId))?.ToList() ?? [];
        return await SetListBySessionAsync(sessionId, units);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetListBySessionIfNotExistsAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        var sessionSetKey = SessionSetKey(sessionId);

        if (!await Database.KeyExistsAsync(sessionSetKey))
        {
            return await SetListBySessionAsync(sessionId, fetchTask);
        }
        return null;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetOrSetListBySessionAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        return await SetListBySessionIfNotExistsAsync(sessionId, fetchTask) ?? await GetListBySessionAsync(sessionId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:SessionUnitId, Value:OwnerId}</returns>
    public async Task<IDictionary<Guid, long>> GetDictBySessionAsync(Guid sessionId)
    {
        var sessionSetKey = SessionSetKey(sessionId);

        if (!await Database.KeyExistsAsync(sessionSetKey))
        {
            return new Dictionary<Guid, long>();
        }
        var entries = await Database.HashGetAllAsync(sessionSetKey);

        var dict = entries.ToDictionary(x => Guid.Parse(x.Value), x => long.Parse(x.Name));

        return dict;
    }

    public async Task<IDictionary<long, Guid>> GetUnitsBySessionAsync(Guid sessionId, List<long> ownerIds)
    {
        var sessionSetKey = SessionSetKey(sessionId);

        // 直接 Hash 长度判断即可，无需 KeyExistsAsync（减少一次 Redis 请求）
        var length = await Database.HashLengthAsync(sessionSetKey);
        if (length == 0)
        {
            return new Dictionary<long, Guid>();
        }

        // ownerIds = null 或 ownerIds.Count == 0 返回全部
        if (ownerIds == null || ownerIds.Count == 0)
        {
            var entries = await Database.HashGetAllAsync(sessionSetKey);

            return entries.ToDictionary(
                e => (long)e.Value,      // value 是 ownerId
                e => Guid.Parse(e.Name)        // name 是 unitId
            );
        }

        // 只取指定 ownerIds 用 Batch 批量获取
        var batch = Database.CreateBatch();
        var tasks = new Dictionary<long, Task<RedisValue>>(ownerIds.Count);

        foreach (var ownerId in ownerIds)
        {
            tasks[ownerId] = batch.HashGetAsync(sessionSetKey, ownerId);
        }

        batch.Execute();

        var result = new Dictionary<long, Guid>(ownerIds.Count);

        foreach (var kv in tasks)
        {
            var ownerId = kv.Key;
            var val = await kv.Value;

            if (val.IsNullOrEmpty)
            {
                // 没有对应 unitId 设为 Guid.Empty（或可不放入）
                result[ownerId] = Guid.Empty;
            }
            else
            {
                result[ownerId] = Guid.Parse(val.ToString());
            }
        }

        return result;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetListBySessionAsync(Guid sessionId)
    {
        var dict = await GetDictBySessionAsync(sessionId);

        var unitIds = dict.Keys.Distinct().ToList();

        var kvs = await GetManyAsync(unitIds);

        return kvs.Where(x => x.Value != null).Select(x => x.Value);
    }

    #endregion

    #region GetListByOwnerIdAsync (DB initial values + Redis merge)


    public async Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units)
    {
        var unitList = units?.Where(x => x != null && x.OwnerId == ownerId).ToList() ?? [];

        var batch = Database.CreateBatch();

        var ownerExistsKey = OwnerExistsSetKey(ownerId);

        // clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(ownerExistsKey);

        _ = batch.SetAddAsync(ownerExistsKey, true);

        //foreach (var unitId in unitList)
        //{
        //    hashTasks.Add(batch.SetAddAsync(ownerExistsKey, unitId.Id.ToString()));
        //}
        _ = batch.KeyExpireAsync(ownerExistsKey, _cacheExpire);

        // owner zset unitKey
        var ownerSortedKey = OwnerSortedSetKey(ownerId);

        var unitMap = await GetManyAsync(unitList.Select(x => x.Id));

        var cachedUnits = unitMap.Where(x => x.Value != null).Select(x => x.Value).ToList();

        var unCachedUnits = unitList.ExceptBy(cachedUnits.Select(x => x.Id), x => x.Id).ToList();

        foreach (var unit in unCachedUnits)
        {
            var idStr = unit.Id.ToString();

            var unitKey = UnitKey(unit.Id);

            var entries = MapToHashEntries(unit);

            _ = batch.HashSetAsync(unitKey, entries);

            _ = batch.KeyExpireAsync(unitKey, _cacheExpire);

            // score
            var score = GetScore(unit.Sorting, unit.LastMessageId ?? 0);

            _ = batch.SortedSetAddAsync(ownerSortedKey, idStr, score);
        }

        if (_cacheExpire.HasValue)
        {
            _ = batch.KeyExpireAsync(ownerSortedKey, _cacheExpire);
        }
        batch.Execute();

        var list = cachedUnits.Concat(unCachedUnits);

        return list;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        ArgumentNullException.ThrowIfNull(fetchTask);
        var units = (await fetchTask(ownerId))?.ToList() ?? [];
        return await SetListByOwnerAsync(ownerId, units);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerIfNotExistsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        var ownerExistsKey = OwnerExistsSetKey(ownerId);
        if (!await Database.KeyExistsAsync(ownerExistsKey))
        {
            return await SetListByOwnerAsync(ownerId, fetchTask);
        }
        return null;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetOrSetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        return await SetListByOwnerIfNotExistsAsync(ownerId, fetchTask) ?? await GetListByOwnerAsync(ownerId);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetListByOwnerAsync(long ownerId, double minScore = double.NegativeInfinity, double maxScore = double.PositiveInfinity, long skip = 0, long take = -1)
    {
        string ownerSortedKey = OwnerSortedSetKey(ownerId);

        // read owner zset (may be empty)
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: ownerSortedKey,
            start: minScore,
            stop: maxScore,
            skip: skip,
            take: take,
            order: Order.Descending);

        // 按序取
        var unitIdList = redisZset.Select(x => Guid.Parse(x.Element)).ToList();

        var listMap = (await GetManyAsync(unitIdList)).ToDictionary(x => x.Key, x => x.Value); ;

        var result = unitIdList.Select(id => listMap[id]).ToList();

        return result;
    }



    #endregion

    #region Map / GetMany

    public async Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetManyAsync(IEnumerable<Guid> unitIds)
    {
        if (unitIds == null) return [];

        var idList = unitIds.ToList();
        if (idList.Count == 0) return [];

        var batch = Database.CreateBatch();
        var tasks = new List<Task<HashEntry[]>>(idList.Count);

        // pipeline 全部 HashGetAll
        foreach (var unitId in idList)
        {
            tasks.Add(batch.HashGetAllAsync(UnitKey(unitId)));
        }

        // 一次提交（高性能关键点）
        batch.Execute();

        // 等待全部完成
        var resultEntries = await Task.WhenAll(tasks);

        // 构建结果
        var result = new KeyValuePair<Guid, SessionUnitCacheItem>[idList.Count];

        for (int i = 0; i < idList.Count; i++)
        {
            var entries = resultEntries[i];

            var item =
                (entries == null || entries.Length == 0)
                ? null
                : RedisMapper.ToObject<SessionUnitCacheItem>(entries);

            result[i] = new KeyValuePair<Guid, SessionUnitCacheItem>(idList[i], item);
        }

        return result;
    }

    public async Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetOrSetManyAsync(IEnumerable<Guid> unitIds, Func<List<Guid>, Task<KeyValuePair<Guid, SessionUnitCacheItem>[]>> func)
    {
        var list = await GetManyAsync(unitIds);

        var nullKeys = list.Where(x => x.Value == null).Select(x => x.Key).ToList();

        if (nullKeys.Count == 0)
        {
            return list;
        }
        var fetchedList = await func(nullKeys);

        var fetchedMap = fetchedList.ToDictionary(x => x.Key, x => x.Value);

        var batch = Database.CreateBatch();

        foreach (var nullKey in nullKeys)
        {
            if (!fetchedMap.TryGetValue(nullKey, out var fetchedItem) || fetchedItem != null)
            {
                continue;
            }
            var unitKey = UnitKey(nullKey);
            var entries = MapToHashEntries(fetchedItem);
            _ = batch.HashSetAsync(unitKey, entries);
            var index = list.FindIndex(x => x.Key == nullKey);
            if (index >= 0)
            {
                list[index] = new KeyValuePair<Guid, SessionUnitCacheItem>(nullKey, fetchedItem);
            }
        }
        batch.Execute();

        return list;
    }

    #endregion

    #region BatchIncrementBadgeAndSetLastMessageAsync (updates owner zset score)

    private static double GetScore(double sorting, long lastMessageId)
    {
        //return sorting * OWNER_SCORE_MULT + lastMessageId;
        return lastMessageId;
    }

    public async Task BatchIncrement_BackAsync(Message message, TimeSpan? expire = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var sessionId = message.SessionId!.Value;
        var lastMessageId = message.Id;
        //var lastMsgKey = LastMessageSetKey(sessionId);
        var isPrivate = message.IsPrivateMessage();
        var isRemindAll = message.IsRemindAll;

        var sessionUnitList = await SessionUnitManager.GetCacheListAsync(message);
        if (sessionUnitList == null || sessionUnitList.Count == 0) return;

        var batch = Database.CreateBatch();

        var hashSetTasks = new List<Task<bool>>();
        var hashIncTasks = new List<Task<long>>();
        var zsetGlobalTasks = new List<Task<bool>>();
        var zsetOwnerTasks = new List<Task<bool>>();
        var expireTasks = new List<Task<bool>>();
        var deleteKeyTasks = new List<Task<bool>>();
        foreach (var unit in sessionUnitList)
        {
            var id = unit.Id;
            var unitKey = UnitKey(id);
            var idStr = id.ToString();
            var isSender = id == message.SenderSessionUnitId;

            if (!isSender)
            {
                if (isPrivate) hashIncTasks.Add(batch.HashIncrementAsync(unitKey, F_PrivateBadge, 1));
                else hashIncTasks.Add(batch.HashIncrementAsync(unitKey, F_PublicBadge, 1));
                if (isRemindAll) hashIncTasks.Add(batch.HashIncrementAsync(unitKey, F_RemindAllCount, 1));
                var ownerTotalBadgeKey = OwnerTotalBadgeSetKey(unit.OwnerId);
                deleteKeyTasks.Add(batch.KeyDeleteAsync(ownerTotalBadgeKey));
                //hashIncTasks.Add(batch.HashIncrementAsync(ownerTotalBadgeKey, F_TotalBadge, 1));
            }

            // update lastMessageId in hash
            hashSetTasks.Add(batch.HashSetAsync(unitKey, F_LastMessageId, lastMessageId));
            // update ticks
            hashSetTasks.Add(batch.HashSetAsync(unitKey, F_Ticks, (double)DateTime.UtcNow.Ticks));
            var expireTime = expire ?? _cacheExpire;
            if (expireTime.HasValue)
            {
                expireTasks.Add(batch.KeyExpireAsync(unitKey, expire ?? _cacheExpire));
            }

            // owner score
            var ownerSortedKey = OwnerSortedSetKey(unit.OwnerId);
            var score = GetScore(unit.Sorting, lastMessageId);
            zsetOwnerTasks.Add(batch.SortedSetAddAsync(ownerSortedKey, idStr, score));
            if (expireTime.HasValue)
            {
                expireTasks.Add(batch.KeyExpireAsync(ownerSortedKey, expire ?? _cacheExpire));
            }
        }

        batch.Execute();

        if (hashSetTasks.Count > 0) await Task.WhenAll(hashSetTasks);
        if (hashIncTasks.Count > 0) await Task.WhenAll(hashIncTasks);
        if (zsetGlobalTasks.Count > 0) await Task.WhenAll(zsetGlobalTasks);
        if (zsetOwnerTasks.Count > 0) await Task.WhenAll(zsetOwnerTasks);
        if (expireTasks.Count > 0) await Task.WhenAll(expireTasks);
        if (deleteKeyTasks.Count > 0) await Task.WhenAll(deleteKeyTasks);
    }

    public async Task BatchIncrementAsync(Message message, TimeSpan? expire = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var sessionId = message.SessionId!.Value;
        var lastMessageId = message.Id;

        var isPrivate = message.IsPrivateMessage();
        var isRemindAll = message.IsRemindAll;
        var reminderIds = message.MessageReminderList.Select(x => x.SessionUnitId).ToHashSet();
        var followerIds = message.MessageFollowerList.Select(x => x.SessionUnitId).ToHashSet();

        //var unitList = await GetDictBySessionAsync(sessionId);
        var unitList = await GetListBySessionAsync(sessionId);
        //var unitList = await SessionUnitManager.GetCacheListAsync(message);

        if (unitList == null || !unitList.Any()) return;

        var batch = Database.CreateBatch();
        var expireTime = expire ?? _cacheExpire;

        foreach (var unit in unitList)
        {
            //var unitId = unit.Key;
            //var ownerId = unit.Value;
            var unitId = unit.Id;
            var ownerId = unit.OwnerId;
            var unitKey = UnitKey(unitId);
            var ownerSortedKey = OwnerSortedSetKey(ownerId);
            var ownerTotalBadgeKey = OwnerTotalBadgeSetKey(ownerId);
            var isSender = unitId == message.SenderSessionUnitId;

            // badge
            if (!isSender)
            {
                _ = batch.HashIncrementAsync(unitKey, isPrivate ? F_PrivateBadge : F_PublicBadge, 1);
                //remindAllCount
                if (isRemindAll)
                {
                    _ = batch.HashIncrementAsync(unitKey, F_RemindAllCount, 1);
                }
                //remindMeCount
                if (reminderIds.Contains(unitId))
                {
                    _ = batch.HashIncrementAsync(unitKey, F_RemindMeCount, 1);
                }
                // followingCount
                if (followerIds.Contains(unitId))
                {
                    _ = batch.HashIncrementAsync(unitKey, F_FollowingCount, 1);
                }

                //// delete owner badge
                //_ = batch.KeyDeleteAsync(ownerTotalBadgeKey);

                // 如果已经统计过角标直接+1
                _ = batch.ScriptEvaluateAsync(IncrementTotalBadgeIfExistsScript, [ownerTotalBadgeKey], [F_TotalBadge, 1]);
            }

            // lastMessageId
            _ = batch.HashSetAsync(unitKey, F_LastMessageId, lastMessageId);

            // ticks
            var ticks = (double)message.CreationTime.Ticks;
            _ = batch.HashSetAsync(unitKey, F_Ticks, ticks);

            // owner sortedset
            var score = GetScore(unit.Sorting, lastMessageId);
            _ = batch.SortedSetAddAsync(ownerSortedKey, unitId.ToString(), score);

            // expire
            if (expireTime.HasValue)
            {
                _ = batch.KeyExpireAsync(unitKey, expireTime.Value);
                _ = batch.KeyExpireAsync(ownerSortedKey, expireTime.Value);
            }
        }

        batch.Execute();

        Logger.LogInformation("BatchIncrementBadgeAndSetLastMessageAsync executed.");
    }

    public virtual async Task<bool> SetTotalBadgeAsync(long ownerId, long badge)
    {
        var ownerTotalBadgeKey = OwnerTotalBadgeSetKey(ownerId);
        var result = await Database.HashSetAsync(ownerTotalBadgeKey, F_TotalBadge, badge);
        return result;
    }

    public virtual async Task<long?> GetTotalBadgeAsync(long ownerId)
    {
        var ownerTotalBadgeKey = OwnerTotalBadgeSetKey(ownerId);

        var redisValue = await Database.HashGetAsync(ownerTotalBadgeKey, F_TotalBadge);

        var value = redisValue.ToString();

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (long.TryParse(redisValue.ToString(), out long badge))
        {
            return badge;
        }
        return null;
    }

    public virtual async Task<bool> RemoveTotalBadgeAsync(long ownerId)
    {
        var ownerTotalBadgeKey = OwnerTotalBadgeSetKey(ownerId);
        return await Database.HashDeleteAsync(ownerTotalBadgeKey, F_TotalBadge);
    }

    #endregion

    #region Remove / Set / Misc

    public async Task UpdateCountersync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask)
    {
        var unitKey = UnitKey(counter.Id);

        //
        var isExists = await Database.KeyExistsAsync(unitKey);

        var batch = Database.CreateBatch();

        if (!isExists)
        {
            // 加载缓存项

            var unit = await fetchTask(counter.Id);

            var hashTasks = new List<Task>();

            var boolHashTasks = new List<Task<bool>>();

            unit.PublicBadge = counter.PublicBadge;
            unit.PrivateBadge = counter.PrivateBadge;
            unit.RemindAllCount = counter.RemindAllCount;
            unit.RemindMeCount = counter.RemindMeCount;
            unit.FollowingCount = counter.FollowingCount;
            unit.Setting.ReadedMessageId = counter.ReadedMessageId;

            var entries = MapToHashEntries(unit);

            hashTasks.Add(batch.HashSetAsync(unitKey, entries));

            if (_cacheExpire.HasValue)
            {
                boolHashTasks.Add(Database.KeyExpireAsync(unitKey, _cacheExpire));
            }
            batch.Execute();
            await Task.WhenAll(boolHashTasks);
            await Task.WhenAll(hashTasks);
            return;
        }

        var setTasks = new List<Task<bool>>
        {
            batch.HashSetAsync(unitKey, F_PublicBadge, counter.PublicBadge),
            batch.HashSetAsync(unitKey, F_PrivateBadge, counter.PrivateBadge),
            batch.HashSetAsync(unitKey, F_RemindAllCount, counter.RemindAllCount),
            batch.HashSetAsync(unitKey, F_RemindMeCount, counter.RemindMeCount),
            batch.HashSetAsync(unitKey, F_FollowingCount, counter.FollowingCount),
            batch.HashSetAsync(unitKey, F_Setting_ReadedMessageId, counter.ReadedMessageId)
        };

        if (_cacheExpire.HasValue)
        {
            setTasks.Add(Database.KeyExpireAsync(unitKey, _cacheExpire));
        }

        batch.Execute();

        await Task.WhenAll(setTasks);
    }

    public async Task<SessionUnitCacheItem> UnitTestAsync()
    {
        var item = new SessionUnitCacheItem
        {
            PublicBadge = 5,
            Setting = new SessionUnitSettingCacheItem
            {
                HistoryLastTime = DateTime.UtcNow,
                JoinWay = JoinWays.Invitation,
            },
        };

        var entries = RedisMapper.ToHashEntries(item); // or ToHashEntries_V2 is provided as robust flatten (see file)

        var key = "IM:Mappers:unit-test";
        await Database.HashSetAsync(key, entries);

        var entries2 = await Database.HashGetAllAsync(key);
        var item2 = RedisMapper.ToObject<SessionUnitCacheItem>(entries2);

        return item2;
    }


    #endregion
}
