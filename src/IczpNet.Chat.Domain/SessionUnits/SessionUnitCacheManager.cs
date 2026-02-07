using AutoMapper.Internal;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.RedisServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitCacheManager : RedisService, ISessionUnitCacheManager
{
    protected IOptions<SessionUnitOptions> SessionUnitOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<SessionUnitOptions>>();

    protected SessionUnitOptions Config => SessionUnitOptions.Value;

    protected override TimeSpan? CacheExpire => Config.CacheExpire;

    private delegate Task SessionMemberLoader(Guid sessionId, IBatch batch, MemberMaps maps);

    protected string Prefix => $"{Options.Value.KeyPrefix}SessionUnits:";

    private const string UnitKeyPattern = "Units:";

    /// <summary>
    /// 会话单元详情信息
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    private RedisKey UnitHashKey(Guid unitId) => $"{Prefix}{UnitKeyPattern}{unitId}";

    /// <summary>
    /// TryParse UnitId
    /// </summary>
    /// <param name="key"></param>
    /// <param name="unitId"></param>
    /// <returns></returns>
    private bool TryParseUnitId(RedisKey key, out Guid unitId)
    {
        var keyStr = key.ToString();

        unitId = Guid.Empty;

        if (!keyStr.StartsWith(Prefix + UnitKeyPattern))
            return false;

        var guidPart = keyStr[(Prefix.Length + UnitKeyPattern.Length)..];

        return Guid.TryParse(guidPart, out unitId);
    }

    /// <summary>
    /// 会话单元集合
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionMembersSetKey(Guid sessionId) => $"{Prefix}Sessions:Members:{sessionId}";

    /// <summary>
    /// 置顶的会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionPinnedSortingHashKey(Guid sessionId) => $"{Prefix}Sessions:PinnedSorting:{sessionId}";

    /// <summary>
    /// 静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionImmersedHashKey(Guid sessionId) => $"{Prefix}Sessions:Immersed:{sessionId}";

    /// <summary>
    /// 创建人
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionCreatorHashKey(Guid sessionId) => $"{Prefix}Sessions:Creator:{sessionId}";

    /// <summary>
    /// 非公开会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionPrivateHashKey(Guid sessionId) => $"{Prefix}Sessions:Private:{sessionId}";

    /// <summary>
    /// 固定的会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionStaticHashKey(Guid sessionId) => $"{Prefix}Sessions:Static:{sessionId}";

    /// <summary>
    /// 创建人
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionBoxHashKey(Guid sessionId) => $"{Prefix}Sessions:Box:{sessionId}";

    /// <summary>
    /// 好友会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerFriendsSetKey(long ownerId) => $"{Prefix}Owners:Friends:{ownerId}";

    /// <summary>
    /// 好友分类
    /// </summary>
    /// <param name="friendType"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerFriendsMapZsetKey(long ownerId, ChatObjectTypeEnums? friendType) => $"{Prefix}Owners:FriendsMap:{friendType}:{ownerId}";

    /// <summary>
    /// 置顶的会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerPinnedBadgeSetKey(long ownerId) => $"{Prefix}Owners:PinnedBadge:{ownerId}";

    /// <summary>
    /// 有未读消息的会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerHasBadgeSetKey(long ownerId) => $"{Prefix}Owners:HasBadge:{ownerId}";

    /// <summary>
    /// 静默会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerImmersedSetKey(long ownerId) => $"{Prefix}Owners:Immersed:{ownerId}";

    /// <summary>
    /// 关注会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerFollowingSetKey(long ownerId) => $"{Prefix}Owners:Following:{ownerId}";

    /// <summary>
    /// @所有人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerRemindAllSetKey(long ownerId) => $"{Prefix}Owners:RemindAll:{ownerId}";

    /// <summary>
    /// @所有人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerRemindMeSetKey(long ownerId) => $"{Prefix}Owners:RemindMe:{ownerId}";

    /// <summary>
    /// 创建人 会话单元
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerCreatorSetKey(long ownerId) => $"{Prefix}Owners:Creator:{ownerId}";

    /// <summary>
    /// 消息盒子
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="boxId"></param>
    /// <returns></returns>
    private RedisKey OwnerBoxFriendsSetKey(long ownerId, Guid boxId) => $"{Prefix}Owners:BoxFriends:{ownerId}:{boxId}";

    /// <summary>
    /// 消息盒子角标统计
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerBoxBadgeZsetKey(long ownerId) => $"{Prefix}Owners:BoxBadge:{ownerId}";

    /// <summary>
    /// 消息统计
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerStatisticHashKey(long ownerId) => $"{Prefix}Owners:Statistic:{ownerId}";

    /// <summary>
    /// 消息统计(分类)
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey StatisticMapHashKey(long ownerId) => $"{Prefix}Owners:StatisticMap:{ownerId}";

    /// <summary>
    /// RedisKey： Element
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private static SessionUnitElement GetElement(SessionUnitCacheItem unit)
    {
        return SessionUnitElement.Create(unit.OwnerId, unit.OwnerObjectType, unit.DestinationId.GetValueOrDefault(), unit.DestinationObjectType, unit.Id, unit.SessionId.GetValueOrDefault());
    }

    /// <summary>
    /// RedisScore FriendScore
    /// </summary>
    private static double GetFriendScore(double sorting, double ticks)
    {
        return FriendScore.Create(sorting, ticks);
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
    private static string F_LastSendMessageId => nameof(SessionUnitCacheItem.LastSendMessageId);
    private static string F_LastSendTime => nameof(SessionUnitCacheItem.LastSendTime);
    private static string F_PublicBadge => nameof(SessionUnitCacheItem.PublicBadge);
    private static string F_PrivateBadge => nameof(SessionUnitCacheItem.PrivateBadge);
    private static string F_RemindAllCount => nameof(SessionUnitCacheItem.RemindAllCount);
    private static string F_RemindMeCount => nameof(SessionUnitCacheItem.RemindMeCount);
    private static string F_FollowingCount => nameof(SessionUnitCacheItem.FollowingCount);
    private static string F_Ticks => nameof(SessionUnitCacheItem.Ticks);
    private static string F_Sorting => nameof(SessionUnitCacheItem.Sorting);
    private static string F_Immersed => nameof(SessionUnitCacheItem.IsImmersed);
    private static string F_BoxId => nameof(SessionUnitCacheItem.BoxId);

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

    private void SetOwnerStatistic(IBatch batch, long ownerId, SessionUnitStatistic statistic)
    {
        var ownerStatisticSetKey = OwnerStatisticHashKey(ownerId);
        var entries = RedisMapper.ToHashEntries(statistic);
        _ = batch.HashSetAsync(ownerStatisticSetKey, entries);
        _ = batch.KeyExpireAsync(ownerStatisticSetKey, CacheExpire);
    }

    private void SetOwnerStatisticTypedMap(IBatch batch, long ownerId, Dictionary<ChatObjectTypeEnums, long> statTypedMap)
    {
        //destinations
        var statisticTypedSetKey = StatisticMapHashKey(ownerId);
        foreach (var item in statTypedMap)
        {
            _ = batch.HashSetAsync(statisticTypedSetKey, item.Key.ToString(), item.Value);
        }
        _ = batch.KeyExpireAsync(statisticTypedSetKey, CacheExpire);
    }

    private void SetOwnerBoxBadge(IBatch batch, long ownerId, Dictionary<Guid, long> statBoxMap)
    {
        var ownerBoxBadgeZsetKey = OwnerBoxBadgeZsetKey(ownerId);
        foreach (var item in statBoxMap)
        {
            var boxId = item.Key;
            _ = batch.SortedSetAddAsync(ownerBoxBadgeZsetKey, boxId.ToString(), statBoxMap.GetValueOrDefault(boxId));
        }
        _ = batch.KeyExpireAsync(ownerBoxBadgeZsetKey, CacheExpire);
    }


    private void SetOwnerPinning(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.Sorting > 0, () => OwnerPinnedBadgeSetKey(unit.OwnerId), element, unit.PublicBadge, batch: batch);

    private void SetOwnerHasBadge(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.PublicBadge > 0, () => OwnerHasBadgeSetKey(unit.OwnerId), element, unit.PublicBadge, batch: batch);

    private void SetOwnerImmersed(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.IsImmersed, () => OwnerImmersedSetKey(unit.OwnerId), element, unit.PublicBadge, batch: batch);

    private void SetOwnerFollowing(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.FollowingCount > 0, () => OwnerFollowingSetKey(unit.OwnerId), element, unit.FollowingCount, batch: batch);

    private void SetOwnerRemindAll(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.RemindAllCount > 0, () => OwnerRemindAllSetKey(unit.OwnerId), element, unit.RemindAllCount, batch: batch);

    private void SetOwnerRemindMe(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.RemindMeCount > 0, () => OwnerRemindMeSetKey(unit.OwnerId), element, unit.RemindMeCount, batch: batch);

    private void SetOwnerCreator(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => SortedSetIf(unit.IsCreator, () => OwnerCreatorSetKey(unit.OwnerId), element, unit.PublicBadge, batch: batch);

    private void SetOwnerBoxFriends(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit, double score)
         => SortedSetIf(unit.BoxId.HasValue, () => OwnerBoxFriendsSetKey(unit.OwnerId, unit.BoxId.Value), element, score, batch: batch);

    private void SetOwnerBoxFriends(IBatch batch, long ownerId, Guid? boxId, SessionUnitElement element, double score)
         => SortedSetIf(boxId.HasValue, () => OwnerBoxFriendsSetKey(ownerId, boxId.Value), element, score, batch: batch);

    private void SetOwnerFriends(IBatch batch, long ownerId, SessionUnitElement element, double score)
        => SortedSetIf(true, () => OwnerFriendsSetKey(ownerId), element, score, batch: batch);

    private void SetOwnerFriendMap(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit, double score)
        => SortedSetIf(true, () => OwnerFriendsMapZsetKey(unit.OwnerId, unit.DestinationObjectType), element, score, batch: batch);

    private void SetSessionPinnedSorting(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => HashSetIf(unit.Sorting > 0, () => SessionPinnedSortingHashKey(unit.SessionId.Value), element, unit.Sorting, batch: batch);

    private void SetSessionImmersed(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => HashSetIf(unit.IsImmersed, () => SessionImmersedHashKey(unit.SessionId.Value), element, unit.IsImmersed, batch: batch);

    private void SetSessionCreator(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => HashSetIf(unit.IsCreator, () => SessionImmersedHashKey(unit.SessionId.Value), element, unit.IsCreator, batch: batch);

    private void SetSessionPrivate(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => HashSetIf(!unit.IsPublic, () => SessionPrivateHashKey(unit.SessionId.Value), element, !unit.IsPublic, batch: batch);

    private void SetSessionStatic(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => HashSetIf(unit.IsStatic, () => SessionStaticHashKey(unit.SessionId.Value), element, unit.IsStatic, batch: batch);

    private void SetSessionBox(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit)
        => HashSetIf(unit.BoxId.HasValue, () => SessionBoxHashKey(unit.SessionId.Value), element, unit.BoxId.ToString(), batch: batch);


    private void SetUnit(IBatch batch, SessionUnitCacheItem unit, bool refreshExpire)
    {
        var unitKey = UnitHashKey(unit.Id);
        var entries = MapToHashEntries(unit);
        _ = batch.HashSetAsync(unitKey, entries);
        if (refreshExpire)
        {
            _ = batch.KeyExpireAsync(unitKey, CacheExpire);
        }
    }

    private string GetFriendTypeKey(FriendViews friendView, long ownerId, Guid? boxId = null)
    {
        return friendView switch
        {
            FriendViews.All => boxId.HasValue ? OwnerBoxFriendsSetKey(ownerId, boxId.Value) : OwnerFriendsSetKey(ownerId),
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

    private static Task<Dictionary<SessionUnitElement, TValue>> GetSessionMembersMapAsync<TValue>(IBatch batch, string redisKey, Func<RedisValue, TValue> valueSelector)
    {
        // 注意这里不 await，而是直接返回 Task
        var entriesTask = batch.HashGetAllAsync(redisKey);

        return entriesTask.ContinueWith(t =>
            t.Result.ToDictionary(
                x => SessionUnitElement.Parse(x.Name),
                x => valueSelector(x.Value)
            )
        );
    }

    private async Task<IEnumerable<KeyValuePair<SessionUnitElement, TValue>>> GetSessionMembersAsync<TValue>(string redisKey, Func<RedisValue, TValue> valueSelector)
    {
        var entries = await Database.HashGetAllAsync(redisKey);
        return entries.Select(x =>
            new KeyValuePair<SessionUnitElement, TValue>(
                SessionUnitElement.Parse(x.Name),
                valueSelector(x.Value)
            ));
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetMembersAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var stopwatch = Stopwatch.StartNew();

        var unitList = units.ToList();

        if (unitList.Count == 0) return [];

        var batch = Database.CreateBatch();

        //var lastMsgKey = LastMessageSetKey(sessionId);

        var unitKeys = unitList.Select(x => UnitHashKey(x.Id)).ToList();
        var unitKeysExists = await BatchKeyExistsAsync(unitKeys);
        var existsCount = unitKeysExists.Count(x => x.Value);

        var uncachedUnitIds = unitKeysExists.Where(x => !x.Value)
            .Select(x => (Guid?)(TryParseUnitId(x.Key, out Guid _unitId) ? _unitId : null))
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            //.Distinct()
            .ToHashSet()
            ;

        var uncachedUnits = unitList.Where(x => uncachedUnitIds.Contains(x.Id));

        foreach (var unit in unitList)
        {
            SetUnit(batch, unit, refreshExpire: true);
        }

        Logger.LogInformation("sessionId={sessionId},unitKeysExists:{ExistsCount}",
            sessionId,
            existsCount);


        // clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(SessionMembersSetKey(sessionId));
        // BuildSessionMembers
        BuildSessionMembers(batch, sessionId, unitList);

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

    private void BuildSessionMembers(IBatch batch, Guid sessionId, List<SessionUnitCacheItem> unitList)
    {
        var sessionMembersSetKey = SessionMembersSetKey(sessionId);
        foreach (var unit in unitList)
        {
            // set session relations
            SetSessionRelations(batch, sessionMembersSetKey, unit);
        }
        _ = batch.KeyExpireAsync(sessionMembersSetKey, CacheExpire);
    }

    private void SetSessionRelations(IBatch batch, RedisKey sessionMembersSetKey, SessionUnitCacheItem unit)
    {
        var element = GetElement(unit);
        var score = MemberScore.Create(unit.IsCreator, unit.CreationTime);
        _ = batch.SortedSetAddAsync(sessionMembersSetKey, element, score);

        // set session Topping
        SetSessionPinnedSorting(batch, element, unit);
        // set session Immersed
        SetSessionImmersed(batch, element, unit);
        // set sessuib IsCreator
        SetSessionCreator(batch, element, unit);
        // set sessuib IsPublic==false
        SetSessionPrivate(batch, element, unit);
        // set sessuib IsStatic
        SetSessionStatic(batch, element, unit);
        // set session boxId
        SetSessionBox(batch, element, unit);
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
    /// 批量加载成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="load"></param>
    /// <returns></returns>
    public async Task<MemberMaps> BatchGetMembersMapAsync(Guid sessionId, MemberLoad load)
    {
        var result = new MemberMaps();
        var batch = Database.CreateBatch();
        var tasks = new List<Task>();

        if (load.HasFlag(MemberLoad.Pinned))
        {
            tasks.Add(GetSessionMembersMapAsync(batch, SessionPinnedSortingHashKey(sessionId), x => x.ToLong()).ContinueWith(t => result.Pinned = t.Result));
        }

        if (load.HasFlag(MemberLoad.Immersed))
        {
            tasks.Add(GetSessionMembersMapAsync(batch, SessionImmersedHashKey(sessionId), x => x.ToBoolean()).ContinueWith(t => result.Immersed = t.Result));
        }

        if (load.HasFlag(MemberLoad.Creator))
        {
            tasks.Add(GetSessionMembersMapAsync(batch, SessionCreatorHashKey(sessionId), x => x.ToBoolean()).ContinueWith(t => result.Creator = t.Result));
        }

        if (load.HasFlag(MemberLoad.Private))
        {
            tasks.Add(GetSessionMembersMapAsync(batch, SessionPrivateHashKey(sessionId), x => x.ToBoolean()).ContinueWith(t => result.Private = t.Result));
        }

        if (load.HasFlag(MemberLoad.Static))
        {
            tasks.Add(GetSessionMembersMapAsync(batch, SessionStaticHashKey(sessionId), x => x.ToBoolean()).ContinueWith(t => result.Static = t.Result));
        }

        if (load.HasFlag(MemberLoad.Box))
        {
            tasks.Add(GetSessionMembersMapAsync(batch, SessionBoxHashKey(sessionId), x => x.ToGuid()).ContinueWith(t => result.Box = t.Result));
        }

        // 批量执行 Redis
        batch.Execute();

        await Task.WhenAll(tasks);

        return result;
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
    public Task<IEnumerable<KeyValuePair<SessionUnitElement, long>>> GetPinnedMembersAsync(Guid sessionId)
        => GetSessionMembersAsync(SessionPinnedSortingHashKey(sessionId), v => v.ToLong());

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>{Key:OwnerId, Value:PublicBadge}</returns>
    public Task<IEnumerable<KeyValuePair<SessionUnitElement, bool>>> GetImmersedMembersAsync(Guid sessionId)
        => GetSessionMembersAsync(SessionImmersedHashKey(sessionId), v => v.ToBoolean());

    /// <summary>
    /// 获取静默会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<IEnumerable<KeyValuePair<SessionUnitElement, bool>>> GetCreatorMembersAsync(Guid sessionId)
        => GetSessionMembersAsync(SessionCreatorHashKey(sessionId), v => v.ToBoolean());

    /// <summary>
    /// 获取非公开会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<IEnumerable<KeyValuePair<SessionUnitElement, bool>>> GetPrivateMembersAsync(Guid sessionId)
        => GetSessionMembersAsync(SessionPrivateHashKey(sessionId), v => v.ToBoolean());

    /// <summary>
    /// 获取固定会话成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<IEnumerable<KeyValuePair<SessionUnitElement, bool>>> GetStaticMembersAsync(Guid sessionId)
        => GetSessionMembersAsync(SessionStaticHashKey(sessionId), v => v.ToBoolean());

    /// <summary>
    /// 获取消息盒子会话单元
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public Task<IEnumerable<KeyValuePair<SessionUnitElement, Guid>>> GetBoxMembersAsync(Guid sessionId)
        => GetSessionMembersAsync(SessionBoxHashKey(sessionId), v => v.ToGuid());

    /// <summary>
    /// 获取会话单元数量
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<long> GetMembersCountAsync(Guid sessionId)
    {
        var totalCount = await Database.SortedSetLengthAsync(SessionMembersSetKey(sessionId));
        var privateCount = await Database.HashLengthAsync(SessionPrivateHashKey(sessionId));
        var result = totalCount - privateCount;
        return result;
    }

    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, MemberScore>>> GetRawMembersAsync(
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
        return redisZset.Select(x => new KeyValuePair<SessionUnitElement, MemberScore>(SessionUnitElement.Parse(x.Element), new MemberScore(x.Score))); ;
    }

    public async Task<IEnumerable<MemberModel>> GetMembersAsync(
        Guid sessionId,
        bool? isCreator = null,
        bool? isPrivate = null,
        bool? isStatic = null,
        bool? isImmersed = null,

        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var redisZset = await GetRawMembersAsync(
            sessionId: sessionId,
            minScore: minScore,
            maxScore: maxScore,
            skip: skip,
            take: take,
            isDescending: isDescending);

        var memberLoad = MemberLoad.Creator;

        if (isPrivate.HasValue)
        {
            memberLoad |= MemberLoad.Private;
        }
        if (isStatic.HasValue)
        {
            memberLoad |= MemberLoad.Static;
        }
        if (isImmersed.HasValue)
        {
            memberLoad |= MemberLoad.Immersed;
        }
        var maps = await BatchGetMembersMapAsync(sessionId, memberLoad);

        return redisZset
            // Private
            .WhereIf(maps.Private.Count != 0 && isPrivate == true, x => maps.Private.Select(d => d.Key).Contains(x.Key))
            .WhereIf(maps.Private.Count != 0 && isPrivate == false, x => !maps.Private.Select(d => d.Key).Contains(x.Key))
            // Static
            .WhereIf(maps.Static.Count != 0 && isStatic == true, x => maps.Static.Select(d => d.Key).Contains(x.Key))
            .WhereIf(maps.Static.Count != 0 && isStatic == false, x => !maps.Static.Select(d => d.Key).Contains(x.Key))
            // isImmersed
            .WhereIf(maps.Immersed.Count != 0 && isImmersed == true, x => maps.Immersed.Select(d => d.Key).Contains(x.Key))
            .WhereIf(maps.Immersed.Count != 0 && isImmersed == false, x => !maps.Immersed.Select(d => d.Key).Contains(x.Key))
            .Select(x =>
            {
                var element = x.Key;
                var score = x.Value;
                return new MemberModel
                {
                    SessionId = element.SessionId,
                    OwnerId = element.OwnerId,
                    OwnerObjectType = element.OwnerObjectType,
                    DestinationId = element.DestinationId,
                    DestinationObjectType = element.DestinationObjectType,
                    Id = element.SessionUnitId,
                    CreationTime = score.CreationTime,
                    IsCreator = score.IsCreator
                };
            })
           ;
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


    private static void AccumulateStatistics(SessionUnitCacheItem unit, SessionUnitStatistic stat, Dictionary<ChatObjectTypeEnums, long> statTypedMap, Dictionary<Guid, long> statBoxMap)
    {
        //Pinned
        if (unit.Sorting > 0)
        {
            stat.Pinned += unit.PublicBadge;
        }
        //IsImmersed
        if (unit.IsImmersed)
        {
            stat.Immersed += unit.PublicBadge;
        }
        else
        {
            //非静默统计PublicBadge
            stat.PublicBadge += unit.PublicBadge;
            // statTypedMap
            if (unit.DestinationObjectType.HasValue)
            {
                statTypedMap[unit.DestinationObjectType.Value] += unit.PublicBadge;
            }
            // statBoxMap
            if (unit.BoxId.HasValue)
            {
                if (!statBoxMap.ContainsKey(unit.BoxId.Value))
                {
                    statBoxMap[unit.BoxId.Value] = 0;
                }
                statBoxMap[unit.BoxId.Value] += unit.PublicBadge;
            }
        }
        stat.PrivateBadge += unit.PrivateBadge;
        stat.RemindMe += unit.RemindMeCount;
        stat.RemindAll += unit.RemindAllCount;
        stat.Following += unit.FollowingCount;
    }

    private sealed record StatisticContext(SessionUnitStatistic Statistic, Dictionary<ChatObjectTypeEnums, long> StatisticTypedMap, Dictionary<Guid, long> StatisticBoxMap);

    private StatisticContext BuildOwnerStatistics(IBatch batch, long ownerId, IEnumerable<SessionUnitCacheItem> units)
    {
        var stat = new SessionUnitStatistic();
        var statBoxMap = new Dictionary<Guid, long>();
        var statTypedMap = Enum.GetValues<ChatObjectTypeEnums>()
            .ToDictionary(x => x, _ => 0L);

        var ownerFriendsSetKey = OwnerFriendsSetKey(ownerId);

        foreach (var unit in units)
        {
            var element = GetElement(unit);
            var score = GetFriendScore(unit.Sorting, unit.Ticks);

            // set friends
            _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, score);

            // 刷新所有UnitKey过期时间
            _ = batch.KeyExpireAsync(UnitHashKey(unit.Id), CacheExpire);

            SetOwnerRelations(batch, element, unit, score);

            AccumulateStatistics(unit, stat, statTypedMap, statBoxMap);
        }

        _ = batch.KeyExpireAsync(ownerFriendsSetKey, CacheExpire);

        return new(stat, statTypedMap, statBoxMap);
    }

    private void SetOwnerStatistics(IBatch batch, long ownerId, StatisticContext context)
    {
        // BoxBadge
        SetOwnerBoxBadge(batch, ownerId, context.StatisticBoxMap);
        // Statistic
        SetOwnerStatistic(batch, ownerId, context.Statistic);
        // StatisticMap
        SetOwnerStatisticTypedMap(batch, ownerId, context.StatisticTypedMap);
    }

    private void SetOwnerRelations(IBatch batch, SessionUnitElement element, SessionUnitCacheItem unit, double score)
    {
        // set Topping
        SetOwnerPinning(batch, element, unit);
        // set Immersed
        SetOwnerImmersed(batch, element, unit);

        SetOwnerRemindMe(batch, element, unit);
        SetOwnerRemindAll(batch, element, unit);
        SetOwnerFollowing(batch, element, unit);
        SetOwnerFriendMap(batch, element, unit, score);
        SetOwnerBoxFriends(batch, element, unit, score);
        SetOwnerCreator(batch, element, unit);
        SetOwnerHasBadge(batch, element, unit);
    }

    private async Task<(List<SessionUnitCacheItem> cached, List<SessionUnitCacheItem> uncached)> LoadCachedUnitsAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units)
    {
        var unitList = units?.Where(x => x != null && x.OwnerId == ownerId).ToList() ?? [];

        //trace.Log("GetManyAsync Start");
        var unitMap = await GetManyAsync(units.Select(x => x.Id));
        //trace.Log("GetManyAsync End");

        var cachedUnits = unitMap.Where(x => x.Value != null).Select(x => x.Value).ToList();
        var uncachedUnits = unitList.ExceptBy(cachedUnits.Select(x => x.Id), x => x.Id).ToList();

        //trace.Log($"cached={cached.Count}, uncached={uncached.Count}");
        return (cachedUnits, uncachedUnits);
    }

    public async Task<IEnumerable<SessionUnitCacheItem>> SetFriendsAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units)
    {
        var stopwatch = Stopwatch.StartNew();

        var (cachedUnits, uncachedUnits) = await LoadCachedUnitsAsync(ownerId, units);

        var batch = Database.CreateBatch();

        //clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(OwnerStatisticHashKey(ownerId));

        foreach (var unit in uncachedUnits)
        {
            SetUnit(batch, unit, refreshExpire: false);
        }

        //合并
        var allList = cachedUnits.Concat(uncachedUnits);
        Logger.LogInformation("allList count={count}", allList.Count());

        // build Statistics
        var ctx = BuildOwnerStatistics(batch, ownerId, allList);

        // set Statistics
        SetOwnerStatistics(batch, ownerId, ctx);

        batch.Execute();

        Logger.LogInformation("batch.Execute() [{Elapsed}]ms", stopwatch.ElapsedMilliseconds);

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

    public async Task<Dictionary<long, IEnumerable<SessionUnitElement>>> GetFriendsElementAsync(List<long> ownerIds)
    {
        var stopwatch = Stopwatch.StartNew();

        var batch = Database.CreateBatch();

        var ownerIdList = ownerIds.Distinct().ToList();

        var taskMap = ownerIdList.ToDictionary(x => x, x => batch.SortedSetRangeByScoreAsync(OwnerFriendsSetKey(x)));

        batch.Execute();

        await Task.WhenAll(taskMap.Values);

        var result = taskMap.ToDictionary(x => x.Key, x => x.Value.Result.Select(SessionUnitElement.Parse));

        Logger.LogInformation("GetFriendsElementAsync ownerIdList=[{ownerIdList}], totalCount={totalCount}, Elapsed:{Elapsed}ms",
            ownerIdList.JoinAsString(","),
            result.Values.Sum(x => x.Count()),
            stopwatch.ElapsedMilliseconds);

        return result;
    }

    /// <summary>
    /// 获取好友Score(可用于判断好友是否存在)
    /// </summary>
    /// <param name="elements"></param>
    /// <returns></returns>
    public async Task<Dictionary<long, Dictionary<long, double?>>> FindFriendsAsync(List<SessionUnitElement> elements)
    {
        var stopwatch = Stopwatch.StartNew();

        var batch = Database.CreateBatch();

        var elementMap = elements
            .Distinct()
            .GroupBy(x => x.OwnerId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var taskMap = elementMap.ToDictionary(
            x => x.Key,
            x => x.Value.ToDictionary(
                e => e.DestinationId,
                e => batch.SortedSetScoreAsync(OwnerFriendsSetKey(x.Key), e)
            )
        );

        batch.Execute();

        await Task.WhenAll(taskMap.Values.SelectMany(v => v.Values));

        var result = taskMap.ToDictionary(
            x => x.Key,
            x => x.Value.ToDictionary(
                v => v.Key,
                v => v.Value.Result
                )
            );

        Logger.LogInformation("FindFriendsAsync Elapsed:{Elapsed}ms", stopwatch.ElapsedMilliseconds);

        return result;
    }

    public async Task<IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>> GetRawFriendsAsync(long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var stopwatch = Stopwatch.StartNew();
        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: OwnerFriendsSetKey(ownerId),
            start: minScore,
            stop: maxScore,
            exclude: Exclude.None,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);
        Logger.LogInformation("GetRawFriendsAsync ownerId={ownerId}, count={count}, Elapsed:{Elapsed}ms", ownerId, entries.Length, stopwatch.ElapsedMilliseconds);
        return entries.Select(x => new KeyValuePair<SessionUnitElement, FriendScore>(SessionUnitElement.Parse(x.Element), FriendScore.Parse(x.Score)));
    }

    public async Task<IEnumerable<FriendModel>> GetFriendsAsync(
        long ownerId,
        Guid? boxId = null,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        return await GetTypedFriendsAsync(FriendViews.All,
            ownerId: ownerId,
            boxId: boxId,
            minScore: minScore,
            maxScore: maxScore,
            skip: skip,
            take: take,
            isDescending);
    }

    public async Task<IEnumerable<FriendModel>> GetTypedFriendsAsync(
        FriendViews friendView,
        long ownerId,
        Guid? boxId = null,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true)
    {
        var stopwatch = Stopwatch.StartNew();
        var zsetKey = GetFriendTypeKey(friendView, ownerId, boxId);

        var entries = await Database.SortedSetRangeByScoreWithScoresAsync(
            key: zsetKey,
            start: minScore,
            stop: maxScore,
            exclude: Exclude.None,
            skip: skip,
            take: take,
            order: isDescending ? Order.Descending : Order.Ascending);

        Logger.LogInformation("GetTypedFriendsAsync[{zsetKey}]: friendView={friendView}, ownerId={ownerId}, count={count}, Elapsed:{Elapsed}ms",
            zsetKey,
            friendView,
            ownerId,
            entries.Length,
            stopwatch.ElapsedMilliseconds);

        stopwatch.Restart();

        var scoreMap = new Dictionary<RedisValue, double>();

        if (friendView != FriendViews.All && entries.Length > 0)
        {
            var elements = entries.Select(x => x.Element).ToArray();
            var scores = await GetZsetScoresAsync(OwnerFriendsSetKey(ownerId), elements);
            for (int i = 0; i < elements.Length; i++)
            {
                scoreMap[elements[i]] = scores[i].GetValueOrDefault();
            }
        }

        var result = entries.Select(x =>
        {
            var element = SessionUnitElement.Parse(x.Element);
            var finalScore = friendView == FriendViews.All ? x.Score : scoreMap.GetValueOrDefault(x.Element);
            var score = new FriendScore(finalScore);
            return new FriendModel
            {
                OwnerId = element.OwnerId,
                OwnerObjectType = element.OwnerObjectType,
                DestinationId = element.DestinationId,
                DestinationObjectType = element.DestinationObjectType,
                SessionId = element.SessionId,
                Id = element.SessionUnitId,
                Sorting = score.Sorting,
                Ticks = score.Ticks,
                Score = score,
            };
        });
        Logger.LogInformation("GetTypedFriendsAsync: friendView={friendView}, ownerId={ownerId}, count={count}, Elapsed:{Elapsed}ms",
            friendView,
            ownerId,
            entries.Length,
            stopwatch.ElapsedMilliseconds);
        return result;
    }

    public async Task<long> GetTypedFriendsCountAsync(FriendViews friendView, long ownerId)
    {
        return await Database.SortedSetLengthAsync(GetFriendTypeKey(friendView, ownerId, boxId: null));
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

    public Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetManyAsync(IEnumerable<Guid> unitIds, CancellationToken token = default) => GetManyHashSetAsync<Guid, SessionUnitCacheItem>(unitIds, UnitHashKey, token);

    public async Task<SessionUnitCacheItem> GetAsync(Guid id, CancellationToken token = default) => (await GetManyAsync([id], token)).FirstOrDefault().Value;

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
            if (!fetchedMap.TryGetValue(nullKey, out var fetchedItem) || fetchedItem == null)
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

        var members = (await GetRawMembersAsync(sessionId)).ToList();

        var memberMaps = await BatchGetMembersMapAsync(sessionId, MemberLoad.Immersed | MemberLoad.Creator | MemberLoad.Pinned | MemberLoad.Box);
        var immersedMap = memberMaps.Immersed;
        var creatorMap = memberMaps.Creator;
        var pinnedMap = memberMaps.Pinned;
        var boxMap = memberMaps.Box;

        Logger.LogInformation(
            "[{method}], sessionId:{sessionId}, lastMessageId:{lastMessageId}, count:{count},Elapsed:{ms}ms",
            nameof(GetMembersAsync),
            sessionId,
            lastMessageId,
            members.Count,
            stopwatch.ElapsedMilliseconds);

        if (members == null || members.Count == 0) return;

        stopwatch.Restart();

        var batch = Database.CreateBatch();

        var expireTime = expire ?? CacheExpire;

        foreach (var item in members)
        {
            var element = item.Key;
            var unitId = element.SessionUnitId;
            var ownerId = element.OwnerId;
            var unitKey = UnitHashKey(unitId);

            var ownerStatisticSetKey = OwnerStatisticHashKey(ownerId);
            var isSender = unitId == message.SenderSessionUnitId;
            var isImmersed = immersedMap.TryGetValue(element, out bool immersed) && immersed;
            var isCreator = creatorMap.TryGetValue(element, out bool creator) && creator;
            var sorting = pinnedMap.TryGetValue(element, out var _sorting) ? _sorting : 0;
            var ticks = new DateTimeOffset(message.CreationTime).ToUnixTimeMilliseconds();
            var score = GetFriendScore(sorting, ticks);

            // lastMessageId
            _ = batch.HashSetAsync(unitKey, F_LastMessageId, lastMessageId);
            // ticks
            _ = batch.HashSetAsync(unitKey, F_Ticks, ticks);
            // expire
            _ = batch.KeyExpireAsync(unitKey, expireTime);

            SetOwnerFriends(batch, ownerId, element, score);

            // Sender
            if (isSender)
            {
                // expire
                //LastSendMessageId
                _ = batch.HashSetAsync(unitKey, F_LastSendMessageId, lastMessageId);
                _ = batch.HashSetAsync(unitKey, F_LastSendTime, message.CreationTime.ToRedisValue());
                continue;
            }

            // 消息盒子 boxId
            if (boxMap.TryGetValue(item.Key, out var boxId))
            {
                SetOwnerBoxFriends(batch, ownerId, boxId, element, score);
                ZsetIncrementIfGuardKeyExist(batch, ownerStatisticSetKey, OwnerBoxBadgeZsetKey(ownerId), boxId.ToString(), 1);
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
        // SessionMembers
        _ = batch.KeyExpireAsync(SessionMembersSetKey(sessionId), expireTime ?? CacheExpire);

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

    public async Task<List<SessionUnitOwnerOverviewInfo>> GetOwnerBadgeAsync(List<long> ownerIds)
    {
        if (ownerIds == null || ownerIds.Count == 0)
            return [];

        var batch = Database.CreateBatch();

        // ownerId -> 好友数量 (type -> count)
        var countTaskMap =
            new Dictionary<long, Dictionary<ChatObjectTypeEnums, Task<long>>>();

        // ownerId -> 角标 Hash
        var badgeHashTaskMap =
            new Dictionary<long, Task<HashEntry[]>>();

        // ownerId -> 统计 Hash（如果你还有别的统计）
        var statisticTaskMap =
            new Dictionary<long, Task<HashEntry[]>>();

        foreach (var ownerId in ownerIds)
        {
            // 角标统计（Hash：ChatObjectTypeEnums -> badgeCount）
            badgeHashTaskMap[ownerId] =
                batch.HashGetAllAsync(StatisticMapHashKey(ownerId));

            // 其他统计（如果和角标是同一个 Hash，可删掉这一份）
            statisticTaskMap[ownerId] =
                batch.HashGetAllAsync(OwnerStatisticHashKey(ownerId));

            var typeCountMap =
                new Dictionary<ChatObjectTypeEnums, Task<long>>();

            foreach (var type in Enum.GetValues<ChatObjectTypeEnums>())
            {
                // 好友 / 会话数量（ZSET）
                typeCountMap[type] =
                    batch.SortedSetLengthAsync(
                        OwnerFriendsMapZsetKey(ownerId, type));
            }

            countTaskMap[ownerId] = typeCountMap;
        }
        // 一次性执行 Redis Batch
        batch.Execute();

        await Task.WhenAll(
            badgeHashTaskMap.Values.Cast<Task>()
                .Concat(statisticTaskMap.Values.Cast<Task>())
                .Concat(countTaskMap.Values.SelectMany(x => x.Values).Cast<Task>())
        );
        // 组装结果
        var result = new List<SessionUnitOwnerOverviewInfo>(ownerIds.Count);

        foreach (var ownerId in ownerIds)
        {
            // ---- 角标 ----
            var badgeEntries = await badgeHashTaskMap[ownerId];

            var badgeDict = badgeEntries.ToDictionary(
                x => x.Name.ToEnum<ChatObjectTypeEnums>(),
                x => x.Value.ToLong()
            );

            // ---- 统计 ----
            var statisticEntries = await statisticTaskMap[ownerId];
            var statistic = statisticEntries.Length == 0
                ? null
                : MapToStatistic(statisticEntries); // 你现有的映射方式

            // ---- Types ----
            var types = new List<SessionUnitStatInfo>();

            long totalFriendsCount = 0;

            foreach (var type in Enum.GetValues<ChatObjectTypeEnums>())
            {
                badgeDict.TryGetValue(type, out var badge);
                var typedCount = await countTaskMap[ownerId][type];
                totalFriendsCount += typedCount;
                types.Add(new SessionUnitStatInfo
                {
                    Id = type.ToString(),
                    Name = type.GetDescription(),
                    Count = typedCount,
                    Badge = badge
                });
            }

            result.Add(new SessionUnitOwnerOverviewInfo
            {
                OwnerId = ownerId,
                TotalUnreadCount = statistic?.PublicBadge ?? 0,
                TotalFriendsCount = totalFriendsCount,
                Stat = statistic,
                Types = types,
                Boxes = [] // 你如果后面有 Box 逻辑，从这里补
            });
        }

        return result;
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

    public async Task<SessionUnitCacheItem> UpdateCounterAsync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask)
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
            return unit;
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

        // 减量更新会话盒子
        if (unit.BoxId.HasValue)
        {
            _ = ZsetDecrementIfExistsAndClampZeroAsync(batch, OwnerBoxBadgeZsetKey(unit.OwnerId), unit.BoxId.ToString(), publicBadge, removeWhenZero: false);
        }

        batch.Execute();

        return unit;
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
        var sessionPinnedSortingHashKey = SessionPinnedSortingHashKey(sessionId);
        if (sorting == 0)
        {
            _ = batch.SortedSetRemoveAsync(ownerPinnedBadgeSetKey, element);
            _ = batch.HashDeleteAsync(sessionPinnedSortingHashKey, element);
        }
        else
        {
            _ = batch.SortedSetAddAsync(ownerPinnedBadgeSetKey, element, unit?.PublicBadge ?? 0);
            _ = batch.HashSetAsync(sessionPinnedSortingHashKey, element, sorting);
            _ = batch.KeyExpireAsync(ownerPinnedBadgeSetKey, CacheExpire);
            _ = batch.KeyExpireAsync(sessionPinnedSortingHashKey, CacheExpire);
        }

        // 6.3 更新 ownerSortedSet 的新 score
        _ = batch.SortedSetAddAsync(ownerFriendsSetKey, element, newScore);
        _ = batch.KeyExpireAsync(ownerFriendsSetKey, CacheExpire);

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

            // box
            if (unit.BoxId.HasValue)
            {
                _ = ZsetIncrementIfExistsAndClampZeroAsync(tran, OwnerBoxBadgeZsetKey(unit.OwnerId), unit.BoxId.ToString(), publicDelta);
            }

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
        var sessionImmersedHashKey = SessionImmersedHashKey(unit.SessionId!.Value);
        if (isImmersed)
        {
            _ = tran.SortedSetAddAsync(ownerImmersedSetKey, element, unit.PublicBadge);
            _ = tran.KeyExpireAsync(ownerImmersedSetKey, CacheExpire);

            _ = tran.HashSetAsync(sessionImmersedHashKey, element, unit.PublicBadge);
            _ = tran.KeyExpireAsync(sessionImmersedHashKey, CacheExpire);
        }
        else
        {
            _ = tran.SortedSetRemoveAsync(ownerImmersedSetKey, element);
            _ = tran.HashDeleteAsync(sessionImmersedHashKey, element);
        }

        var committed = await tran.ExecuteAsync();

        if (!committed)
        {
            Logger.LogWarning("Redis transaction failed, unitId:{unitId}", unitId);
            return;
        }

        unit.IsImmersed = isImmersed;
    }

    public async Task ChangeBoxAsync(Guid unitId, Guid? boxId)
    {
        var unit = await GetAsync(unitId);

        if (unit == null)
        {
            Logger.LogWarning("unit is null,unitId:{unitId}", unitId);
            return;
        }

        if (unit.BoxId == boxId)
        {
            Logger.LogWarning("Unchanged boxId:{boxId}", boxId);
            return;
        }
        var ownerId = unit.OwnerId;

        var ownerBoxBadgeKey = OwnerBoxBadgeZsetKey(ownerId);
        var element = GetElement(unit);
        var score = GetFriendScore(unit.Sorting, unit.Ticks);
        var isImmersed = unit.IsImmersed;

        var tran = Database.CreateTransaction(ownerId);

        // old boxId
        if (unit.BoxId.HasValue)
        {
            var oldOwnerBoxFriendsSetKey = OwnerBoxFriendsSetKey(ownerId, unit.BoxId.Value);
            _ = tran.SortedSetRemoveAsync(oldOwnerBoxFriendsSetKey, element);
            if (!isImmersed && unit.PublicBadge > 0)
            {
                _ = ZsetIncrementIfExistsAndClampZeroAsync(tran, ownerBoxBadgeKey, unit.BoxId.ToString(), -unit.PublicBadge);
            }
        }

        // new boxId
        if (boxId.HasValue)
        {
            var newOwnerBoxFriendsSetKey = OwnerBoxFriendsSetKey(ownerId, boxId.Value);
            _ = tran.SortedSetAddAsync(newOwnerBoxFriendsSetKey, element, score);
            if (!isImmersed && unit.PublicBadge > 0)
            {
                _ = tran.SortedSetIncrementAsync(ownerBoxBadgeKey, boxId.ToString(), unit.PublicBadge);
            }
        }

        // session box
        if (unit.SessionId.HasValue)
        {
            if (boxId.HasValue)
            {
                _ = tran.HashSetAsync(SessionBoxHashKey(unit.SessionId.Value), element, boxId.ToString());
            }
            else
            {
                _ = tran.HashDeleteAsync(SessionBoxHashKey(unit.SessionId.Value), element);
            }
        }

        // unit
        _ = tran.HashSetAsync(UnitHashKey(unitId), F_BoxId, boxId.ToString());

        var committed = await tran.ExecuteAsync();

        if (!committed)
        {
            Logger.LogWarning("Redis transaction failed, unitId:{unitId}", unitId);
            return;
        }

        unit.BoxId = boxId;
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
        _ = batch.KeyExpireAsync(ownerStatisticHashKey, CacheExpire, when: ExpireWhen.Always);

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
        foreach (var item in friends)
        {
            _ = HashSetIfFieldExistsAsync(batch, SessionImmersedHashKey(item.Key.SessionId), item.Key, 0);
        }


        // 更新会话盒子
        var ownerBoxBadgeZsetKey = OwnerBoxBadgeZsetKey(ownerId);
        var entries = await Database.SortedSetRangeByRankWithScoresAsync(ownerBoxBadgeZsetKey);

        foreach (var entry in entries)
        {
            var boxId = entry.Element.ToGuid();
            _ = batch.SortedSetAddAsync(ownerBoxBadgeZsetKey, boxId.ToString(), 0, SortedSetWhen.Exists);
        }

        batch.Execute();

        Logger.LogInformation("{method} ownerId:{ownerId}, [END] Elapsed:{elapsed}ms",
            nameof(ClearBadgeAsync),
            ownerId,
            stopwatch.ElapsedMilliseconds);

        return true;
    }

    #endregion

    /// <summary>
    /// 获取盒子角标
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<KeyValuePair<Guid, double>>> GetBoxFriendsBadgeMapAsync(long ownerId)
    {
        return (await GetBoxFriendsBadgeAsync([ownerId])).GetValueOrDefault(ownerId);
    }
    /// <summary>
    /// 获取盒子角标
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <returns></returns>
    public async Task<Dictionary<long, IEnumerable<KeyValuePair<Guid, double>>>> GetBoxFriendsBadgeAsync(List<long> ownerIds)
    {
        var batch = Database.CreateBatch();

        var badgeTaskMap = ownerIds
            .Distinct()
            .ToDictionary(
                x => x,
                x => batch.SortedSetRangeByRankWithScoresAsync(OwnerBoxBadgeZsetKey(x))
            );

        batch.Execute();

        await Task.WhenAll(badgeTaskMap.Values);

        return badgeTaskMap.ToDictionary(
            x => x.Key,
            x => x.Value.Result.Select(x => new KeyValuePair<Guid, double>(x.Element.ToGuid(), x.Score))
         );
    }

    /// <summary>
    /// 获取盒子角标与好友数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<SessionUnitStatInfo>> GetBoxBadgeInfoAsync(long ownerId)
    {
        return (await GetBoxBadgeInfoMapAsync([ownerId])).GetValueOrDefault(ownerId) ?? [];
    }
    /// <summary>
    /// 获取盒子角标与好友数量
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <returns></returns>

    public async Task<Dictionary<long, IEnumerable<SessionUnitStatInfo>>> GetBoxBadgeInfoMapAsync(List<long> ownerIds)
    {
        var boxFriendsBadgeMap = await GetBoxFriendsBadgeAsync(ownerIds);

        if (boxFriendsBadgeMap.Count == 0)
        {
            return [];
        }

        var batch = Database.CreateBatch();

        var countTaskMap = boxFriendsBadgeMap.ToDictionary(
            x => x.Key,
            x => x.Value.ToDictionary(v => v.Key, v => batch.SortedSetLengthAsync(OwnerBoxFriendsSetKey(x.Key, v.Key)))
        );

        batch.Execute();

        await Task.WhenAll(countTaskMap.SelectMany(x => x.Value.Values));

        return boxFriendsBadgeMap.ToDictionary(
              x => x.Key,
              x => x.Value.Select(v =>
              {
                  var count =
                      countTaskMap
                          .GetValueOrDefault(x.Key)?
                          .GetValueOrDefault(v.Key)?
                          .Result ?? 0;

                  return new SessionUnitStatInfo
                  {
                      Id = v.Key.ToString(),
                      Name = null, // 可以根据 boxId 去加载名称
                      Badge = (long)v.Value,
                      Count = count
                  };
              })
         );
    }

    /// <summary>
    /// 添加会话单元缓存
    /// </summary>
    /// <param name="units"></param>
    /// <returns></returns>
    public async Task AddUnitsAsync(IEnumerable<SessionUnitCacheItem> units)
    {
        await Task.Yield();
        var batch = Database.CreateBatch();

        //unit
        foreach (var unit in units)
        {
            SetUnit(batch, unit, refreshExpire: true);
        }

        // owner
        foreach (var items in units.GroupBy(x => x.OwnerId))
        {
            var ctx = BuildOwnerStatistics(batch, items.Key, items);
        }

        //session 
        foreach (var items in units.Where(x => x.SessionId.HasValue).GroupBy(x => x.SessionId))
        {
            BuildSessionMembers(batch, items.Key.Value, items.ToList());
        }
        batch.Execute();
    }
}
