using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.RedisServices;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Logging;
using NUglify.Helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitCacheManager : RedisService, ISessionUnitCacheManager
{
    //protected readonly IDatabase Database = connection.GetDatabase();

    private readonly TimeSpan? _cacheExpire = TimeSpan.FromDays(7);

    protected string Prefix => $"{Options.Value.KeyPrefix}SessionUnits:";

    /// <summary>
    /// 会话单元详情信息
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    private string UnitKey(Guid unitId)
        => $"{Prefix}Units:UnitId-{unitId}";

    /// <summary>
    /// 会话单元集合
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private string SessionMembersSetKey(Guid sessionId)
        => $"{Prefix}Sessions:Members:SessionId-{sessionId}";

    /// <summary>
    /// 置顶的会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private string SessionPinnedSortingSetKey(Guid sessionId)
       => $"{Prefix}Sessions:PinnedSorting:SessionId-{sessionId}";

    /// <summary>
    /// 静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private string SessionImmersedSetKey(Guid sessionId)
       => $"{Prefix}Sessions:Immerseds:SessionId-{sessionId}";

    /// <summary>
    /// 好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerFriendsSetKey(long ownerId)
        => $"{Prefix}Owners:Friends:OwnerId-{ownerId}";

    /// <summary>
    /// 置顶的会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerPinnedBadgeSetKey(long ownerId)
       => $"{Prefix}Owners:PinnedBadge:OwnerId-{ownerId}";

    /// <summary>
    /// 静默会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerImmersedSetKey(long ownerId)
       => $"{Prefix}Owners:Immerseds:OwnerId-{ownerId}";

    /// <summary>
    /// 关注会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerFollowingSetKey(long ownerId)
       => $"{Prefix}Owners:Following:OwnerId-{ownerId}";

    /// <summary>
    /// @所有人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerRemindAllSetKey(long ownerId)
       => $"{Prefix}Owners:RemindAll:OwnerId-{ownerId}";

    /// <summary>
    /// @所有人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerRemindMeSetKey(long ownerId)
       => $"{Prefix}Owners:RemindMe:OwnerId-{ownerId}";

    /// <summary>
    /// 消息统计
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerStatisticSetKey(long ownerId)
        => $"{Prefix}Owners:Statistics:OwnerId-{ownerId}";

    /// <summary>
    /// 消息统计(分类)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string StatisticTypedSetKey(long ownerId)
        => $"{Prefix}Owners:StatisticTyped:OwnerId-{ownerId}";

    /// <summary>
    /// 1e16
    /// </summary>
    //public const long OWNER_SCORE_MULT = 10_000_000_000_000_000; // 1e16
    private static double GetFriendScore(double sorting, double ticks)
    {
        return FriendScore.Create(sorting, ticks);

        //var score = sorting * OWNER_SCORE_MULT + ticks;
        //return score;
    }

    #region Field names
    //private static string F_Id => nameof(SessionUnitCacheItem.Id);
    //private static string F_SessionId => nameof(SessionUnitCacheItem.SessionId);
    //private static string F_OwnerId => nameof(SessionUnitCacheItem.OwnerId);
    //private static string F_OwnerObjectType => nameof(SessionUnitCacheItem.OwnerObjectType);
    //private static string F_DestinationId => nameof(SessionUnitCacheItem.DestinationId);
    //private static string F_DestinationObjectType => nameof(SessionUnitCacheItem.DestinationObjectType);
    //private static string F_IsStatic => nameof(SessionUnitCacheItem.IsStatic);
    //private static string F_IsPublic => nameof(SessionUnitCacheItem.IsPublic);
    //private static string F_IsVisible => nameof(SessionUnitCacheItem.IsVisible);
    //private static string F_IsEnabled => nameof(SessionUnitCacheItem.IsEnabled);
    //private static string F_ReadedMessageId => nameof(SessionUnitCacheItem.ReadedMessageId);
    private static string F_LastMessageId => nameof(SessionUnitCacheItem.LastMessageId);
    private static string F_PublicBadge => nameof(SessionUnitCacheItem.PublicBadge);
    private static string F_PrivateBadge => nameof(SessionUnitCacheItem.PrivateBadge);
    private static string F_RemindAllCount => nameof(SessionUnitCacheItem.RemindAllCount);
    private static string F_RemindMeCount => nameof(SessionUnitCacheItem.RemindMeCount);
    private static string F_FollowingCount => nameof(SessionUnitCacheItem.FollowingCount);
    private static string F_Ticks => nameof(SessionUnitCacheItem.Ticks);
    private static string F_Sorting => nameof(SessionUnitCacheItem.Sorting);
    private static string F_Immersed => nameof(SessionUnitCacheItem.IsImmersed);

    //private static string F_Setting_ReadedMessageId => $"{nameof(SessionUnitCacheItem.Setting)}.{nameof(SessionUnitCacheItem.Setting.ReadedMessageId)}";

    // total
    //private static string F_Total_Badge => "TotalBadge";
    private static string F_Total_Public => nameof(SessionUnitStatistic.PublicBadge);
    private static string F_Total_Private => nameof(SessionUnitStatistic.PrivateBadge);
    private static string F_Total_Following => nameof(SessionUnitStatistic.Following);
    private static string F_Total_RemindMe => nameof(SessionUnitStatistic.RemindMe);
    private static string F_Total_RemindAll => nameof(SessionUnitStatistic.RemindAll);
    private static string F_Total_Immersed => nameof(SessionUnitStatistic.Immersed);
    private static string F_Total_Type => nameof(SessionUnitStatistic.Immersed);

    #endregion

    #region Initialize / Ensure helpers


    private static HashEntry[] MapToHashEntries(SessionUnitCacheItem unit)
    {
        return RedisMapper.ToHashEntries(unit);
    }

    private void SetOwnerPinning(IBatch batch, SessionUnitCacheItem unit)
    {
        if (unit.Sorting == 0)
        {
            return;
        }
        var ownerPinnedBadgeSetKey = OwnerPinnedBadgeSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(ownerPinnedBadgeSetKey, unit.Id.ToString(), unit.PublicBadge);
        _ = batch.KeyExpireAsync(ownerPinnedBadgeSetKey, _cacheExpire);
    }

    private void SetOwnerImmersed(IBatch batch, SessionUnitCacheItem unit)
    {
        if (!unit.IsImmersed)
        {
            return;
        }
        var immersedSetKey = OwnerImmersedSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(immersedSetKey, unit.Id.ToString(), unit.PublicBadge);
        _ = batch.KeyExpireAsync(immersedSetKey, _cacheExpire);
    }
    private void SetOwnerFollowing(IBatch batch, SessionUnitCacheItem unit)
    {
        if (unit.FollowingCount == 0)
        {
            return;
        }
        var followingSetKey = OwnerFollowingSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(followingSetKey, unit.Id.ToString(), unit.FollowingCount);
        _ = batch.KeyExpireAsync(followingSetKey, _cacheExpire);
    }
    private void SetOwnerRemindAll(IBatch batch, SessionUnitCacheItem unit)
    {
        if (unit.RemindAllCount == 0)
        {
            return;
        }
        var remindAllSetKey = OwnerRemindAllSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(remindAllSetKey, unit.Id.ToString(), unit.RemindAllCount);
        _ = batch.KeyExpireAsync(remindAllSetKey, _cacheExpire);
    }
    private void SetOwnerRemindMe(IBatch batch, SessionUnitCacheItem unit)
    {
        if (unit.RemindMeCount == 0)
        {
            return;
        }
        var remindMeSetKey = OwnerRemindMeSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(remindMeSetKey, unit.Id.ToString(), unit.RemindMeCount);
        _ = batch.KeyExpireAsync(remindMeSetKey, _cacheExpire);
    }

    private void SetSessionPinnedSorting(IBatch batch, SessionUnitCacheItem unit)
    {
        if (unit.Sorting == 0)
        {
            return;
        }
        var sessionPinnedSetKey = SessionPinnedSortingSetKey(unit.SessionId.Value);
        _ = batch.SortedSetAddAsync(sessionPinnedSetKey, unit.OwnerId.ToString(), unit.Sorting);
        _ = batch.KeyExpireAsync(sessionPinnedSetKey, _cacheExpire);
    }
    private void SetSessionImmersed(IBatch batch, SessionUnitCacheItem unit)
    {
        if (!unit.IsImmersed)
        {
            return;
        }
        var immersedSetKey = SessionImmersedSetKey(unit.SessionId.Value);
        _ = batch.SortedSetAddAsync(immersedSetKey, unit.OwnerId.ToString(), unit.PublicBadge);
        _ = batch.KeyExpireAsync(immersedSetKey, _cacheExpire);
    }

    private void SetUnit(IBatch batch, SessionUnitCacheItem unit, bool refreshExpire)
    {
        var unitKey = UnitKey(unit.Id);
        var entries = MapToHashEntries(unit);
        _ = batch.HashSetAsync(unitKey, entries);
        if (refreshExpire)
        {
            _ = batch.KeyExpireAsync(unitKey, _cacheExpire);
        }
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetMembersAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var stopwatch = Stopwatch.StartNew();

        var unitList = units.ToList();

        if (unitList.Count == 0) return [];

        var sessionMembersSetKey = SessionMembersSetKey(sessionId);

        //var lastMsgKey = LastMessageSetKey(sessionId);

        var unitKeys = unitList.Select(x => UnitKey(x.Id)).ToList();
        var unitKeysExists = await BatchKeyExistsAsync(unitKeys);
        var existsCount = unitKeysExists.Count(x => x.Value);

        Logger.LogInformation("sessionId={sessionId},unitKeysExists:{ExistsCount}",
            sessionId,
            existsCount);

        var batch = Database.CreateBatch();

        // clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(sessionMembersSetKey);

        foreach (var unit in unitList)
        {
            var element = new SessionUnitElement(sessionId, unit.OwnerId, unit.Id);
            var score = MemberScore.Create(unit.IsCreator, unit.CreationTime);
            _ = batch.SortedSetAddAsync(sessionMembersSetKey, element, score);

            var unitKey = UnitKey(unit.Id);
            var isExists = unitKeysExists.TryGetValue(unitKey, out var exists) && exists;

            if (!isExists)
            {
                SetUnit(batch, unit, refreshExpire: true);
            }

            // set session Topping
            SetSessionPinnedSorting(batch, unit);
            // set session Immersed
            SetSessionImmersed(batch, unit);

            //// set owner Topping
            //SetOwnerTopping(batch, unit);
            //// set owner Immersed
            //SetOwnerImmersed(batch, unit);
        }

        _ = batch.KeyExpireAsync(sessionMembersSetKey, _cacheExpire);

        batch.Execute();

        Logger.LogInformation(
           "[{Method}] sessionId={sessionId}, count:{Count},Exists:{ExistsCount} Elapsed:{Elapsed}ms",
           nameof(SetMembersAsync),
           sessionId,
           unitList.Count,
           existsCount,
           stopwatch.ElapsedMilliseconds);

        return units;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetMembersAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        ArgumentNullException.ThrowIfNull(fetchTask);
        var units = (await fetchTask(sessionId))?.ToList() ?? [];
        return await SetMembersAsync(sessionId, units);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetMembersIfNotExistsAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        if (!await Database.KeyExistsAsync(SessionMembersSetKey(sessionId)))
        {
            return await SetMembersAsync(sessionId, fetchTask);
        }
        return null;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetOrSetMembersAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        return await SetMembersIfNotExistsAsync(sessionId, fetchTask) ?? await GetMemberUnitsAsync(sessionId);
    }

    /// <summary>
    /// 获取会话成员映射
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:SessionUnitId, Value:OwnerId}</returns>
    public async Task<IDictionary<Guid, long>> GetMembersMapAsync(Guid sessionId)
    {
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(SessionMembersSetKey(sessionId));

        var dict = redisZset
            .Select(x => SessionUnitElement.Parse(x.Element))
            .ToDictionary(x => x.SessionUnitId, x => x.OwnerId);

        return dict;
    }

    /// <summary>
    /// 获取置顶会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:Sorting}</returns>
    public async Task<IDictionary<long, double>> GetPinnedMembersAsync(Guid sessionId)
    {
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(SessionPinnedSortingSetKey(sessionId));
        var dict = entries.ToDictionary(x => x.Element.ToLong(), x => x.Score);
        return dict;
    }

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:Immersed}</returns>
    public async Task<IDictionary<long, bool>> GetImmersedMembersAsync(Guid sessionId)
    {
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(SessionImmersedSetKey(sessionId));
        var dict = entries.ToDictionary(x => x.Element.ToLong(), x => x.Score == 1);
        return dict;
    }

    /// <summary>
    /// 获取会话单元数量
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<long> GetMembersCountAsync(Guid sessionId)
    {
        return await Database.SortedSetLengthAsync(SessionMembersSetKey(sessionId));
    }

    public async Task<IEnumerable<MemberModel>> GetMembersAsync(
        Guid sessionId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: SessionMembersSetKey(sessionId),
            start: minScore,
            stop: maxScore,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);
        return redisZset
           .Select(x =>
           {
               var element = SessionUnitElement.Parse(x.Element);
               var score = new MemberScore(x.Score);
               return new MemberModel
               {
                   SessionId = element.SessionId,
                   OwnerId = element.OwnerId,
                   Id = element.SessionUnitId,
                   CreationTime = score.CreationTime,
                   IsCreator = score.IsCreator
               };
           }); ;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetMemberUnitsAsync(
       Guid sessionId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var members = await GetMembersAsync(
             sessionId: sessionId,
             minScore: minScore,
             maxScore: maxScore,
             skip: skip,
             take: take,
             isDescending: isDescending);
        var unitIds = members.Select(x => x.Id).Distinct().ToList();
        var kvs = await GetManyAsync(unitIds);
        return kvs.Where(x => x.Value != null).Select(x => x.Value);
    }

    #endregion

    #region GetListByOwnerIdAsync (DB initial values + Redis merge)


    public async Task<IEnumerable<SessionUnitCacheItem>> SetFriendsAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units)
    {

        var stopwatch = Stopwatch.StartNew();
        var index = 0;
        void Log(string log)
        {
            index++;
            Logger.LogInformation($"{nameof(SetFriendsAsync)}({index})--[{stopwatch.ElapsedMilliseconds}ms]: {log}");
        }

        Log($"ownerId={ownerId},units:{units.Count()}");

        var unitList = units?.Where(x => x != null && x.OwnerId == ownerId).ToList() ?? [];

        var batch = Database.CreateBatch();

        var ownerStatisticSetKey = OwnerStatisticSetKey(ownerId);


        //clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(ownerStatisticSetKey);

        // owner zset unitKey
        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);
        //var toppingKey = OwnerToppingSetKey(ownerId);
        //var ownerImmersedSetKey = OwnerImmersedSetKey(ownerId);

        Log($"GetManyAsync Start");
        var unitMap = await GetManyAsync(unitList.Select(x => x.Id));
        Log($"GetManyAsync End");

        var cachedUnits = unitMap.Where(x => x.Value != null).Select(x => x.Value).ToList();
        Log($"cachedUnits:{cachedUnits.Count}");

        var unCachedUnits = unitList.ExceptBy(cachedUnits.Select(x => x.Id), x => x.Id).ToList();
        Log($"unCachedUnits:{unCachedUnits.Count}");

        foreach (var unit in unCachedUnits)
        {
            SetUnit(batch, unit, refreshExpire: false);
            // set Topping
            SetOwnerPinning(batch, unit);
            // set Immersed
            SetOwnerImmersed(batch, unit);
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
        long totalImmersed = 0;

        var countMap = new Dictionary<ChatObjectTypeEnums?, long>();

        Enum.GetValues<ChatObjectTypeEnums>().ForEach(x => countMap[x] = 0);

        foreach (var unit in allList)
        {
            var unitId = unit.Id.ToString();

            // score
            var element = new SessionUnitElement(unit.SessionId!.Value, unit.OwnerId, unit.Id);
            var score = GetFriendScore(unit.Sorting, unit.Ticks);
            _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, score);
            // 刷新所有UnitKey过期时间
            _ = batch.KeyExpireAsync(UnitKey(unit.Id), _cacheExpire);

            SetOwnerRemindMe(batch, unit);
            SetOwnerRemindAll(batch, unit);
            SetOwnerFollowing(batch, unit);
            //IsImmersed
            if (unit.IsImmersed)
            {
                totalImmersed += unit.PublicBadge;
            }
            else
            {
                //非静默统计PublicBadge
                countMap[unit.DestinationObjectType] += unit.PublicBadge;
                totalPublic += unit.PublicBadge;
            }
            totalPrivate += unit.PrivateBadge;
            totalRemindMe += unit.RemindMeCount;
            totalRemindAll += unit.RemindAllCount;
            totalFollowing += unit.FollowingCount;
        }

        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_Public, totalPublic);
        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_Private, totalPrivate);
        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_RemindMe, totalRemindMe);
        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_RemindAll, totalRemindAll);
        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_Following, totalFollowing);
        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_Immersed, totalImmersed);
        _ = batch.KeyExpireAsync(ownerStatisticSetKey, _cacheExpire);

        _ = batch.KeyExpireAsync(ownerFriendsSetKey, _cacheExpire);

        //destinations
        var statisticTypedSetKey = StatisticTypedSetKey(ownerId);
        foreach (var item in countMap)
        {
            _ = batch.HashSetAsync(statisticTypedSetKey, item.Key.ToString(), item.Value);
        }
        _ = batch.KeyExpireAsync(statisticTypedSetKey, _cacheExpire);

        batch.Execute();

        Log($"batch.Execute()");

        return allList;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetFriendsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        ArgumentNullException.ThrowIfNull(fetchTask);
        var stopwatch = Stopwatch.StartNew();
        var index = 0;
        void Log(string log)
        {
            index++;
            Logger.LogInformation($"{nameof(SetFriendsAsync)}({index})--[{stopwatch.ElapsedMilliseconds}ms]: {log}");
        }
        Log($"fetchUnits ownerId:{ownerId} Start");
        var units = (await fetchTask(ownerId))?.ToList() ?? [];
        Log($"fetchUnits ownerId:{ownerId} End, units:{units.Count}");
        return await SetFriendsAsync(ownerId, units);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetFriendsIfNotExistsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        if (!await Database.KeyExistsAsync(OwnerStatisticSetKey(ownerId)))
        {
            return await SetFriendsAsync(ownerId, fetchTask);
        }
        return null;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetOrSetFriendsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        return await SetFriendsIfNotExistsAsync(ownerId, fetchTask) ?? await GetFriendUnitsAsync(ownerId);
    }

    public async Task<IEnumerable<FriendModel>> GetFriendsAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);

        // read owner zset (may be empty) SortedSetRangeByScoreWithScoresAsync
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: ownerFriendsSetKey,
            start: minScore,
            stop: maxScore,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);

        //var result = redisZset.Select(x => new KeyValuePair<SessionUnitElement, SessionUnitScore>(SessionUnitElement.Parse(x.Element), new SessionUnitScore(x.Score))).ToArray();
        var result = redisZset.Select(x =>
        {
            var element = SessionUnitElement.Parse(x.Element);
            var score = new FriendScore(x.Score);
            return new FriendModel
            {
                OwnerId = element.OwnerId,//ownerId
                SessionId = element.SessionId,
                Id = element.SessionUnitId,
                Sorting = score.Sorting,
                Ticks = score.Ticks
            };
        });

        return result;

    }

    /// <summary>
    /// 获取好友会话单元数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<long> GetFirendsCountAsync(long ownerId)
    {
        return await Database.SortedSetLengthAsync(OwnerFriendsSetKey(ownerId));
    }


    protected async Task<List<SortedSetEntry>> GetHistoryByOwnerInternalAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);

        if (maxScore == 0)
        {
            maxScore = double.PositiveInfinity;
        }

        var order = isDescending ? Order.Descending : Order.Ascending;
        // 1. 获取置顶的所有元素（不分页）
        var allPinned = await Database.SortedSetRangeByScoreWithScoresAsync(
            ownerFriendsSetKey,
            start: FriendScore.Multiplier + minScore,
            stop: FriendScore.Multiplier + maxScore - 1,
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
                ownerFriendsSetKey,
                start: minScore,
                stop: Math.Min(FriendScore.Multiplier, maxScore) - 1,
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
            ownerFriendsSetKey,
            start: minScore,
            stop: Math.Min(FriendScore.Multiplier, maxScore) - 1,
            skip: skipFromNonPinned,
            take: take,
            order: order
        );

        return nonPinnedOnly.ToList();
    }



    public async Task<IEnumerable<SessionUnitCacheItem>> GetFriendUnitsAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);

        // read owner zset (may be empty)
        var kvs = await GetFriendsAsync(
            ownerId: ownerId,
            minScore: minScore,
            maxScore: maxScore,
            skip: skip,
            take: take,
            isDescending: true);

        // 按序取
        var unitIdList = kvs.Select(x => x.Id).ToList();

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


    public async Task<SessionUnitCacheItem> GetAsync(Guid id)
    {
        var units = await GetManyAsync([id]);

        return units[0].Value;
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
            SetUnit(batch, fetchedItem, refreshExpire: true);
            //var unitKey = UnitKey(nullKey);
            //var entries = MapToHashEntries(fetchedItem);
            //_ = batch.HashSetAsync(unitKey, entries);
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

        var stopwatch = Stopwatch.StartNew();

        var sessionId = message.SessionId!.Value;
        var lastMessageId = message.Id;

        var isPrivate = message.IsPrivateMessage();
        var isRemindAll = message.IsRemindAll;
        var reminderIds = message.MessageReminderList.Select(x => x.SessionUnitId).ToHashSet();
        var followerIds = message.MessageFollowerList.Select(x => x.SessionUnitId).ToHashSet();

        var receiverType = message.ReceiverType;

        var unitList = await GetMembersMapAsync(sessionId);
        //var unitList = await GetListBySessionAsync(sessionId);
        //var unitList = await SessionUnitManager.GetCacheListAsync(message);

        var pinnedMap = await GetPinnedMembersAsync(sessionId);
        var immersedMap = await GetImmersedMembersAsync(sessionId);

        //var umitMap = new Dictionary<long, (bool Immersed, double Sorting)>();

        Logger.LogInformation(
            "[{method}], sessionId:{sessionId}, lastMessageId:{lastMessageId}, count:{count},Elapsed:{ms}ms",
            nameof(GetMembersMapAsync),
            sessionId,
            lastMessageId,
            unitList.Count,
            stopwatch.ElapsedMilliseconds);

        if (unitList == null || !unitList.Any()) return;

        stopwatch.Restart();

        var batch = Database.CreateBatch();

        var expireTime = expire ?? _cacheExpire;

        foreach (var unit in unitList)
        {
            var unitId = unit.Key;
            var ownerId = unit.Value;
            //var unitId = unit.Id;
            //var ownerId = unit.OwnerId;
            var unitKey = UnitKey(unitId);
            var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);
            var ownerStatisticSetKey = OwnerStatisticSetKey(ownerId);
            var isSender = unitId == message.SenderSessionUnitId;
            //var (isImmersed, sorting) = umitMap.TryGetValue(ownerId, out var v) ? v : (Immersed: false, Sorting: 0);
            //var isImmersed = unit.IsImmersed;
            //var sorting = unit.Sorting;
            var isImmersed = immersedMap.TryGetValue(ownerId, out bool immersed) && immersed;
            var sorting = pinnedMap.TryGetValue(ownerId, out var _sorting) ? _sorting : 0;

            // lastMessageId
            _ = batch.HashSetAsync(unitKey, F_LastMessageId, lastMessageId);

            // ticks
            var ticks = new DateTimeOffset(message.CreationTime).ToUnixTimeMilliseconds();
            _ = batch.HashSetAsync(unitKey, F_Ticks, ticks);

            // owner sortedset tick: message.CreationTime.Ticks
            var element = new SessionUnitElement(sessionId, ownerId, unitId);
            var score = GetFriendScore(sorting, ticks);
            _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, score);

            // expire
            if (expireTime.HasValue)
            {
                _ = batch.KeyExpireAsync(unitKey, expireTime.Value);
                _ = batch.KeyExpireAsync(ownerFriendsSetKey, expireTime.Value);
            }

            // badge
            if (isSender)
            {
                continue;
            }

            // receiverType
            HashIncrementIfExist(batch, StatisticTypedSetKey(ownerId), receiverType.ToString(), 1);

            // badge
            _ = batch.HashIncrementAsync(unitKey, isPrivate ? F_PrivateBadge : F_PublicBadge, 1);

            // setTopping/pinned
            if (sorting > 0)
            {
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerPinnedBadgeSetKey(ownerId), unitId.ToString(), 1);
            }

            // 静默方式 IsImmersed
            if (isImmersed)
            {
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_Immersed, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerImmersedSetKey(ownerId), unitId.ToString(), 1);
            }
            else
            {
                HashIncrementIfExist(batch, ownerStatisticSetKey, isPrivate ? F_Total_Private : F_Total_Public, 1);
            }

            //remindAllCount
            if (isRemindAll)
            {
                _ = batch.HashIncrementAsync(unitKey, F_RemindAllCount, 1);
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_RemindAll, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerRemindAllSetKey(ownerId), unitId.ToString(), 1);
            }
            //remindMeCount
            if (reminderIds.Contains(unitId))
            {
                _ = batch.HashIncrementAsync(unitKey, F_RemindMeCount, 1);
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_RemindMe, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerRemindMeSetKey(ownerId), unitId.ToString(), 1);
            }
            // followingCount
            if (followerIds.Contains(unitId))
            {
                _ = batch.HashIncrementAsync(unitKey, F_FollowingCount, 1);
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_Following, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerFollowingSetKey(ownerId), unitId.ToString(), 1);
            }
        }

        if (expireTime.HasValue)
        {
            _ = batch.KeyExpireAsync(SessionMembersSetKey(sessionId), expireTime.Value);
        }

        batch.Execute();

        Logger.LogInformation("[{method}] executed, lastMessageId:{lastMessageId}, Elapsed:{Elapsed}ms",
            nameof(BatchIncrementAsync),
            lastMessageId,
            stopwatch.ElapsedMilliseconds);
    }

    private static SessionUnitStatistic MapToStatistic(HashEntry[] entries)
    {
        return RedisMapper.ToObject<SessionUnitStatistic>(entries);

    }

    public virtual async Task<SessionUnitStatistic> GetStatisticAsync(long ownerId)
    {
        var entries = await Database.HashGetAllAsync(OwnerStatisticSetKey(ownerId));
        return MapToStatistic(entries);
    }

    public virtual async Task<bool> RemoveStatisticAsync(long ownerId)
    {
        return await Database.KeyDeleteAsync(OwnerStatisticSetKey(ownerId));
    }

    public virtual async Task<Dictionary<string, long>> GetRawBadgeMapAsync(long ownerId)
    {
        var entries = await Database.HashGetAllAsync(StatisticTypedSetKey(ownerId));
        var result = entries.ToDictionary(
            x => x.Name.ToString(),
            x => long.TryParse(x.Value.ToString(), out var v) ? v : 0);

        return result;
    }
    public async Task<Dictionary<ChatObjectTypeEnums, long>> GetBadgeMapAsync(long ownerId)
    {
        var raw = await GetRawBadgeMapAsync(ownerId);

        var map = new Dictionary<ChatObjectTypeEnums, long>();

        foreach (var (key, value) in raw)
        {
            if (!Enum.TryParse<ChatObjectTypeEnums>(key, true, out var type))
            {
                continue;
            }

            map[type] = value;
        }

        // 补齐所有枚举
        foreach (var type in Enum.GetValues<ChatObjectTypeEnums>())
        {
            map.TryAdd(type, 0);
        }

        return map;
    }

    #endregion

    #region Remove / Set / Misc
    private async Task UpdateCounter_BackAsync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask)
    {
        var unitKey = UnitKey(counter.Id);
        SessionUnitCacheItem unit;
        //
        var isExists = await Database.KeyExistsAsync(unitKey);

        if (!isExists)
        {
            // 加载缓存项
            unit = await fetchTask(counter.Id);

            unit.PublicBadge = counter.PublicBadge;
            unit.PrivateBadge = counter.PrivateBadge;
            unit.RemindAllCount = counter.RemindAllCount;
            unit.RemindMeCount = counter.RemindMeCount;
            unit.FollowingCount = counter.FollowingCount;
            //unit.Setting.ReadedMessageId = counter.ReadedMessageId;

            var batch = Database.CreateBatch();
            SetUnit(batch, unit, refreshExpire: true);
            batch.Execute();

            Logger.LogInformation($"[{nameof(UpdateCounter_BackAsync)}] 还未缓存，直接写入缓存:{counter}");
            return;
        }

        // -----------------------------
        // 2. 先读旧值（只读）
        // -----------------------------
        var readBatch = Database.CreateBatch();
        //old badge
        var taskPublicOld = readBatch.HashGetAsync(unitKey, F_PublicBadge);
        var taskPrivateOld = readBatch.HashGetAsync(unitKey, F_PrivateBadge);
        var taskRemindMeCountOld = readBatch.HashGetAsync(unitKey, F_RemindMeCount);
        var taskRemindAllCountOld = readBatch.HashGetAsync(unitKey, F_RemindAllCount);
        var taskFollowingCountOld = readBatch.HashGetAsync(unitKey, F_FollowingCount);
        var taskImmersedOld = readBatch.HashGetAsync(unitKey, F_Immersed);
        readBatch.Execute();

        var oldPublic = await taskPublicOld;
        var oldPrivate = await taskPrivateOld;
        var oldRemindMeCount = await taskRemindMeCountOld;
        var oldRemindAllCount = await taskRemindAllCountOld;
        var oldFollowingCount = await taskFollowingCountOld;
        var oldImmersed = await taskImmersedOld;

        // 3. 写入新值（只写）
        // -----------------------------
        var writeBatch = Database.CreateBatch();

        _ = writeBatch.HashSetAsync(unitKey, F_PublicBadge, counter.PublicBadge);
        _ = writeBatch.HashSetAsync(unitKey, F_PrivateBadge, counter.PrivateBadge);
        _ = writeBatch.HashSetAsync(unitKey, F_RemindAllCount, counter.RemindAllCount);
        _ = writeBatch.HashSetAsync(unitKey, F_RemindMeCount, counter.RemindMeCount);
        _ = writeBatch.HashSetAsync(unitKey, F_FollowingCount, counter.FollowingCount);
        //_ = writeBatch.HashSetAsync(unitKey, F_Setting_ReadedMessageId, counter.ReadedMessageId);

        if (_cacheExpire.HasValue)
        {
            _ = writeBatch.KeyExpireAsync(unitKey, _cacheExpire);
        }

        writeBatch.Execute();

        Logger.LogInformation($"[{nameof(UpdateCounter_BackAsync)}] 已缓存，直接更新缓存:{counter}");

        // -----------------------------
        // 减量更新 Owner 总角标（key 必须已存在才更新）
        // -----------------------------
        var ownerStatisticSetKey = OwnerStatisticSetKey(counter.OwnerId);

        static long ToLong(RedisValue v) => v.HasValue && long.TryParse(v.ToString(), out var n) ? n : 0;
        static bool ToBool(RedisValue v) => v.HasValue && bool.TryParse(v.ToString(), out var n) && n;

        var oldPublicBadge = ToLong(oldPublic);
        var oldPrivatePublic = ToLong(oldPrivate);
        var remindMeCount = ToLong(oldRemindMeCount);
        var remindAllCount = ToLong(oldRemindAllCount);
        var followingCount = ToLong(oldFollowingCount);
        //静默方式
        var immersed = ToBool(oldImmersed);

        async Task UpdateAsync(string field, RedisValue redisValue)
        {
            var val = ToLong(redisValue);

            if (val > 0)
            {
                Logger.LogInformation($"ownerId:{counter.OwnerId},需要减量 [{field}]:{val}");
                _ = await HashIncrementIfExistsAsync(ownerStatisticSetKey, field, -val);
            }
        }


        await Task.WhenAll(
            UpdateAsync(immersed ? F_Total_Immersed : F_Total_Public, oldPublicBadge),
            UpdateAsync(F_Total_Private, oldPrivatePublic),
            UpdateAsync(F_Total_RemindMe, oldRemindMeCount),
            UpdateAsync(F_Total_RemindAll, oldRemindAllCount),
            UpdateAsync(F_Total_Following, oldFollowingCount)
         );
    }



    private void UpdateUnit(IBatch batch, SessionUnitCacheItem unit, SessionUnitCounterInfo counter)
    {
        unit.PublicBadge = counter.PublicBadge;
        unit.PrivateBadge = counter.PrivateBadge;
        unit.RemindAllCount = counter.RemindAllCount;
        unit.RemindMeCount = counter.RemindMeCount;
        unit.FollowingCount = counter.FollowingCount;

        SetUnit(batch, unit, refreshExpire: true);

        ZsetUpdateIfExistsAsync(batch, OwnerFollowingSetKey(unit.OwnerId), unit.Id.ToString(), counter.FollowingCount);
        ZsetUpdateIfExistsAsync(batch, OwnerRemindAllSetKey(unit.OwnerId), unit.Id.ToString(), counter.RemindAllCount, removeWhenZero: true);
        ZsetUpdateIfExistsAsync(batch, OwnerRemindMeSetKey(unit.OwnerId), unit.Id.ToString(), counter.RemindMeCount, removeWhenZero: true);
        ZsetUpdateIfExistsAsync(batch, OwnerImmersedSetKey(unit.OwnerId), unit.Id.ToString(), counter.PublicBadge);
        ZsetUpdateIfExistsAsync(batch, OwnerPinnedBadgeSetKey(unit.OwnerId), unit.Id.ToString(), counter.PublicBadge);
    }
    public async Task UpdateCounterAsync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask)
    {
        var unit = await GetAsync(counter.Id);

        var batch = Database.CreateBatch();

        if (unit == null)
        {
            unit = await fetchTask(counter.Id);
            // 加载缓存项
            UpdateUnit(batch, unit, counter);
            Logger.LogInformation("[{method}] 还未缓存，直接写入缓存:{counter}", nameof(UpdateCounterAsync), counter);
            batch.Execute();
            return;
        }

        // -----------------------------
        // 2. 先读旧值（只读）
        // -----------------------------

        var publicBadge = unit.PublicBadge;
        var privatePublic = unit.PublicBadge;
        var remindMeCount = unit.RemindMeCount;
        var remindAllCount = unit.RemindAllCount;
        var followingCount = unit.FollowingCount;

        //静默方式
        var immersed = unit.IsImmersed;

        // 3. 写入新值（只写）
        // -----------------------------

        UpdateUnit(batch, unit, counter);
        Logger.LogInformation("[{method}] 已缓存，直接更新缓存:{counter}", nameof(UpdateCounterAsync), counter);

        if (!immersed)
        {
            // //非静默减量 destinations （key 必须已存在才更新）
            HashIncrementIfExist(batch, StatisticTypedSetKey(unit.OwnerId), unit.DestinationObjectType.ToString(), -publicBadge);
        }

        // 减量更新 Owner 总角标（key 必须已存在才更新）
        var ownerStatisticSetKey = OwnerStatisticSetKey(counter.OwnerId);

        void UpdateStatisticAsync(string field, int val)
        {
            if (val > 0)
            {
                Logger.LogInformation($"ownerId:{counter.OwnerId},需要减量 [{field}]:{val}");
                HashIncrementIfExist(batch, ownerStatisticSetKey, field, -val);
            }
        }

        UpdateStatisticAsync(immersed ? F_Total_Immersed : F_Total_Public, publicBadge);
        UpdateStatisticAsync(F_Total_Private, privatePublic);
        UpdateStatisticAsync(F_Total_RemindMe, remindMeCount);
        UpdateStatisticAsync(F_Total_RemindAll, remindAllCount);
        UpdateStatisticAsync(F_Total_Following, followingCount);


        batch.Execute();
    }

    public async Task SetPinningAsync(Guid sessionId, Guid unitId, long ownerId, long sorting)
    {
        var unitKey = UnitKey(unitId);

        var unit = await GetAsync(unitId);

        // 1. 检查缓存项是否存在
        var isExists = unit != null;

        // 2. owner sortedset key
        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);

        // 3. 先读取旧 score
        var element = new SessionUnitElement(sessionId, ownerId, unitId);
        double? oldScore = await Database.SortedSetScoreAsync(ownerFriendsSetKey, element);

        if (oldScore is null)
        {
            // 没有旧 score，说明该单元未加入 ownerSortedSet，不做置顶
            return;
        }

        // 4. 解析旧的 ticks
        var scoreObj = new FriendScore(oldScore.Value);

        var ticks = scoreObj.Ticks;     // 低位 JS 毫秒
                                        // 如果你担心 double 精度，可以加  Math.Round
        ticks = Math.Round(ticks);

        // 5. 新的 score = sorting * MULT + ticks
        var newScore = GetFriendScore(sorting, ticks);

        // 6. 批处理写入
        var batch = Database.CreateBatch();

        // 6.1 更新 unitKey 中的 Sorting 字段（如果你有该字段）
        if (isExists)
        {
            _ = batch.HashSetAsync(unitKey, F_Sorting, sorting);
        }

        // 6.2 更新置顶表（ownerToppingSet）
        var ownerPinnedBadgeSetKey = OwnerPinnedBadgeSetKey(ownerId);
        var sessionPinnedSortingSetKey = SessionPinnedSortingSetKey(sessionId);
        if (sorting == 0)
        {
            _ = batch.SortedSetRemoveAsync(ownerPinnedBadgeSetKey, unitId.ToString());
            _ = batch.SortedSetRemoveAsync(sessionPinnedSortingSetKey, unitId.ToString());
        }
        else
        {
            _ = batch.SortedSetAddAsync(ownerPinnedBadgeSetKey, unitId.ToString(), unit?.PublicBadge ?? 0);
            _ = batch.SortedSetAddAsync(sessionPinnedSortingSetKey, unitId.ToString(), sorting);
            _ = batch.KeyExpireAsync(ownerPinnedBadgeSetKey, _cacheExpire);
            _ = batch.KeyExpireAsync(sessionPinnedSortingSetKey, _cacheExpire);
        }

        // 6.3 更新 ownerSortedSet 的新 score
        _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, newScore);
        _ = batch.KeyExpireAsync(ownerFriendsSetKey, _cacheExpire);

        // 7. 执行 batch
        batch.Execute();
    }

    public async Task ChangeImmersedAsync(Guid unitId, bool isImmersed)
    {
        var unit = await GetAsync(unitId);

        if (unit == null)
        {
            Logger.LogWarning("unit is null,unitId:{unitId}", unitId);
            return;
        }

        if (unit.IsImmersed == isImmersed)
        {
            Logger.LogWarning("Unchanged unitId:{unitId},isImmersed={isImmersed}", unitId, isImmersed);
            return;
        }

        var tran = Database.CreateTransaction();

        if (unit.PublicBadge > 0)
        {
            var ownerStatisticSetKey = OwnerStatisticSetKey(unit.OwnerId);
            var publicDelta = isImmersed ? -unit.PublicBadge : unit.PublicBadge;
            var immersedDelta = -publicDelta;

            //_ = tran.HashIncrementAsync(ownerStatisticSetKey, F_Total_Public, publicDelta);
            //_ = tran.HashIncrementAsync(ownerStatisticSetKey, F_Total_Immersed, immersedDelta);
            //_ = tran.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerStatisticSetKey], [F_Total_Public, publicDelta]);
            //_ = tran.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerStatisticSetKey], [F_Total_Immersed, immersedDelta]);

            HashIncrementIfExist(tran, ownerStatisticSetKey, F_Total_Public, publicDelta);
            HashIncrementIfExist(tran, ownerStatisticSetKey, F_Total_Public, immersedDelta);

            //destinations
            //_ = tran.ScriptEvaluateAsync(IncrementIfExistsScript, [StatisticTypedSetKey(unit.OwnerId)], [unit.DestinationObjectType.ToString(), publicDelta]);
            HashIncrementIfExist(tran, StatisticTypedSetKey(unit.OwnerId), unit.DestinationObjectType.ToString(), publicDelta);
        }
        else
        {
            Logger.LogWarning("PublicBadge: 0 ,unitId:{unitId}", unitId);
        }

        _ = tran.HashSetAsync(UnitKey(unitId), F_Immersed, isImmersed);


        var ownerImmersedSetKey = OwnerImmersedSetKey(unit.OwnerId);
        var sessionImmersedSetKey = SessionImmersedSetKey(unit.SessionId!.Value);
        if (isImmersed)
        {
            _ = tran.SortedSetAddAsync(ownerImmersedSetKey, unitId.ToString(), unit.PublicBadge);
            _ = tran.KeyExpireAsync(ownerImmersedSetKey, _cacheExpire);

            _ = tran.SortedSetAddAsync(sessionImmersedSetKey, unitId.ToString(), unit.PublicBadge);
            _ = tran.KeyExpireAsync(sessionImmersedSetKey, _cacheExpire);
        }
        else
        {
            _ = tran.SortedSetRemoveAsync(ownerImmersedSetKey, unitId.ToString());
            _ = tran.SortedSetRemoveAsync(sessionImmersedSetKey, unitId.ToString());
        }

        var committed = await tran.ExecuteAsync();

        if (!committed)
        {
            Logger.LogWarning("Redis transaction failed, unitId:{unitId}", unitId);
            return;
        }

        unit.IsImmersed = isImmersed;
    }

    public async Task UnfollowAsync(long ownerId, List<Guid> unitIdList)
    {
        var tran = Database.CreateTransaction();

        foreach (var unitId in unitIdList)
        {
            _ = tran.SortedSetRemoveAsync(OwnerFollowingSetKey(ownerId), unitId.ToString());
        }
        var committed = await tran.ExecuteAsync();

        if (!committed)
        {
            Logger.LogWarning("Redis transaction failed, ownerId:{ownerId}, unitIdList:{unitIdList}",
                ownerId,
                unitIdList.JoinAsString(","));
            return;
        }
    }

    public async Task FollowAsync(long ownerId, List<Guid> unitIdList)
    {
        var tran = Database.CreateTransaction();

        //foreach (var unitId in unitIdList)
        //{
        //    _ = tran.SortedSetRemoveAsync(OwnerFollowingSetKey(ownerId), unitId.ToString());
        //}
        var committed = await tran.ExecuteAsync();
    }

    #endregion
}
