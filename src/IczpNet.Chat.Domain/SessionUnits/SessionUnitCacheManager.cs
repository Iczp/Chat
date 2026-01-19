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
using System.Xml.Linq;
using Volo.Abp;

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
    private RedisKey UnitHashKey(Guid unitId) => $"{Prefix}Units:UnitId-{unitId}";

    /// <summary>
    /// 会话单元集合
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionMembersSetKey(Guid sessionId) => $"{Prefix}Sessions:Members:SessionId-{sessionId}";

    /// <summary>
    /// 置顶的会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionPinnedSortingSetKey(Guid sessionId) => $"{Prefix}Sessions:PinnedSorting:SessionId-{sessionId}";

    /// <summary>
    /// 静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionImmersedSetKey(Guid sessionId) => $"{Prefix}Sessions:Immersed:SessionId-{sessionId}";

    /// <summary>
    /// 创建人
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionCreatorSetKey(Guid sessionId) => $"{Prefix}Sessions:Creator:SessionId-{sessionId}";

    /// <summary>
    /// 好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerFriendsSetKey(long ownerId) => $"{Prefix}Owners:Friends:OwnerId-{ownerId}";

    /// <summary>
    /// 好友分类
    /// </summary>
    /// <param name="friendType"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerFriendsMapZsetKey(long ownerId, ChatObjectTypeEnums? friendType) => $"{Prefix}Owners:FriendsMap:{friendType}:OwnerId-{ownerId}";

    /// <summary>
    /// 置顶的会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerPinnedBadgeSetKey(long ownerId) => $"{Prefix}Owners:PinnedBadge:OwnerId-{ownerId}";

    /// <summary>
    /// 有未读消息的会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerHasBadgeSetKey(long ownerId) => $"{Prefix}Owners:HasBadge:OwnerId-{ownerId}";

    /// <summary>
    /// 静默会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerImmersedSetKey(long ownerId) => $"{Prefix}Owners:Immersed:OwnerId-{ownerId}";

    /// <summary>
    /// 关注会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerFollowingSetKey(long ownerId) => $"{Prefix}Owners:Following:OwnerId-{ownerId}";

    /// <summary>
    /// @所有人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerRemindAllSetKey(long ownerId) => $"{Prefix}Owners:RemindAll:OwnerId-{ownerId}";

    /// <summary>
    /// @所有人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerRemindMeSetKey(long ownerId) => $"{Prefix}Owners:RemindMe:OwnerId-{ownerId}";

    /// <summary>
    /// 创建人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerCreatorSetKey(long ownerId) => $"{Prefix}Owners:Creator:OwnerId-{ownerId}";

    /// <summary>
    /// 消息统计
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerStatisticHashKey(long ownerId) => $"{Prefix}Owners:Statistic:OwnerId-{ownerId}";

    /// <summary>
    /// 消息统计(分类)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey StatisticMapHashKey(long ownerId) => $"{Prefix}Owners:StatisticMap:OwnerId-{ownerId}";



    private static SessionUnitElement GetElement(SessionUnitCacheItem unit)
    {
        return SessionUnitElement.Create(unit.OwnerId, unit.DestinationId.GetValueOrDefault(), unit.Id, unit.SessionId.GetValueOrDefault());
    }
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
    private static string F_Total_Pinned => nameof(SessionUnitStatistic.Pinned);

    #endregion

    #region Initialize / Ensure helpers


    private static HashEntry[] MapToHashEntries(SessionUnitCacheItem unit)
    {
        return RedisMapper.ToHashEntries(unit);
    }

    private void SetOwnerPinning(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (unit.Sorting == 0)
        {
            return;
        }
        var ownerPinnedBadgeSetKey = OwnerPinnedBadgeSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(ownerPinnedBadgeSetKey, element, unit.PublicBadge);
        _ = batch.KeyExpireAsync(ownerPinnedBadgeSetKey, _cacheExpire);
    }
    private void SetOwnerHasBadge(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (unit.PublicBadge == 0)
        {
            return;
        }
        var ownerHasBadgeSetKey = OwnerHasBadgeSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(ownerHasBadgeSetKey, element, unit.PublicBadge);
        _ = batch.KeyExpireAsync(ownerHasBadgeSetKey, _cacheExpire);
    }

    private void SetOwnerImmersed(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (!unit.IsImmersed)
        {
            return;
        }
        var immersedSetKey = OwnerImmersedSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(immersedSetKey, element, unit.PublicBadge);
        _ = batch.KeyExpireAsync(immersedSetKey, _cacheExpire);
    }
    private void SetOwnerFollowing(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (unit.FollowingCount == 0)
        {
            return;
        }
        var followingSetKey = OwnerFollowingSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(followingSetKey, element, unit.FollowingCount);
        _ = batch.KeyExpireAsync(followingSetKey, _cacheExpire);
    }


    private void SetOwnerRemindAll(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (unit.RemindAllCount == 0)
        {
            return;
        }
        var remindAllSetKey = OwnerRemindAllSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(remindAllSetKey, element, unit.RemindAllCount);
        _ = batch.KeyExpireAsync(remindAllSetKey, _cacheExpire);
    }
    private void SetOwnerRemindMe(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (unit.RemindMeCount == 0)
        {
            return;
        }
        var remindMeSetKey = OwnerRemindMeSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(remindMeSetKey, element, unit.RemindMeCount);
        _ = batch.KeyExpireAsync(remindMeSetKey, _cacheExpire);
    }
    private void SetOwnerCreator(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (!unit.IsCreator)
        {
            return;
        }
        var creatorSetKey = OwnerCreatorSetKey(unit.OwnerId);
        _ = batch.SortedSetAddAsync(creatorSetKey, element, unit.PublicBadge);
        _ = batch.KeyExpireAsync(creatorSetKey, _cacheExpire);
    }

    private void SetOwnerFriendMap(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit, double score)
    {
        var friendsMapZsetKey = OwnerFriendsMapZsetKey(unit.OwnerId, unit.DestinationObjectType);
        _ = batch.SortedSetAddAsync(friendsMapZsetKey, element, score);
        _ = batch.KeyExpireAsync(friendsMapZsetKey, _cacheExpire);
    }

    private void SetSessionPinnedSorting(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (unit.Sorting == 0)
        {
            return;
        }
        var sessionPinnedSetKey = SessionPinnedSortingSetKey(unit.SessionId.Value);
        _ = batch.SortedSetAddAsync(sessionPinnedSetKey, element, unit.Sorting);
        _ = batch.KeyExpireAsync(sessionPinnedSetKey, _cacheExpire);
    }
    private void SetSessionImmersed(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (!unit.IsImmersed)
        {
            return;
        }
        var immersedSetKey = SessionImmersedSetKey(unit.SessionId.Value);
        _ = batch.SortedSetAddAsync(immersedSetKey, element, unit.PublicBadge);
        _ = batch.KeyExpireAsync(immersedSetKey, _cacheExpire);
    }
    private void SetSessionCreator(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
    {
        if (!unit.IsCreator)
        {
            return;
        }
        var creatorSetKey = SessionCreatorSetKey(unit.SessionId.Value);
        _ = batch.SortedSetAddAsync(creatorSetKey, element, unit.PublicBadge);
        _ = batch.KeyExpireAsync(creatorSetKey, _cacheExpire);
    }

    private void SetUnit(IBatch batch, SessionUnitCacheItem unit, bool refreshExpire)
    {
        var unitKey = UnitHashKey(unit.Id);
        var entries = MapToHashEntries(unit);
        _ = batch.HashSetAsync(unitKey, entries);
        if (refreshExpire)
        {
            _ = batch.KeyExpireAsync(unitKey, _cacheExpire);
        }
    }

    private string GetFriendTypeKey(FriendViews friendView, long ownerId)
    {
        return friendView switch
        {
            FriendViews.All => OwnerFriendsSetKey(ownerId),
            FriendViews.HasBadge => OwnerHasBadgeSetKey(ownerId),
            FriendViews.Pinned => OwnerPinnedBadgeSetKey(ownerId),
            FriendViews.Following => OwnerFollowingSetKey(ownerId),
            FriendViews.RemindAll => OwnerRemindAllSetKey(ownerId),
            FriendViews.RemindMe => OwnerRemindMeSetKey(ownerId),
            FriendViews.Immersed => OwnerImmersedSetKey(ownerId),
            FriendViews.Creator => OwnerCreatorSetKey(ownerId),
            _ => throw new UserFriendlyException($"Non handle FriendTypes:{friendView}"),
        };
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetMembersAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var stopwatch = Stopwatch.StartNew();

        var unitList = units.ToList();

        if (unitList.Count == 0) return [];

        var sessionMembersSetKey = SessionMembersSetKey(sessionId);

        //var lastMsgKey = LastMessageSetKey(sessionId);

        var unitKeys = unitList.Select(x => UnitHashKey(x.Id)).ToList();
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
            var element = GetElement(unit);
            var score = MemberScore.Create(unit.IsCreator, unit.CreationTime);
            _ = batch.SortedSetAddAsync(sessionMembersSetKey, element, score);

            var unitKey = UnitHashKey(unit.Id);
            var isExists = unitKeysExists.TryGetValue(unitKey, out var exists) && exists;

            if (!isExists)
            {
                SetUnit(batch, unit, refreshExpire: true);
            }

            // set session Topping
            SetSessionPinnedSorting(batch, element, unit);
            // set session Immersed
            SetSessionImmersed(batch, element, unit);
            // set sessuib Creator
            SetSessionCreator(batch, element, unit);

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
    public async Task<IDictionary<Guid, SessionUnitElement>> GetMembersMapAsync(Guid sessionId)
    {
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(SessionMembersSetKey(sessionId));
        var dict = redisZset
            .Select(x => SessionUnitElement.Parse(x.Element))
            .ToDictionary(x => x.SessionUnitId);
        return dict;
    }


    /// <summary>
    /// 获取置顶会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:Sorting}</returns>
    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, double>>> GetPinnedMembersAsync(Guid sessionId)
    {
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(SessionPinnedSortingSetKey(sessionId));
        var dict = entries.Select(x => new KeyValuePair<SessionUnitElement, double>(SessionUnitElement.Parse(x.Element), x.Score));
        return dict;
    }

    /// <summary>
    /// 获取置顶会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:Sorting}</returns>
    public async Task<IDictionary<long, double>> GetPinnedMembersMapAsync(Guid sessionId)
    {
        var items = await GetPinnedMembersAsync(sessionId);
        var dict = items.ToDictionary(x => x.Key.OwnerId, x => x.Value);
        return dict;
    }

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:PublicBadge}</returns>
    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, double>>> GetImmersedMembersAsync(Guid sessionId)
    {
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(SessionImmersedSetKey(sessionId));
        var dict = entries.Select(x => new KeyValuePair<SessionUnitElement, double>(SessionUnitElement.Parse(x.Element), x.Score));
        return dict;
    }

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:Immersed}</returns>
    public async Task<IDictionary<long, bool>> GetImmersedMembersMapAsync(Guid sessionId)
    {
        var entries = await GetImmersedMembersAsync(sessionId);
        var dict = entries.ToDictionary(x => x.Key.OwnerId, x => x.Value == 1);
        return dict;
    }

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, double>>> GetCreatorMembersAsync(Guid sessionId)
    {
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(SessionCreatorSetKey(sessionId));
        var dict = entries.Select(x => new KeyValuePair<SessionUnitElement, double>(SessionUnitElement.Parse(x.Element), x.Score));
        return dict;
    }

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:Immersed}</returns>
    public async Task<IDictionary<long, bool>> GetCreatorMembersMapAsync(Guid sessionId)
    {
        var items = await GetCreatorMembersAsync(sessionId);
        var dict = items.ToDictionary(x => x.Key.OwnerId, x => x.Value == 1);
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
                   FriendId = element.FriendId,
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

        var ownerStatisticSetKey = OwnerStatisticHashKey(ownerId);


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
            var element = GetElement(unit);
            // set Topping
            SetOwnerPinning(batch, element, unit);
            // set Immersed
            SetOwnerImmersed(batch, element, unit);
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
        long totalPinned = 0;

        var countMap = new Dictionary<ChatObjectTypeEnums?, long>();

        Enum.GetValues<ChatObjectTypeEnums>().ForEach(x => countMap[x] = 0);

        foreach (var unit in allList)
        {
            var unitId = unit.Id.ToString();

            // SetOwnerFriend()
            var element = GetElement(unit);
            var score = GetFriendScore(unit.Sorting, unit.Ticks);
            _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, score);

            // 刷新所有UnitKey过期时间
            _ = batch.KeyExpireAsync(UnitHashKey(unit.Id), _cacheExpire);

            SetOwnerRemindMe(batch, element, unit);
            SetOwnerRemindAll(batch, element, unit);
            SetOwnerFollowing(batch, element, unit);
            SetOwnerFriendMap(batch, element, unit, score);
            SetOwnerCreator(batch, element, unit);
            SetOwnerHasBadge(batch, element, unit);

            //Pinned
            if (unit.Sorting > 0)
            {
                totalPinned += unit.PublicBadge;
            }
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
        _ = batch.HashSetAsync(ownerStatisticSetKey, F_Total_Pinned, totalPinned);
        _ = batch.KeyExpireAsync(ownerStatisticSetKey, _cacheExpire);

        _ = batch.KeyExpireAsync(ownerFriendsSetKey, _cacheExpire);

        //destinations
        var statisticTypedSetKey = StatisticMapHashKey(ownerId);
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
        if (!await Database.KeyExistsAsync(OwnerStatisticHashKey(ownerId)))
        {
            return await SetFriendsAsync(ownerId, fetchTask);
        }
        return null;
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> GetOrSetFriendsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask)
    {
        return await SetFriendsIfNotExistsAsync(ownerId, fetchTask) ?? await GetFriendUnitsAsync(ownerId);
    }


    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, double>>> GetRawFriendsAsync(long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: OwnerFriendsSetKey(ownerId),
            start: minScore,
            stop: maxScore,
            exclude: Exclude.None,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);
        return entries.Select(x => new KeyValuePair<SessionUnitElement, double>(SessionUnitElement.Parse(x.Element), x.Score));
    }

    public async Task<IReadOnlyList<FriendModel>> GetFriendsAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        return await GetTypedFriendsAsync(FriendViews.All,
            ownerId: ownerId,
            minScore: minScore,
            maxScore: maxScore,
            skip: skip,
            take: take,
            isDescending);
    }

    public async Task<IReadOnlyList<FriendModel>> GetTypedFriendsAsync(
        FriendViews friendView,
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var zsetKey = GetFriendTypeKey(friendView, ownerId);

        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: zsetKey,
            start: minScore,
            stop: maxScore,
            exclude: Exclude.None,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);

        var list = entries.ToList();

        var scoreMap = new Dictionary<RedisValue, double>();

        if (friendView != FriendViews.All && list.Count > 0)
        {
            var elements = list.Select(x => x.Element).ToArray();
            var scores = await GetZsetScoresAsync(OwnerFriendsSetKey(ownerId), elements);
            for (int i = 0; i < elements.Length; i++)
            {
                scoreMap[elements[i]] = scores[i].GetValueOrDefault();
            }
        }

        return list.Select(x =>
        {
            var element = SessionUnitElement.Parse(x.Element);
            var finalScore = friendView == FriendViews.All ? x.Score : scoreMap.GetValueOrDefault(x.Element);
            var score = new FriendScore(finalScore);
            return new FriendModel
            {
                OwnerId = element.OwnerId,
                FriendId = element.FriendId,
                SessionId = element.SessionId,
                Id = element.SessionUnitId,
                Sorting = score.Sorting,
                Ticks = score.Ticks
            };
        }).ToList();
    }

    public async Task<long> GetTypedFriendsCountAsync(FriendViews friendView, long ownerId)
    {
        var zsetKey = GetFriendTypeKey(friendView, ownerId);
        return await Database.SortedSetLengthAsync(zsetKey);
    }

    /// <summary>
    /// 获取好友会话单元数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<long> GetFriendsCountAsync(long ownerId)
    {
        return await Database.SortedSetLengthAsync(OwnerFriendsSetKey(ownerId));
    }

    /// <summary>
    /// 获取指定类型的好友列表
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="friendType"></param>
    /// <returns>Key:UnitId,Value:OwnerId</returns>
    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, double>>> GetFriendsMapAsync(long ownerId, ChatObjectTypeEnums friendType)
    {
        var entries = await Database.SortedSetRangeByRankWithScoresAsync(OwnerFriendsMapZsetKey(ownerId, friendType));
        var result = entries.Select(x => new KeyValuePair<SessionUnitElement, double>(SessionUnitElement.Parse(x.Element), x.Score));
        return result;
    }

    public async Task<Dictionary<ChatObjectTypeEnums, long>> GetFriendsCountMapAsync(long ownerId, IEnumerable<ChatObjectTypeEnums> types = null)
    {
        var result = new Dictionary<ChatObjectTypeEnums, long>();
        var typeList = types ?? Enum.GetValues<ChatObjectTypeEnums>();
        foreach (var item in typeList)
        {
            var length = await Database.SortedSetLengthAsync(OwnerFriendsMapZsetKey(ownerId, item));
            result.TryAdd(item, length);
        }
        return result;
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
            tasks.Add(batch.HashGetAllAsync(UnitHashKey(unitId)));
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

        return units.FirstOrDefault().Value;
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

        var membersMap = await GetMembersMapAsync(sessionId);
        //var unitList = await GetListBySessionAsync(sessionId);
        //var unitList = await SessionUnitManager.GetCacheListAsync(message);

        var pinnedMap = await GetPinnedMembersMapAsync(sessionId);
        var immersedMap = await GetImmersedMembersMapAsync(sessionId);
        var creatorMap = await GetCreatorMembersMapAsync(sessionId);
        //var umitMap = new Dictionary<long, (bool Immersed, double Sorting)>();

        Logger.LogInformation(
            "[{method}], sessionId:{sessionId}, lastMessageId:{lastMessageId}, count:{count},Elapsed:{ms}ms",
            nameof(GetMembersMapAsync),
            sessionId,
            lastMessageId,
            membersMap.Count,
            stopwatch.ElapsedMilliseconds);

        if (membersMap == null || !membersMap.Any()) return;

        stopwatch.Restart();

        var batch = Database.CreateBatch();

        var expireTime = expire ?? _cacheExpire;

        foreach (var item in membersMap)
        {
            var unitId = item.Key;
            var element = item.Value;
            var ownerId = element.OwnerId;
            var friendId = element.FriendId;
            var unitKey = UnitHashKey(unitId);
            var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);
            var ownerStatisticSetKey = OwnerStatisticHashKey(ownerId);
            var isSender = unitId == message.SenderSessionUnitId;
            var isImmersed = immersedMap.TryGetValue(ownerId, out bool immersed) && immersed;
            var isCreator = creatorMap.TryGetValue(ownerId, out bool creator) && creator;
            var sorting = pinnedMap.TryGetValue(ownerId, out var _sorting) ? _sorting : 0;

            // lastMessageId
            _ = batch.HashSetAsync(unitKey, F_LastMessageId, lastMessageId);

            // ticks
            var ticks = new DateTimeOffset(message.CreationTime).ToUnixTimeMilliseconds();
            _ = batch.HashSetAsync(unitKey, F_Ticks, ticks);

            // owner sortedset tick: message.CreationTime.Ticks
            var score = GetFriendScore(sorting, ticks);
            _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, score);

            // expire
            if (expireTime.HasValue)
            {
                _ = batch.KeyExpireAsync(unitKey, expireTime.Value);
                _ = batch.KeyExpireAsync(ownerFriendsSetKey, expireTime.Value);
            }

            // Sender
            if (isSender)
            {
                continue;
            }

            // receiverType
            HashIncrementIfExist(batch, StatisticMapHashKey(ownerId), receiverType.ToString(), 1);

            // badge
            _ = batch.HashIncrementAsync(unitKey, isPrivate ? F_PrivateBadge : F_PublicBadge, 1);

            // setTopping/pinned
            if (sorting > 0)
            {
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_Pinned, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerPinnedBadgeSetKey(ownerId), element, 1);
            }

            //创建人
            if (isCreator)
            {
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerCreatorSetKey(ownerId), element, 1);
            }

            // 静默方式 IsImmersed
            if (isImmersed)
            {
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_Immersed, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerImmersedSetKey(ownerId), element, 1);
            }
            else
            {
                HashIncrementIfExist(batch, ownerStatisticSetKey, isPrivate ? F_Total_Private : F_Total_Public, 1);
                //HasBadge
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerHasBadgeSetKey(ownerId), element, 1);
            }

            //remindAllCount
            if (isRemindAll)
            {
                _ = batch.HashIncrementAsync(unitKey, F_RemindAllCount, 1);
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_RemindAll, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerRemindAllSetKey(ownerId), element, 1);
            }
            //remindMeCount
            if (reminderIds.Contains(unitId))
            {
                _ = batch.HashIncrementAsync(unitKey, F_RemindMeCount, 1);
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_RemindMe, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerRemindMeSetKey(ownerId), element, 1);
            }
            // followingCount
            if (followerIds.Contains(unitId))
            {
                _ = batch.HashIncrementAsync(unitKey, F_FollowingCount, 1);
                HashIncrementIfExist(batch, ownerStatisticSetKey, F_Total_Following, 1);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerFollowingSetKey(ownerId), element, 1);
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
        var entries = await Database.HashGetAllAsync(OwnerStatisticHashKey(ownerId));
        return MapToStatistic(entries);
    }

    public virtual async Task<bool> RemoveStatisticAsync(long ownerId)
    {
        return await Database.KeyDeleteAsync(OwnerStatisticHashKey(ownerId));
    }

    public virtual async Task<Dictionary<ChatObjectTypeEnums, long>> GetRawBadgeMapAsync(long ownerId)
    {
        var entries = await Database.HashGetAllAsync(StatisticMapHashKey(ownerId));
        var result = entries.ToDictionary(
            x => x.Name.ToEnum<ChatObjectTypeEnums>(),
            x => x.Value.ToLong());

        return result;
    }

    public async Task<Dictionary<ChatObjectTypeEnums, long>> GetBadgeMapAsync(long ownerId)
    {
        var raw = await GetRawBadgeMapAsync(ownerId);
        // 补齐所有枚举
        foreach (var type in Enum.GetValues<ChatObjectTypeEnums>())
        {
            raw.TryAdd(type, 0);
        }
        return raw;
    }

    #endregion

    #region Remove / Set / Misc

    private void UpdateUnit(IBatch batch, SessionUnitCacheItem unit, SessionUnitCounterInfo counter)
    {
        unit.PublicBadge = counter.PublicBadge;
        unit.PrivateBadge = counter.PrivateBadge;
        unit.RemindAllCount = counter.RemindAllCount;
        unit.RemindMeCount = counter.RemindMeCount;
        unit.FollowingCount = counter.FollowingCount;

        SetUnit(batch, unit, refreshExpire: true);

        var element = GetElement(unit);

        ZsetUpdateIfExistsAsync(batch, OwnerFollowingSetKey(unit.OwnerId), element, counter.FollowingCount);
        ZsetUpdateIfExistsAsync(batch, OwnerRemindAllSetKey(unit.OwnerId), element, counter.RemindAllCount, removeWhenZero: true);
        ZsetUpdateIfExistsAsync(batch, OwnerRemindMeSetKey(unit.OwnerId), element, counter.RemindMeCount, removeWhenZero: true);
        ZsetUpdateIfExistsAsync(batch, OwnerImmersedSetKey(unit.OwnerId), element, counter.PublicBadge);
        ZsetUpdateIfExistsAsync(batch, OwnerPinnedBadgeSetKey(unit.OwnerId), element, counter.PublicBadge);
        ZsetUpdateIfExistsAsync(batch, OwnerCreatorSetKey(unit.OwnerId), element, counter.PublicBadge);
        ZsetUpdateIfExistsAsync(batch, OwnerHasBadgeSetKey(unit.OwnerId), element, counter.PublicBadge, removeWhenZero: true);
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
            HashIncrementIfExist(batch, StatisticMapHashKey(unit.OwnerId), unit.DestinationObjectType.ToString(), -publicBadge);
        }

        // 减量更新 Owner 总角标（key 必须已存在才更新）
        var ownerStatisticSetKey = OwnerStatisticHashKey(counter.OwnerId);

        void DecrementStatisticAsync(string field, int val)
        {
            if (val > 0)
            {
                Logger.LogInformation($"ownerId:{counter.OwnerId},需要减量 [{field}]:{val}");
                HashIncrementIfExist(batch, ownerStatisticSetKey, field, -val);
            }
        }

        DecrementStatisticAsync(immersed ? F_Total_Immersed : F_Total_Public, publicBadge);

        DecrementStatisticAsync(F_Total_Pinned, publicBadge);
        DecrementStatisticAsync(F_Total_Private, privatePublic);
        DecrementStatisticAsync(F_Total_RemindMe, remindMeCount);
        DecrementStatisticAsync(F_Total_RemindAll, remindAllCount);
        DecrementStatisticAsync(F_Total_Following, followingCount);

        batch.Execute();
    }

    public async Task SetPinningAsync(Guid sessionId, Guid unitId, long ownerId, long sorting)
    {
        var unitKey = UnitHashKey(unitId);

        var unit = await GetAsync(unitId);

        // 1. 检查缓存项是否存在
        var isExists = unit != null;

        // 2. owner sortedset key
        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);

        // 3. 先读取旧 score
        var element = GetElement(unit);

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
            _ = batch.SortedSetRemoveAsync(ownerPinnedBadgeSetKey, element);
            _ = batch.SortedSetRemoveAsync(sessionPinnedSortingSetKey, element);
        }
        else
        {
            _ = batch.SortedSetAddAsync(ownerPinnedBadgeSetKey, element, unit?.PublicBadge ?? 0);
            _ = batch.SortedSetAddAsync(sessionPinnedSortingSetKey, element, sorting);
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
            var ownerStatisticSetKey = OwnerStatisticHashKey(unit.OwnerId);
            var publicDelta = isImmersed ? -unit.PublicBadge : unit.PublicBadge;
            var immersedDelta = -publicDelta;

            //_ = tran.HashIncrementAsync(ownerStatisticSetKey, F_Total_Public, publicDelta);
            //_ = tran.HashIncrementAsync(ownerStatisticSetKey, F_Total_Immersed, immersedDelta);
            //_ = tran.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerStatisticSetKey], [F_Total_Public, publicDelta]);
            //_ = tran.ScriptEvaluateAsync(IncrementIfExistsScript, [ownerStatisticSetKey], [F_Total_Immersed, immersedDelta]);

            HashIncrementIfExist(tran, ownerStatisticSetKey, F_Total_Public, publicDelta);
            HashIncrementIfExist(tran, ownerStatisticSetKey, F_Total_Immersed, immersedDelta);

            //destinations
            //_ = tran.ScriptEvaluateAsync(IncrementIfExistsScript, [StatisticTypedSetKey(unit.OwnerId)], [unit.DestinationObjectType.ToString(), publicDelta]);
            HashIncrementIfExist(tran, StatisticMapHashKey(unit.OwnerId), unit.DestinationObjectType.ToString(), publicDelta);
        }
        else
        {
            Logger.LogWarning("PublicBadge: 0 ,unitId:{unitId}", unitId);
        }

        _ = tran.HashSetAsync(UnitHashKey(unitId), F_Immersed, isImmersed);

        var element = GetElement(unit);

        //HasBadge
        _ = ZsetUpdateIfExistsAsync(tran, OwnerHasBadgeSetKey(unit.OwnerId), element, isImmersed ? 0 : unit.PublicBadge, removeWhenZero: true);

        var ownerImmersedSetKey = OwnerImmersedSetKey(unit.OwnerId);
        var sessionImmersedSetKey = SessionImmersedSetKey(unit.SessionId!.Value);
        if (isImmersed)
        {
            _ = tran.SortedSetAddAsync(ownerImmersedSetKey, element, unit.PublicBadge);
            _ = tran.KeyExpireAsync(ownerImmersedSetKey, _cacheExpire);

            _ = tran.SortedSetAddAsync(sessionImmersedSetKey, element, unit.PublicBadge);
            _ = tran.KeyExpireAsync(sessionImmersedSetKey, _cacheExpire);
        }
        else
        {
            _ = tran.SortedSetRemoveAsync(ownerImmersedSetKey, element);
            _ = tran.SortedSetRemoveAsync(sessionImmersedSetKey, element);
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

    public async Task<bool> ClearBadgeAsync(long ownerId)
    {
        var stopwatch = Stopwatch.StartNew();

        var friends = (await GetRawFriendsAsync(ownerId)).ToList();

        Logger.LogInformation("{method}, ownerId={ownerId}, friends count={count},[START] Elapsed: {Elapsed}ms",
            nameof(ClearBadgeAsync),
            ownerId,
            friends.Count,
            stopwatch.ElapsedMilliseconds);

        var batch = Database.CreateBatch();

        // Statistic
        var ownerStatisticHashKey = OwnerStatisticHashKey(ownerId);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_Public, 0);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_Private, 0);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_RemindMe, 0);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_RemindAll, 0);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_Following, 0);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_Immersed, 0);
        _ = HashSetIfFieldExistsAsync(batch, ownerStatisticHashKey, F_Total_Pinned, 0);
        _ = batch.KeyExpireAsync(ownerStatisticHashKey, _cacheExpire, when: ExpireWhen.Always);

        // StatisticMap
        var statisticMapHashKey = StatisticMapHashKey(ownerId);
        foreach (var item in Enum.GetValues<ChatObjectTypeEnums>())
        {
            _ = HashSetIfFieldExistsAsync(batch, statisticMapHashKey, item.ToString(), 0);
        }

        // friends
        foreach (var item in friends)
        {
            var element = item.Key;
            var unitKey = UnitHashKey(element.SessionUnitId);
            _ = HashSetIfFieldExistsAsync(batch, unitKey, F_PublicBadge, 0);
            _ = HashSetIfFieldExistsAsync(batch, unitKey, F_PrivateBadge, 0);
            _ = HashSetIfFieldExistsAsync(batch, unitKey, F_RemindMeCount, 0);
            _ = HashSetIfFieldExistsAsync(batch, unitKey, F_RemindAllCount, 0);
            _ = HashSetIfFieldExistsAsync(batch, unitKey, F_FollowingCount, 0);

            // Immersed
            _ = batch.SortedSetAddAsync(OwnerImmersedSetKey(ownerId), element, 0, SortedSetWhen.Exists);
            // PinnedBadge
            _ = batch.SortedSetAddAsync(OwnerPinnedBadgeSetKey(ownerId), element, 0, SortedSetWhen.Exists);
            //// RemindAll
            //_ = batch.SortedSetAddAsync(OwnerRemindAllSetKey(ownerId), element, 0, SortedSetWhen.Exists);
            //// RemindMe
            //_ = batch.SortedSetAddAsync(OwnerRemindMeSetKey(ownerId), element, 0, SortedSetWhen.Exists);
            // Following
            _ = batch.SortedSetAddAsync(OwnerFollowingSetKey(ownerId), element, 0, SortedSetWhen.Exists);
            // Creator
            _ = batch.SortedSetAddAsync(OwnerCreatorSetKey(ownerId), element, 0, SortedSetWhen.Exists);
        }
        // RemindAll
        _ = batch.KeyDeleteAsync(OwnerRemindAllSetKey(ownerId));
        // RemindMe
        _ = batch.KeyDeleteAsync(OwnerRemindMeSetKey(ownerId));
        // HasBadge
        _ = batch.KeyDeleteAsync(OwnerHasBadgeSetKey(ownerId));

        // session
        var sessionIds = friends.Select(x => x.Key.SessionId).Distinct();
        foreach (var item in friends)
        {
            _ = batch.SortedSetAddAsync(SessionImmersedSetKey(item.Key.SessionId), item.Key, 0, SortedSetWhen.Exists);
        }
        batch.Execute();

        Logger.LogInformation("{method} ownerId:{ownerId}, [END] Elapsed:{elapsed}ms",
            nameof(ClearBadgeAsync),
            ownerId,
            stopwatch.ElapsedMilliseconds);

        return true;
    }

    #endregion
}
