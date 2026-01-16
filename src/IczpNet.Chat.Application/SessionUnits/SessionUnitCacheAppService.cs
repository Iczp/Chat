using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Clocks;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pipelines.Sockets.Unofficial.Buffers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Users;


namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话单元
/// </summary>
public class SessionUnitCacheAppService(
    IMessageManager messageManager,
    IMessageRepository messageRepository,
    ISessionUnitSettingManager sessionUnitSettingManager,
    ISessionUnitRepository sessionUnitRepository,
    IFollowManager followManager,
    IOnlineManager onlineManager,
    IDistributedCache<SessionUnitSearchCacheItem, SessionUnitSearchCacheKey> searchCache,
    ISessionUnitCacheManager sessionUnitCacheManager) : ChatAppService, ISessionUnitCacheAppService
{
    public IMessageManager MessageManager { get; } = messageManager;
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ISessionUnitSettingManager SessionUnitSettingManager { get; } = sessionUnitSettingManager;
    public ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    public IFollowManager FollowManager { get; } = followManager;
    public IOnlineManager OnlineManager { get; } = onlineManager;
    public IDistributedCache<SessionUnitSearchCacheItem, SessionUnitSearchCacheKey> SearchCache { get; } = searchCache;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;



    protected override string GetListPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected override string GetPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected virtual string GetDetailPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected virtual string GetListForSameSessionPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetSameSession;
    protected virtual string GetItemForSameSessionPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetSameSession;
    protected virtual string GetBadgePolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetBadge;
    protected virtual string FindPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.Find;
    protected virtual string GetCounterPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetCounter;

    protected virtual Task<bool> ShouldLoadAllAsync(SessionUnitCacheItemGetListInput input)
    {
        // 排序字段中是否包含 Setting.* 或 Destination.*
        var sortingFields = input.Sorting?
            .Trim()
            .Split(",")
            .Select(x => x.Trim().Split(" ")[0]) ?? [];

        var needLoadBySorting = sortingFields.Any(f =>
               //f.StartsWith($"{nameof(SessionUnitCacheDto.Setting)}.") || 
               f.StartsWith($"{nameof(SessionUnitFriendDto.Destination)}.")
        );

        return Task.FromResult(
               !string.IsNullOrWhiteSpace(input.Keyword)  // 搜索 Keyword 时一定需要全量加载
            || needLoadBySorting
        );
    }

    protected virtual async Task LoadFriendsIfNotExistsAsync(long ownerId)
    {
        await SessionUnitManager.LoadFriendsIfNotExistsAsync(ownerId);
    }
    protected virtual async Task LoadMembersIfNotExistsAsync(Guid sessionId)
    {
        await SessionUnitManager.LoadMembersIfNotExistsAsync(sessionId);
    }

    protected virtual async Task<IEnumerable<SessionUnitCacheItem>> GetAllListAsync(long ownerId)
    {
        await LoadFriendsIfNotExistsAsync(ownerId);
        return await SessionUnitCacheManager.GetFriendUnitsAsync(ownerId);
    }

    protected Task<SessionUnitCacheItem> MapToCacheItemAsync(SessionUnit entity)
    {
        return Task.FromResult(MapToCacheItem(entity));
    }

    protected virtual SessionUnitCacheItem MapToCacheItem(SessionUnit entity)
    {
        return ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(entity);
    }

    protected virtual SessionUnitFriendDto MapToDto(SessionUnitCacheItem item)
    {
        return ObjectMapper.Map<SessionUnitCacheItem, SessionUnitFriendDto>(item);
    }

    protected virtual async Task<IQueryable<SessionUnitFriendDto>> CreateQueryableAsync(SessionUnitCacheItemGetListInput input)
    {
        var allList = await GetAllListAsync(input.OwnerId);

        var result = allList
            .Select(MapToDto)
            .ToList()
            .AsQueryable();

        var shouldLoadAll = await ShouldLoadAllAsync(input);

        if (shouldLoadAll)
        {
            //var allUnitIds = result.Select(x => x.MessageId).Distinct().ToList();
            //var settingMap = (await SessionUnitSettingManager.GetManyCacheAsync(allUnitIds))
            //    .ToDictionary(x => x.Key, x => x.Value);

            var allDestIds = result
                .Where(x => x.DestinationId.HasValue)
                .Select(x => x.DestinationId.Value)
                .Distinct()
                .ToList();

            var allDestList = await ChatObjectManager.GetManyByCacheAsync(allDestIds);

            var destMap = allDestList.ToDictionary(x => x.Id, x => x);

            //  提前填充
            foreach (var item in result)
            {
                //item.Setting = settingMap.GetValueOrDefault(item.MessageId);
                item.Destination = item.DestinationId.HasValue
                    ? destMap.GetValueOrDefault(item.DestinationId.Value)
                    : null;
                item.SearchText = string.Join(" ",
                    item.Destination?.Name ?? ""
                //item.Setting?.MemberName ?? "",
                //item.Setting?.Rename ?? ""
                ).Replace("  ", " ").ToLower();
            }
        }
        return result;
    }

    private async Task FillLastMessageAsync(IEnumerable<SessionUnitFriendDto> items)
    {
        // fill Setting
        var messageIdList = items
            .Where(x => x.LastMessageId.HasValue)
            .Select(x => x.LastMessageId.Value)
            .ToList();
        if (messageIdList.Count == 0)
        {
            return;
        }
        var messages = await MessageManager.GetOrAddManyCacheAsync(messageIdList);

        var messageMap = messages.ToDictionary(x => x.Key.MessageId, x => x.Value);

        foreach (var item in items)
        {
            item.LastMessage = item.LastMessageId.HasValue ? messageMap.GetValueOrDefault(item.LastMessageId.Value) : null;
        }

        //var messages = await MessageRepository.GetListAsync(x => messageIdList.Contains(x.Id));
        //var messageMap = messages
        //    .Select(ObjectMapper.Map<Message, MessageOwnerDto>)
        //    .ToDictionary(x => x.Id, x => x);
        //foreach (var item in items)
        //{
        //    item.LastMessage = item.LastMessageId.HasValue ? messageMap.GetValueOrDefault(item.LastMessageId.Value) : null;
        //}
    }

    private async Task FillSettingAsync(IEnumerable<SessionUnitFriendDto> items)
    {
        //return;
        // fill Setting
        var nullSettingsItems = items
            //.Where(x => x.Setting == null)
            .ToList();
        if (nullSettingsItems.Count == 0)
        {
            return;
        }
        var unitIds = nullSettingsItems.Select(x => x.Id).Distinct().ToList();
        var settingMap = (await SessionUnitSettingManager.GetOrAddManyCacheAsync(unitIds))
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var item in nullSettingsItems)
        {
            item.Setting = settingMap.GetValueOrDefault(item.Id);
        }
    }

    private async Task FillDestinationAsync(IEnumerable<SessionUnitFriendDto> items)
    {
        // fill Destination
        var nulltems = items
            .Where(x => x.DestinationId.HasValue && x.Destination == null)
            .ToList();
        if (nulltems.Count == 0)
        {
            return;
        }
        var destIds = nulltems.Select(x => x.DestinationId.Value).Distinct().ToList();
        var destMap = (await ChatObjectManager.GetManyByCacheAsync(destIds))
            .ToDictionary(x => x.Id, x => x);

        foreach (var item in nulltems)
        {
            item.Destination = destMap.GetValueOrDefault(item.DestinationId.Value);
        }
    }
    private async Task FillOwnerAsync(IEnumerable<SessionUnitMemberDto> items)
    {
        // fill Destination
        var nulltems = items
            .Where(x => x.Owner == null)
            .ToList();
        if (nulltems.Count == 0)
        {
            return;
        }
        var idlist = nulltems.Select(x => x.OwnerId).Distinct().ToList();
        var destMap = (await ChatObjectManager.GetManyByCacheAsync(idlist))
            .ToDictionary(x => x.Id, x => x);

        foreach (var item in nulltems)
        {
            item.Owner = destMap.GetValueOrDefault(item.OwnerId);
        }
    }

    private async Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetCacheManyAsync(List<Guid> unitIds)
    {
        return await SessionUnitCacheManager.GetOrSetManyAsync(unitIds, async (keys) =>
           {
               var kvs = await SessionUnitManager.GetManyAsync(keys);

               var cacheItems = kvs.Select(x => MapToCacheItem(x.Value)).ToList();

               var arr = new KeyValuePair<Guid, SessionUnitCacheItem>[cacheItems.Count];

               for (int i = 0; i < cacheItems.Count; i++)
               {
                   arr[i] = new KeyValuePair<Guid, SessionUnitCacheItem>(cacheItems[i].Id, cacheItems[i]);
               }
               return arr;
           });
    }

    private async Task<SessionUnitCacheItem> GetCacheAsync(Guid unitId)
    {
        var list = await GetCacheManyAsync([unitId]);
        var unit = list.FirstOrDefault().Value;
        Assert.If(unit == null, $"No such cache id:{unitId}");
        return unit;
    }


    protected virtual async IAsyncEnumerable<Guid> BatchSearchAsync(
        long ownerId,
        string keyword,
        IReadOnlyCollection<long> searchChatObjectIdList,
        IReadOnlyCollection<Guid> allUnitIds,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var totalCount = 0;
        var batchIndex = 0;
        var method = nameof(BatchSearchAsync);

        Logger.LogInformation(
            "[{method}] Start | OwnerId={OwnerId}, batchSize={batchSize}, Keyword={Keyword}",
            method,
            ownerId,
            batchSize,
            keyword);

        var baseQuery = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == ownerId)
            .Where(SessionUnit.GetActivePredicate(Clock.Now))
            .Where(x =>
                searchChatObjectIdList.Contains(x.DestinationId!.Value)
                || x.Setting.Rename.Contains(keyword)
                || x.Setting.RenameSpellingAbbreviation.Contains(keyword)
                || x.Setting.RenameSpelling.Contains(keyword))
            .Where(x => allUnitIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.CreationTime
            });

        DateTime? lastTime = null;
        Guid? lastId = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batchStopwatch = Stopwatch.StartNew();

            var query = baseQuery;

            if (lastTime.HasValue)
            {
                query = query.Where(x =>
                    x.CreationTime > lastTime.Value ||
                    (x.CreationTime == lastTime.Value &&
                     x.Id.CompareTo(lastId!.Value) > 0));
            }

            var batch = await query
                .OrderBy(x => x.CreationTime)
                .ThenBy(x => x.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            batchStopwatch.Stop();

            if (batch.Count == 0)
                break;

            batchIndex++;
            totalCount += batch.Count;

            foreach (var item in batch)
                yield return item.Id;

            var last = batch[^1];
            lastTime = last.CreationTime;
            lastId = last.Id;

            Logger.LogDebug(
                "[{method}] Batch#{Batch} Count={Count}, batchSize={batchSize}, Elapsed={Elapsed}ms",
                method,
                batchIndex,
                batch.Count,
                batchSize,
                batchStopwatch.ElapsedMilliseconds);
        }

        Logger.LogInformation(
            "[{method}] Completed | Total={Total}, Batches={Batches}, batchSize={batchSize}, Elapsed={Elapsed}ms",
            method,
            totalCount,
            batchIndex,
            batchSize,
            totalStopwatch.ElapsedMilliseconds);
    }

    private static async Task<IQueryable<MemberModel>> ApplyMemberFilterAsync(IQueryable<MemberModel> query, Guid sessionId, string keyword)
    {
        return query;

    }

    /// <summary>
    /// 查询/过滤
    /// </summary>
    /// <param name="query"></param>
    /// <param name="ownerId"></param>
    /// <param name="keyword"></param>
    /// <returns></returns>
    private async Task<IQueryable<FriendModel>> ApplyFriendFilterAsync(IQueryable<FriendModel> query, long ownerId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return query;
        }
        var allUnitIds = query.Select(x => x.Id).ToList();

        if (allUnitIds.Count == 0)
        {
            return null;
        }

        var searchChatObjectIdList = await ChatObjectManager.SearchKeywordByCacheAsync(keyword);

        if (searchChatObjectIdList.Count == 0)
        {
            return null;
        }

        var searchResult = await SearchCache.GetOrAddAsync(new SessionUnitSearchCacheKey(ownerId, keyword), async () =>
        {

            var ids = new List<Guid>();

            await foreach (var id in BatchSearchAsync(
                ownerId,
                keyword,
                searchChatObjectIdList,
                allUnitIds,
                batchSize: 200))
            {
                ids.Add(id);
            }

            return new SessionUnitSearchCacheItem(ids);

            //var querySearch = (await SessionUnitRepository.GetQueryableAsync())
            //.Where(x => x.OwnerId == ownerId)
            //.Where(SessionUnit.GetActivePredicate(Clock.Now))
            //.Where(x => searchChatObjectIdList.Contains(x.DestinationId.Value)
            //    || x.Setting.Rename.Contains(keyword)
            //    || x.Setting.RenameSpellingAbbreviation.Contains(keyword)
            //    || x.Setting.RenameSpelling.Contains(keyword))
            //.Where(x => allUnitIds.Contains(x.MessageId))
            //;
            //var searchUnitIds = querySearch.Select(x => x.MessageId).ToList();
            //return new SessionUnitSearchCacheItem(searchUnitIds);
        });

        var searchUnitIds = searchResult.UnitIds;

        if (searchUnitIds.Count == 0)
        {
            return null;
        }

        // 加入查询条件
        query = query.WhereIf(searchUnitIds.Count > 0, x => searchUnitIds.Contains(x.Id));

        return query;
    }


    protected virtual SessionUnitMemberDto MapToMemberDto(SessionUnitCacheItem item)
    {
        return ObjectMapper.Map<SessionUnitCacheItem, SessionUnitMemberDto>(item);
    }

    protected virtual async Task<BadgeDto> GetBadgeInternalAsync(long ownerId)
    {
        var owner = await ChatObjectManager.GetItemByCacheAsync(ownerId);
        //加载全部
        await LoadFriendsIfNotExistsAsync(ownerId);

        return new BadgeDto()
        {
            AppUserId = owner.AppUserId,
            ChatObjectId = ownerId,
            Statistic = await SessionUnitCacheManager.GetStatisticAsync(ownerId),
            BadgeMap = await SessionUnitCacheManager.GetRawBadgeMapAsync(ownerId),
            CountMap = await SessionUnitCacheManager.GetFriendsCountMapAsync(ownerId),
        };
    }

    public async Task<PagedResultDto<SessionUnitFriendDto>> GetListAsync(SessionUnitCacheItemGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        var queryable = await CreateQueryableAsync(input);

        var baseQuery = queryable
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.DestinationObjectType == input.DestinationObjectType)
            .WhereIf(input.MinMessageId.HasValue, x => x.LastMessageId >= input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.LastMessageId < input.MaxMessageId)
            .WhereIf(input.MinTicks.HasValue, x => x.Ticks >= input.MinTicks)
            .WhereIf(input.MaxTicks.HasValue, x => x.Ticks < input.MaxTicks)
            .WhereIf(input.IsBadge.HasValue, x => x.PublicBadge > 0)
            //@我、@所有人
            .WhereIf(input.IsRemind == true, x => (x.RemindAllCount + x.RemindMeCount) > 0)
            .WhereIf(input.IsRemind == false, x => (x.RemindAllCount + x.RemindMeCount) == 0)
            //搜索
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.SearchText.Contains(input.Keyword.ToLower()))
            ;

        var pagedList = await GetPagedListAsync(baseQuery, input);

        // 分页后再按需加载 Setting & Destination
        await FillSettingAsync(pagedList.Items);

        await FillDestinationAsync(pagedList.Items);

        await FillLastMessageAsync(pagedList.Items);

        return pagedList;
    }

    /// <summary>
    /// 获取会话（多个）
    /// </summary>
    /// <param name="unitIds"></param>
    /// <returns></returns>
    public async Task<List<SessionUnitFriendDto>> GetManyAsync(List<Guid> unitIds)
    {
        var list = await GetCacheManyAsync(unitIds);

        // check owner
        var chatObjectIdList = list.Select(x => x.Value).Select(x => x.OwnerId).Distinct().ToList();
        await CheckPolicyForUserAsync(chatObjectIdList, () => CheckPolicyAsync(GetListPolicyName));

        var items = list.Select(x => x.Value)
            .Where(x => x != null)
            .Select(MapToDto)
            .ToList();

        await FillSettingAsync(items);

        await FillDestinationAsync(items);

        await FillLastMessageAsync(items);

        return items;
    }

    /// <summary>
    /// 获取会话
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SessionUnitFriendDto> GetAsync(Guid id)
    {
        var items = await GetManyAsync([id]);

        var item = items.FirstOrDefault();

        Assert.If(item == null, $"No such cache id:{id}");

        return item;
    }


    /// <summary>
    /// 获取最新消息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitFriendDto>> GetLatestAsync(SessionUnitLatestGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        Assert.If(input.MaxResultCount > 100, $"params:{nameof(input.MaxResultCount)} max value: 100");

        var minScore = input.MinScore ?? 0;

        var queryable = await SessionUnitCacheManager.GetFriendsAsync(
            input.OwnerId,
            minScore: minScore,
            skip: input.SkipCount,
            take: Math.Min(input.MaxResultCount, 100));

        var unitIds = queryable
            .Where(x => x.Ticks > minScore)
            .OrderByDescending(x => x.Ticks)
            .Select(x => x.Id)
            .ToList();

        var items = await GetManyAsync(unitIds);

        return new PagedResultDto<SessionUnitFriendDto>(items.Count, items);

    }

    /// <summary>
    /// 获取好友会话
    /// </summary>
    public async Task<PagedResultDto<SessionUnitFriendDto>> GetFriendsAsync(SessionUnitFirendGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        //加载全部
        await LoadFriendsIfNotExistsAsync(input.OwnerId);

        IEnumerable<long> onlineFriendIds = [];
        //是否在线
        if (input.IsOnline.HasValue)
        {
            var online = await OnlineManager.GetOnlineFriendsAsync(input.OwnerId);
            onlineFriendIds = online.Select(x => x.OwnerId).Distinct();
        }

        var queryable = await SessionUnitCacheManager.GetTypedFriendsAsync(input.FriendType, input.OwnerId,
            //minScore: input.MinScore ?? double.NegativeInfinity,
            //maxScore: input.MaxScore ?? double.PositiveInfinity,
            //skip: input.SkipCount,
            //take: input.MaxResultCount,
            isDescending: true);


        var query = queryable.AsQueryable()
            .WhereIf(input.FriendId.HasValue, x => x.FriendId == input.FriendId)
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.UnitId.HasValue, x => x.Id == input.UnitId)
            .WhereIf(input.MinScore > 0, x => x.Ticks > input.MinScore)
            .WhereIf(input.MaxScore > 0, x => x.Ticks < input.MaxScore)
            .WhereIf(input.IsOnline == true, x => onlineFriendIds.Contains(x.FriendId))
            .WhereIf(input.IsOnline == false, x => !onlineFriendIds.Contains(x.FriendId))
            ;

        //search 
        query = await ApplyFriendFilterAsync(query, input.OwnerId, input.Keyword);

        if (query == null)
        {
            return new PagedResultDto<SessionUnitFriendDto>(0, []);
        }

        var totalCount = query.Count(); //kvs.Length
        //var totalCount = await SessionUnitCacheManager.GetFirendsCountAsync(ownerId);

        // sorting
        query = query
            .OrderByDescending(x => x.Sorting)
            .ThenByDescending(x => x.Ticks)
            ;

        //paged
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var unitIds = query.Select(x => x.Id).ToList();

        var items = await GetManyAsync(unitIds);

        return new PagedResultDto<SessionUnitFriendDto>(totalCount, items);
    }

    /// <summary>
    /// 获取好友数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<FriendCountDto> GetFriendsCountAsync([Required] long ownerId)
    {
        await LoadFriendsIfNotExistsAsync(ownerId);
        return new FriendCountDto()
        {
            TotalCount = await SessionUnitCacheManager.GetFriendsCountAsync(ownerId),
            CountMap = await SessionUnitCacheManager.GetFriendsCountMapAsync(ownerId),
        };
    }

    /// <summary>
    /// 获取会话成员
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitMemberDto>> GetMembersAsync(SessionUnitMemberGetListInput input)
    {
        var unit = await GetCacheAsync(input.UnitId);
        var ownerId = unit.OwnerId;
        var sessionId = unit.SessionId.Value;
        // check owner
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName, ownerId));

        //加载全部
        await LoadMembersIfNotExistsAsync(sessionId);

        var queryable = await SessionUnitCacheManager.GetMembersAsync(sessionId);

        var query = queryable.AsQueryable()
            .WhereIf(input.MinScore > 0, x => x.CreationTime.ToUnixTimeMilliseconds() > input.MinScore)
            .WhereIf(input.MaxScore > 0, x => x.CreationTime.ToUnixTimeMilliseconds() < input.MaxScore)
            .WhereIf(input.IsCreator.HasValue, x => x.IsCreator == input.IsCreator.Value)
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId.Value)
            ;

        //search 
        query = await ApplyMemberFilterAsync(query, sessionId, input.Keyword);

        if (query == null)
        {
            return new PagedResultDto<SessionUnitMemberDto>(0, []);
        }

        var totalCount = query.Count(); //kvs.Length
        //var totalCount = await SessionUnitCacheManager.GetTotalCountByOwnerAsync(ownerId);

        // sorting
        query = query
            .OrderByDescending(x => x.IsCreator)
            .ThenByDescending(x => x.CreationTime)
            ;

        // paged
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var unitIds = query.Select(x => x.Id).ToList();

        var list = await GetCacheManyAsync(unitIds);

        var items = list
            .Select(x => x.Value)
            .Select(MapToMemberDto)
            .ToList();

        await FillOwnerAsync(items);

        return new PagedResultDto<SessionUnitMemberDto>(totalCount, items);
    }

    /// <summary>
    /// 获取会话成员数量
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task<MemberCountDto> GetMembersCountAsync(Guid unitId)
    {
        var unit = await GetCacheAsync(unitId);
        var sessionId = unit.SessionId.Value;
        await LoadMembersIfNotExistsAsync(sessionId);
        return new MemberCountDto()
        {
            SessionId = sessionId,
            TotalCount = await SessionUnitCacheManager.GetMembersCountAsync(sessionId),
        };
    }

    /// <summary>
    /// 聊天对象角标总数
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<BadgeDto> GetBadgeAsync(long ownerId)
    {
        // check owner
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName, ownerId));
        return await GetBadgeInternalAsync(ownerId);
    }

    /// <summary>
    /// 用户聊天对象角标列表
    /// </summary>
    /// <param name="userId"></param>
    public async Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(userId);

        await CheckPolicyForUserAsync(chatObjectIdList, () => CheckPolicyAsync(GetBadgePolicyName));

        var result = new List<BadgeDto>();

        foreach (var ownerId in chatObjectIdList)
        {
            result.Add(await GetBadgeInternalAsync(ownerId));
        }

        return result;
    }

    /// <summary>
    /// 登录用户聊天对象角标列表
    /// </summary>
    /// <returns></returns>
    public Task<List<BadgeDto>> GetBadgeByCurrentUserAsync()
    {
        return GetBadgeByUserIdAsync(CurrentUser.GetId());
    }

}
