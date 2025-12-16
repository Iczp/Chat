using IczpNet.Chat.Clocks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
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
    private string OwnerToppingSetKey(long ownerId)
       => $"{Prefix}ChatObjects:Toppings:OwnerId-{ownerId}";
    private string OwnerExistsSetKey(long ownerId)
        => $"{Prefix}ChatObjects:Exists:OwnerId-{ownerId}";
    private string OwnerBadgeSetKey(long ownerId)
        => $"{Prefix}ChatObjects:TotalBadges:OwnerId-{ownerId}";


    /// <summary>
    /// 1. key 不存在 : 返回 nil（不创建）
    /// 2. key 存在 : 执行 HINCRBY(key, field, increment)
    /// 3. 若结果 &lt;0,设置为 0
    /// </summary>
    private static string IncrementIfExistsScript => @"
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
    /// 1e16
    /// </summary>
    public const long OWNER_SCORE_MULT = 10_000_000_000_000_000; // 1e16
    private static double GetScore(double sorting, double ticks)
    {
        var score = sorting * OWNER_SCORE_MULT + ticks;

        return score;
    }

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

    // total
    private static string F_Total_Badge => "TotalBadge";
    private static string F_Total_Public => "Public";
    private static string F_Total_Private => "Private";
    private static string F_Total_Following => "Following";
    private static string F_Total_RemindMe => "RemindMe";
    private static string F_Total_RemindAll => "RemindAll";

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

            // set top
            if (unit.Sorting > 0)
            {
                var toppingKey = OwnerToppingSetKey(unit.OwnerId);
                _ = batch.SortedSetAddAsync(toppingKey, unitId, unit.Sorting);
                _ = batch.KeyExpireAsync(toppingKey, _cacheExpire);
            }

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
                e => (long)e.Name,      // Name 是 ownerId
                e => Guid.Parse(e.Value)        // value 是 unitId
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

        var stopwatch = Stopwatch.StartNew();
        var index = 0;
        void Log(string log)
        {
            index++;
            Logger.LogInformation($"{nameof(SetListByOwnerAsync)}({index})--[{stopwatch.ElapsedMilliseconds}ms]: {log}");
        }

        Log($"ownerId={ownerId},units:{units.Count()}");

        var unitList = units?.Where(x => x != null && x.OwnerId == ownerId).ToList() ?? [];

        var batch = Database.CreateBatch();

        var ownerExistsKey = OwnerExistsSetKey(ownerId);

        // clear existing set to avoid stale members
        //_ = batch.KeyDeleteAsync(ownerExistsKey);

        _ = batch.SetAddAsync(ownerExistsKey, Clock.Now.ToUnixTimeMilliseconds());
        _ = batch.KeyExpireAsync(ownerExistsKey, _cacheExpire);

        // owner zset unitKey
        var ownerSortedKey = OwnerSortedSetKey(ownerId);
        var toppingKey = OwnerToppingSetKey(ownerId);

        Log($"GetManyAsync Start");
        var unitMap = await GetManyAsync(unitList.Select(x => x.Id));
        Log($"GetManyAsync End");

        var cachedUnits = unitMap.Where(x => x.Value != null).Select(x => x.Value).ToList();
        Log($"cachedUnits:{cachedUnits.Count}");

        var unCachedUnits = unitList.ExceptBy(cachedUnits.Select(x => x.Id), x => x.Id).ToList();
        Log($"unCachedUnits:{unCachedUnits.Count}");

        foreach (var unit in unCachedUnits)
        {
            var unitId = unit.Id.ToString();

            var unitKey = UnitKey(unit.Id);

            var entries = MapToHashEntries(unit);

            _ = batch.HashSetAsync(unitKey, entries);
            _ = batch.KeyExpireAsync(unitKey, _cacheExpire);

            // score
            var score = GetScore(unit.Sorting, unit.Ticks);
            _ = batch.SortedSetAddAsync(ownerSortedKey, unitId, score);

            // set top
            if (unit.Sorting > 0)
            {
                _ = batch.SortedSetAddAsync(toppingKey, unitId, unit.Sorting);
                _ = batch.KeyExpireAsync(toppingKey, _cacheExpire);
            }
        }

        if (_cacheExpire.HasValue)
        {
            _ = batch.KeyExpireAsync(ownerSortedKey, _cacheExpire);
        }

        //合并
        var allList = cachedUnits.Concat(unCachedUnits);
        Log($"allList={allList.Count()}");

        // total
        long totalPublic = 0;
        long totalPrivate = 0;
        long totalRemindMe = 0;
        long totalRemindAll = 0;
        long totalFollowing = 0;

        foreach (var x in allList)
        {
            totalPublic += x.PublicBadge;
            totalPrivate += x.PrivateBadge;
            totalRemindMe += x.RemindMeCount;
            totalRemindAll += x.RemindAllCount;
            totalFollowing += x.FollowingCount;
        }

        var ownerBadgeKey = OwnerBadgeSetKey(ownerId);

        _ = batch.HashSetAsync(ownerBadgeKey, F_Total_Public, totalPublic);
        _ = batch.HashSetAsync(ownerBadgeKey, F_Total_Private, totalPrivate);
        _ = batch.HashSetAsync(ownerBadgeKey, F_Total_RemindMe, totalRemindMe);
        _ = batch.HashSetAsync(ownerBadgeKey, F_Total_RemindAll, totalRemindAll);
        _ = batch.HashSetAsync(ownerBadgeKey, F_Total_Following, totalFollowing);

        _ = batch.KeyExpireAsync(ownerBadgeKey, _cacheExpire);

        batch.Execute();

        Log($"batch.Execute()");

        return allList;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        ArgumentNullException.ThrowIfNull(fetchTask);
        var stopwatch = Stopwatch.StartNew();
        var index=0;
        void Log(string log)
        {
            index++;
            Logger.LogInformation($"{nameof(SetListByOwnerAsync)}({index})--[{stopwatch.ElapsedMilliseconds}ms]: {log}");
        }
        Log($"fetchUnits ownerId:{ownerId} Start");
        var units = (await fetchTask(ownerId))?.ToList() ?? [];
        Log($"fetchUnits ownerId:{ownerId} End, units:{units.Count}");
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

    public async Task<KeyValuePair<Guid, double>[]> GetSortedSetByOwnerAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var ownerSortedKey = OwnerSortedSetKey(ownerId);

        // read owner zset (may be empty) SortedSetRangeByScoreWithScoresAsync
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: ownerSortedKey,
            start: minScore,
            stop: maxScore,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);

        var result = redisZset.Select(x => new KeyValuePair<Guid, double>(Guid.Parse(x.Element), x.Score)).ToArray();

        return result;

    }

    public async Task<IQueryable<(Guid UnitId, double Sorting, double Ticks)>> GetSortedSetQueryableByOwnerAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var kvs = await GetSortedSetByOwnerAsync(ownerId, minScore, maxScore, skip, take, isDescending);

        var result = kvs.Select(x => (
               UnitId: x.Key,
               Sorting: Math.Floor(x.Value / OWNER_SCORE_MULT),
               Ticks: x.Value % OWNER_SCORE_MULT
           )).AsQueryable();

        return result;
    }

    public async Task<long> GetTotalCountByOwnerAsync(long ownerId)
    {
        var ownerSortedKey = OwnerSortedSetKey(ownerId);
        var totalCount = await Database.SortedSetLengthAsync(ownerSortedKey);
        return totalCount;
    }


    protected async Task<List<SortedSetEntry>> GetHistoryByOwnerInternalAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var ownerSortedKey = OwnerSortedSetKey(ownerId);

        if (maxScore == 0)
        {
            maxScore = double.PositiveInfinity;
        }

        var order = isDescending ? Order.Descending : Order.Ascending;
        // 1. 获取置顶的所有元素（不分页）
        var allPinned = await Database.SortedSetRangeByScoreWithScoresAsync(
            ownerSortedKey,
            start: OWNER_SCORE_MULT + minScore,
            stop: OWNER_SCORE_MULT + maxScore - 1,
            order: order
        );

        int pinnedCount = allPinned.Length;

        // 结果
        var result = new List<SortedSetEntry>();

        // 如果 skip 在置顶区内，则要从置顶区分页
        if (skip < pinnedCount)
        {
            // 决定从置顶区取多少
            int takeFromPinned = (int)(take == -1 ? int.MaxValue : take);

            var pinnedPart = allPinned
                .Skip((int)skip)
                .Take((int)takeFromPinned)
                .ToArray();

            result.AddRange(pinnedPart);

            // 如果置顶区已经提供足够数量，直接返回
            if (take != -1 && result.Count >= take)
            {
                return result;
            }

            // 需要从未置顶区补齐
            var remainingTake = take == -1 ? -1 : (take - result.Count);

            var nonPinned = await Database.SortedSetRangeByScoreWithScoresAsync(
                ownerSortedKey,
                start: minScore,
                stop: Math.Min(OWNER_SCORE_MULT, maxScore) - 1,
                skip: 0,
                take: remainingTake,
                order: order
            );

            result.AddRange(nonPinned);
            return result;
        }

        // skip 超出置顶区  只从未置顶区开始取
        var skipFromNonPinned = skip - pinnedCount;

        var nonPinnedOnly = await Database.SortedSetRangeByScoreWithScoresAsync(
            ownerSortedKey,
            start: minScore,
            stop: Math.Min(OWNER_SCORE_MULT, maxScore) - 1,
            skip: skipFromNonPinned,
            take: take,
            order: order
        );

        return nonPinnedOnly.ToList();
    }



    public async Task<IEnumerable<SessionUnitCacheItem>> GetListByOwnerAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var ownerSortedKey = OwnerSortedSetKey(ownerId);

        // read owner zset (may be empty)
        var kvs = await GetSortedSetByOwnerAsync(
            ownerId: ownerId,
            minScore: minScore,
            maxScore: maxScore,
            skip: skip,
            take: take,
            isDescending: true);

        // 按序取
        var unitIdList = kvs.Select(x => x.Key).ToList();

        var listMap = (await GetManyAsync(unitIdList)).ToDictionary(x => x.Key, x => x.Value);

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
            var ownerBadgeKey = OwnerBadgeSetKey(ownerId);
            var isSender = unitId == message.SenderSessionUnitId;

            // badge
            if (!isSender)
            {
                _ = batch.HashIncrementAsync(unitKey, isPrivate ? F_PrivateBadge : F_PublicBadge, 1);
                _ = batch.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerBadgeKey], [isPrivate ? F_Total_Private : F_Total_Public, 1]);

                //remindAllCount
                if (isRemindAll)
                {
                    _ = batch.HashIncrementAsync(unitKey, F_RemindAllCount, 1);
                    _ = batch.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerBadgeKey], [F_Total_RemindAll, 1]);
                }
                //remindMeCount
                if (reminderIds.Contains(unitId))
                {
                    _ = batch.HashIncrementAsync(unitKey, F_RemindMeCount, 1);
                    _ = batch.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerBadgeKey], [F_Total_RemindMe, 1]);
                }
                // followingCount
                if (followerIds.Contains(unitId))
                {
                    _ = batch.HashIncrementAsync(unitKey, F_FollowingCount, 1);
                    _ = batch.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerBadgeKey], [F_Total_Following, 1]);
                }
            }

            // lastMessageId
            _ = batch.HashSetAsync(unitKey, F_LastMessageId, lastMessageId);

            // ticks
            var ticks = new DateTimeOffset(message.CreationTime).ToUnixTimeMilliseconds();
            _ = batch.HashSetAsync(unitKey, F_Ticks, ticks);

            // owner sortedset tick: message.CreationTime.Ticks
            var score = GetScore(unit.Sorting, ticks);
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
        var ownerBadgeKey = OwnerBadgeSetKey(ownerId);
        var result = await Database.HashSetAsync(ownerBadgeKey, F_Total_Badge, badge);
        return result;
    }

    public virtual async Task<IDictionary<string, long>> GetTotalBadgeAsync(long ownerId)
    {
        var ownerBadgeKey = OwnerBadgeSetKey(ownerId);

        var entities = await Database.HashGetAllAsync(ownerBadgeKey);

        var dict = entities.ToDictionary(x => x.Name.ToString(), x => (long)x.Value);

        return dict;
    }

    public virtual async Task<bool> RemoveTotalBadgeAsync(long ownerId)
    {
        var ownerBadgeKey = OwnerBadgeSetKey(ownerId);
        return await Database.KeyDeleteAsync(ownerBadgeKey);
    }

    #endregion

    #region Remove / Set / Misc

    public async Task UpdateCountersync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask)
    {
        var unitKey = UnitKey(counter.Id);

        //
        var isExists = await Database.KeyExistsAsync(unitKey);

        if (!isExists)
        {
            // 加载缓存项
            var unit = await fetchTask(counter.Id);

            unit.PublicBadge = counter.PublicBadge;
            unit.PrivateBadge = counter.PrivateBadge;
            unit.RemindAllCount = counter.RemindAllCount;
            unit.RemindMeCount = counter.RemindMeCount;
            unit.FollowingCount = counter.FollowingCount;
            unit.Setting.ReadedMessageId = counter.ReadedMessageId;

            var entries = MapToHashEntries(unit);

            var batch = Database.CreateBatch();
            _ = batch.HashSetAsync(unitKey, entries);

            if (_cacheExpire.HasValue)
            {
                _ = batch.KeyExpireAsync(unitKey, _cacheExpire);
            }
            batch.Execute();

            Logger.LogInformation($"[{nameof(UpdateCountersync)}] 还未缓存，直接写入缓存:{counter}");
            return;
        }

        // -----------------------------
        // 2. 先读旧值（只读）
        // -----------------------------
        var readBatch = Database.CreateBatch();
        //old badge
        var taskPublicOld = readBatch.HashGetAsync(unitKey, F_PublicBadge);
        var taskPrivateOld = readBatch.HashGetAsync(unitKey, F_PrivateBadge);
        var tasRemindMeCountOld = readBatch.HashGetAsync(unitKey, F_RemindMeCount);
        var tasRemindAllCountOld = readBatch.HashGetAsync(unitKey, F_RemindAllCount);
        var tasFollowingCountOld = readBatch.HashGetAsync(unitKey, F_FollowingCount);
        readBatch.Execute();

        var oldPublic = await taskPublicOld;
        var oldPrivate = await taskPrivateOld;
        var oldRemindMeCount = await tasRemindMeCountOld;
        var oldRemindAllCount = await tasRemindAllCountOld;
        var oldFollowingCount = await tasFollowingCountOld;


        // 3. 写入新值（只写）
        // -----------------------------
        var writeBatch = Database.CreateBatch();

        _ = writeBatch.HashSetAsync(unitKey, F_PublicBadge, counter.PublicBadge);
        _ = writeBatch.HashSetAsync(unitKey, F_PrivateBadge, counter.PrivateBadge);
        _ = writeBatch.HashSetAsync(unitKey, F_RemindAllCount, counter.RemindAllCount);
        _ = writeBatch.HashSetAsync(unitKey, F_RemindMeCount, counter.RemindMeCount);
        _ = writeBatch.HashSetAsync(unitKey, F_FollowingCount, counter.FollowingCount);
        _ = writeBatch.HashSetAsync(unitKey, F_Setting_ReadedMessageId, counter.ReadedMessageId);

        if (_cacheExpire.HasValue)
        {
            _ = writeBatch.KeyExpireAsync(unitKey, _cacheExpire);
        }

        writeBatch.Execute();

        Logger.LogInformation($"[{nameof(UpdateCountersync)}] 已缓存，直接更新缓存:{counter}");

        // -----------------------------
        // 减量更新 Owner 总角标（key 必须已存在才更新）
        // -----------------------------
        var ownerBadgeKey = OwnerBadgeSetKey(counter.OwnerId);

        static long ToLong(RedisValue v) => v.HasValue && long.TryParse(v.ToString(), out var n) ? n : 0;

        var oldPublicBadge = ToLong(oldPublic);
        var oldPrivatePublic = ToLong(oldPrivate);
        var remindMeCount = ToLong(oldRemindMeCount);
        var remindAllCount = ToLong(oldRemindAllCount);
        var followingCount = ToLong(oldFollowingCount);

        async Task UpdateAsync(string field, RedisValue redisValue)
        {
            var val = ToLong(redisValue);

            if (val > 0)
            {
                Logger.LogInformation($"ownerId:{counter.OwnerId},需要减量 [{field}]:{val}");
                _ = await Database.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerBadgeKey], [field, -val]);
            }
        }

        await Task.WhenAll(
            UpdateAsync(F_Total_Public, oldPublicBadge),
            UpdateAsync(F_Total_Private, oldPrivatePublic),
            UpdateAsync(F_Total_RemindMe, oldRemindMeCount),
            UpdateAsync(F_Total_RemindAll, oldRemindAllCount),
            UpdateAsync(F_Total_Following, oldFollowingCount)
         );
    }

    public async Task SetToppingAsync(Guid unitId, long ownerId, long sorting)
    {
        var unitKey = UnitKey(unitId);

        // 1. 检查缓存项是否存在
        var isExists = await Database.KeyExistsAsync(unitKey);

        // 2. owner sortedset key
        var ownerSortedKey = OwnerSortedSetKey(ownerId);

        // 3. 先读取旧 score
        double? oldScore = await Database.SortedSetScoreAsync(ownerSortedKey, unitId.ToString());

        if (oldScore is null)
        {
            // 没有旧 score，说明该单元未加入 ownerSortedSet，不做置顶
            return;
        }

        // 4. 解析旧的 ticks
        var ticks = oldScore.Value % OWNER_SCORE_MULT;     // 低位 JS 毫秒
                                                           // 如果你担心 double 精度，可以加  Math.Round
        ticks = Math.Round(ticks);

        // 5. 新的 score = sorting * MULT + ticks
        var newScore = GetScore(sorting, ticks);

        // 6. 批处理写入
        var batch = Database.CreateBatch();

        // 6.1 更新 unitKey 中的 Sorting 字段（如果你有该字段）
        if (isExists)
        {
            _ = batch.HashSetAsync(unitKey, F_Sorting, sorting);
        }

        // 6.2 更新置顶表（ownerToppingSet）
        var toppingKey = OwnerToppingSetKey(ownerId);
        _ = batch.SortedSetAddAsync(toppingKey, unitId.ToString(), sorting);
        _ = batch.KeyExpireAsync(toppingKey, _cacheExpire);

        // 6.3 更新 ownerSortedSet 的新 score
        _ = batch.SortedSetAddAsync(ownerSortedKey, unitId.ToString(), newScore);
        _ = batch.KeyExpireAsync(ownerSortedKey, _cacheExpire);

        // 7. 执行 batch
        batch.Execute();
    }


    #endregion
}
