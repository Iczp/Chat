using AutoMapper.Internal;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
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
    private string LastMessageSetKey(Guid sessionId)
        => $"{Prefix}LastMessages:SessionId-{sessionId}";
    private string OwnerSortedSetKey(long ownerId)
        => $"{Prefix}Owners:Sorted:OwnerId-{ownerId}";

    private string OwnerExistsSetKey(long ownerId)
        => $"{Prefix}Owners:Exists:OwnerId-{ownerId}";

    // composite score multiplier (sorting * MULT + lastMessageId)
    private const double OWNER_SCORE_MULT = 1_000_000_000_000d; // 1e12

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

    #endregion

    #region Initialize / Ensure helpers

    private static HashEntry[] MapToHashEntries(SessionUnitCacheItem unit)
    {

        return RedisMapper.ToHashEntries(unit);
    }
    public async Task SetListBySessionAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        var unitList = units.ToList();
        if (unitList.Count == 0) return;

        var sessionSetKey = SessionSetKey(sessionId);
        //var lastMsgKey = LastMessageSetKey(sessionId);

        var batch = Database.CreateBatch();

        // clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(sessionSetKey);

        var setAddTasks = new List<Task<bool>>();
        var hashTasks = new List<Task>();
        var zsetAddTasks = new List<Task<bool>>();

        foreach (var unit in unitList)
        {
            var idStr = unit.Id.ToString();
            setAddTasks.Add(batch.SetAddAsync(sessionSetKey, idStr));

            var unitKey = UnitKey(unit.Id);

            var entries = MapToHashEntries(unit);

            hashTasks.Add(batch.HashSetAsync(unitKey, entries));

            //if (unit.LastMessageId.HasValue)
            //{
            //    zsetAddTasks.Add(batch.SortedSetAddAsync(lastMsgKey, idStr, unit.LastMessageId.Value));
            //}

            _ = batch.KeyExpireAsync(unitKey, _cacheExpire);
        }

        _ = batch.KeyExpireAsync(sessionSetKey, _cacheExpire);
        //_ = batch.KeyExpireAsync(lastMsgKey, _cacheExpire);

        batch.Execute();

        if (setAddTasks.Count > 0) await Task.WhenAll(setAddTasks);
        if (hashTasks.Count > 0) await Task.WhenAll(hashTasks);
        if (zsetAddTasks.Count > 0) await Task.WhenAll(zsetAddTasks);
    }

    public async Task SetListBySessionAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        ArgumentNullException.ThrowIfNull(fetchTask);
        var units = (await fetchTask(sessionId))?.ToList() ?? [];
        await SetListBySessionAsync(sessionId, units);
    }

    public async Task SetListBySessionIfNotExistsAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        var sessionSetKey = SessionSetKey(sessionId);

        if (!await Database.KeyExistsAsync(sessionSetKey))
        {
            await SetListBySessionAsync(sessionId, fetchTask);
        }
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

        var boolHashTasks = new List<Task<bool>>();

        var hashTasks = new List<Task>
        {
            batch.SetAddAsync(ownerExistsKey, true)
        };
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

        var zsetOwnerTasks = new List<Task<bool>>();

        foreach (var unit in unCachedUnits)
        {
            var idStr = unit.Id.ToString();

            var unitKey = UnitKey(unit.Id);

            var entries = MapToHashEntries(unit);

            hashTasks.Add(batch.HashSetAsync(unitKey, entries));

            boolHashTasks.Add(batch.KeyExpireAsync(unitKey, _cacheExpire));

            // score
            var score = GetScore(unit.Sorting, unit.LastMessageId ?? 0);

            zsetOwnerTasks.Add(batch.SortedSetAddAsync(ownerSortedKey, idStr, score));
        }

        if (_cacheExpire.HasValue)
        {
            boolHashTasks.Add(batch.KeyExpireAsync(ownerSortedKey, _cacheExpire));
        }
        batch.Execute();

        if (zsetOwnerTasks.Count > 0) await Task.WhenAll(zsetOwnerTasks);

        if (hashTasks.Count > 0) await Task.WhenAll(hashTasks);

        if (boolHashTasks.Count > 0) await Task.WhenAll(boolHashTasks);

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
        return await SetListByOwnerIfNotExistsAsync(ownerId, fetchTask) ?? await GetListByOwnerIdAsync(ownerId);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetListByOwnerIdAsync(long ownerId)
    {
        string ownerSortedKey = OwnerSortedSetKey(ownerId);
        // read owner zset (may be empty)
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: ownerSortedKey,
            order: Order.Descending
            );
        var redisMap = redisZset.ToDictionary(x => Guid.Parse(x.Element), x => x.Score);
        var unitIdList = redisMap.Keys.ToList();
        var listMap = await GetManyAsync(unitIdList);
        return listMap.Select(x => x.Value)
            .OrderByDescending(x => x.Sorting)
            .ThenByDescending(x => x.LastMessageId);
    }

    #endregion

    #region Map / GetMany

    public async Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetManyAsync(IEnumerable<Guid> unitIds)
    {
        var unitIdList = unitIds?.ToList();
        var batch = Database.CreateBatch();

        var tasks = new Task<HashEntry[]>[unitIdList.Count];

        for (int i = 0; i < unitIdList.Count; i++)
        {
            var unitId = unitIdList[i];
            tasks[i] = batch.HashGetAllAsync(UnitKey(unitId));
        }

        batch.Execute();

        await Task.WhenAll(tasks);

        var arr = new KeyValuePair<Guid, SessionUnitCacheItem>[unitIdList.Count];

        for (int i = 0; i < unitIdList.Count; i++)
        {
            var entries = tasks[i].Result;
            var unitId = unitIdList[i];
            //arr[unitId] = MapHashEntriesToCacheItem(entries);
            var val = RedisMapper.ToObject<SessionUnitCacheItem>(entries);
            arr[i] = new KeyValuePair<Guid, SessionUnitCacheItem>(unitId, val);
        }

        return arr;
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
        var hashTasks = new List<Task>();
        foreach (var nullKey in nullKeys)
        {
            if (fetchedMap.TryGetValue(nullKey, out var fetchedItem) && fetchedItem != null)
            {
                var unitKey = UnitKey(nullKey);
                var entries = MapToHashEntries(fetchedItem);
                hashTasks.Add(batch.HashSetAsync(unitKey, entries));
                var index = list.FindIndex(x => x.Key == nullKey);
                if (index >= 0)
                {
                    list[index] = new KeyValuePair<Guid, SessionUnitCacheItem>(nullKey, fetchedItem);
                }
            }
        }
        batch.Execute();

        if (hashTasks.Count > 0)
        {
            await Task.WhenAll(hashTasks);
        }

        return list;
    }

    #endregion

    #region BatchIncrementBadgeAndSetLastMessageAsync (updates owner zset score)

    private static double GetScore(double sorting, long lastMessageId)
    {
        return sorting * OWNER_SCORE_MULT + lastMessageId;
    }

    public async Task BatchIncrementBadgeAndSetLastMessageAsync(
        Message message,
        TimeSpan? expire = null)
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
    }

    #endregion

    #region Remove / Set / Misc

    public async Task AddUnitIdToSessionAsync(Guid sessionId, Guid unitId)
    {
        await Database.SetAddAsync(SessionSetKey(sessionId), unitId.ToString());
        await Database.KeyExpireAsync(SessionSetKey(sessionId), _cacheExpire);
    }

    public async Task RemoveUnitIdFromSessionAsync(Guid sessionId, Guid unitId)
    {
        await Database.SetRemoveAsync(SessionSetKey(sessionId), unitId.ToString());
        await Database.KeyDeleteAsync(UnitKey(unitId));
    }

    public async Task<bool> RemoveManyAsync(Guid sessionId, IEnumerable<Guid> unitIds)
    {
        var batch = Database.CreateBatch();
        //var zkey = LastMessageSetKey(sessionId);

        var delTasks = new List<Task<bool>>();
        var zremoveTasks = new List<Task<bool>>();

        foreach (var id in unitIds)
        {
            delTasks.Add(batch.KeyDeleteAsync(UnitKey(id)));
            //zremoveTasks.Add(batch.SortedSetRemoveAsync(zkey, id.ToString()));
            _ = batch.SetRemoveAsync(SessionSetKey(sessionId), id.ToString());
        }

        batch.Execute();

        if (delTasks.Count > 0) await Task.WhenAll(delTasks);
        if (zremoveTasks.Count > 0) await Task.WhenAll(zremoveTasks);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid sessionId, Guid unitId)
    {
        return await RemoveManyAsync(sessionId, new[] { unitId });
    }

    public async Task<bool> RemoveAllAsync(Guid sessionId)
    {
        var sessionSetKey = SessionSetKey(sessionId);
        //var lastKey = LastMessageSetKey(sessionId);

        var r1 = await Database.KeyDeleteAsync(sessionSetKey);
        //var r2 = await Database.KeyDeleteAsync(lastKey);
        return r1;//&& r2;
    }

    public async Task UpdateReadedMessageIdAsync(Guid unitId, long readedMessageId, TimeSpan? expire = null)
    {
        var key = UnitKey(unitId);
        await Database.HashSetAsync(key, F_Setting_ReadedMessageId, readedMessageId);
        if (expire.HasValue) await Database.KeyExpireAsync(key, expire.Value);
    }

    public async Task<bool> SetExpireAsync(Guid sessionId, Guid? unitId = null, TimeSpan? expire = null)
    {
        var e = expire ?? _cacheExpire;
        var tasks = new List<Task<bool>>();
        if (unitId.HasValue)
        {
            tasks.Add(Database.KeyExpireAsync(UnitKey(unitId.Value), e));
        }
        else
        {
            tasks.Add(Database.KeyExpireAsync(SessionSetKey(sessionId), e));
            //tasks.Add(Database.KeyExpireAsync(LastMessageSetKey(sessionId), e));
        }

        var results = await Task.WhenAll(tasks);
        return results.All(x => x);
    }

    public async Task UpdateCountersync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask)
    {
        var unitKey = UnitKey(counter.Id);

        //
        var isExists = await Database.KeyExistsAsync(unitKey);

        var batch = Database.CreateBatch();

        if (!isExists)
        {
            // º”‘ÿª∫¥ÊœÓ

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
