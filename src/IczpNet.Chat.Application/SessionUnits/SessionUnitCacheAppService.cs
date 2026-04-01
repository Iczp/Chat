using CommunityToolkit.HighPerformance;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Dtos;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Clocks;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionBoxes;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUglify;
using Pipelines.Sockets.Unofficial.Buffers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
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
    IBoxManager boxManager,
    ISessionTagManager sessionTagManager,
    ISessionUnitFriendshipMapper sessionUnitFriendshipMapper,
    ISessionUnitCacheManager sessionUnitCacheManager) : ChatAppService, ISessionUnitCacheAppService
{


    public IMessageManager MessageManager { get; } = messageManager;
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ISessionUnitSettingManager SessionUnitSettingManager { get; } = sessionUnitSettingManager;
    public ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    public IFollowManager FollowManager { get; } = followManager;
    public IOnlineManager OnlineManager { get; } = onlineManager;
    public IDistributedCache<SessionUnitSearchCacheItem, SessionUnitSearchCacheKey> SearchCache { get; } = searchCache;
    public IBoxManager BoxManager { get; } = boxManager;
    public ISessionTagManager SessionTagManager { get; } = sessionTagManager;
    public ISessionUnitFriendshipMapper SessionUnitFriendshipMapper { get; } = sessionUnitFriendshipMapper;
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

    protected virtual async Task LoadFriendsAsync(long ownerId)
    {
        await SessionUnitManager.LoadFriendsIfNotExistsAsync(ownerId);
    }
    protected virtual async Task LoadMembersAsync(Guid sessionId)
    {
        await SessionUnitManager.LoadMembersIfNotExistsAsync(sessionId);
    }

    protected virtual async Task<IEnumerable<SessionUnitCacheItem>> GetAllListAsync(long ownerId)
    {
        await LoadFriendsAsync(ownerId);
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

    protected virtual SessionUnitFriendDetailDto MapToFriendDetailDto(SessionUnitCacheItem item)
    {
        return ObjectMapper.Map<SessionUnitCacheItem, SessionUnitFriendDetailDto>(item);
    }

    protected virtual SessionUnitMemberDetailDto MapToMemberDetailDto(SessionUnitCacheItem item)
    {
        return ObjectMapper.Map<SessionUnitCacheItem, SessionUnitMemberDetailDto>(item);
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
            .Distinct()
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
    private async Task FillSessionTagAsync(IEnumerable<SessionUnitMemberDto> items)
    {
        var unitIds = items.Select(x => x.Id).Distinct().ToList();
        if (unitIds.Count == 0)
        {
            return;
        }
        var tagMap = await SessionTagManager.GetSessionUnitTagMapAsync(unitIds);

        foreach (var item in items)
        {
            item.TagList = tagMap.GetValueOrDefault(item.Id) ?? [];
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
        //return new SessionUnitDetailDto()
        //{

        //};

        return ObjectMapper.Map<SessionUnitCacheItem, SessionUnitMemberDto>(item);
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
    /// <param name="unitIds">SessionUnitId</param>
    /// <returns></returns>
    public async Task<List<SessionUnitFriendDto>> GetManyAsync(List<Guid> unitIds)
    {
        var list = await GetCacheManyAsync(unitIds);

        // check owner
        var chatObjectIdList = list
            .Select(x => x.Value)
            .Where(x => x != null)
            .Select(x => x.OwnerId)
            .Distinct()
            .ToList();

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
    /// <param name="id">SessionUnitId</param>
    /// <returns></returns>
    public async Task<SessionUnitFriendDto> GetAsync(Guid id)
    {
        var items = await GetManyAsync([id]);

        var item = items.FirstOrDefault();

        Assert.If(item == null, $"No such cache id:{id}");

        return item;
    }

    /// <summary>
    /// 获取好友信息
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public async Task<SessionUnitFriendDetailDto> GetFriendAsync(Guid unitId)
    {
        var unit = await GetCacheAsync(unitId);

        await CheckPolicyForUserAsync([unit.OwnerId], () => CheckPolicyAsync(GetListPolicyName));

        var allIds = new List<long?>() { unit.OwnerId, unit.DestinationId }
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Distinct()
            .ToList();

        var chatObjectMap = (await ChatObjectManager.GetManyByCacheAsync(allIds))
            .ToDictionary(x => x.Id, x => x);

        var item = MapToFriendDetailDto(unit);

        //
        await LoadMembersAsync(unit.SessionId.Value);

        item.SessionUnitCount = await SessionUnitCacheManager.GetMembersCountAsync(unit.SessionId.Value);
        item.Owner = chatObjectMap.GetValueOrDefault(item.OwnerId);
        item.Destination = item.DestinationId.HasValue ? chatObjectMap.GetValueOrDefault(item.DestinationId.Value) : null;
        item.Setting = await SessionUnitSettingManager.GetOrAddCacheAsync(unit.Id);

        return item;
    }


    /// <summary>
    /// 获取最新消息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitFriendDto>> GetChangesAsync(SessionUnitChangesGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        Assert.If(input.MaxResultCount > 100, $"params:{nameof(input.MaxResultCount)} max value: 100");

        var queryable = await SessionUnitCacheManager.GetFriendsAsync(
            input.OwnerId,
            //minScore: input.MinTicks ?? 0,
            skip: input.SkipCount,
            take: Math.Min(input.MaxResultCount, 100));

        var query = queryable
            .WhereIf(input.MinTicks > 0, x => x.Ticks > input.MinTicks)
            .WhereIf(input.MaxTicks > 0, x => x.Ticks < input.MaxTicks)
            .OrderByDescending(x => x.Ticks);

        var items = await GetManyWithScoreAsync(query);

        return new PagedResultDto<SessionUnitFriendDto>(items.Count, items);
    }

    /// <summary>
    /// 获取好友会话
    /// </summary>
    public async Task<ExtraPagedResultDto<SessionUnitFriendDto>> GetFriendsAsync(SessionUnitFirendGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        //加载全部
        await LoadFriendsAsync(input.OwnerId);

        IEnumerable<long> onlineFriendIds = [];

        var stopwatch = Stopwatch.StartNew();

        var queryable = await SessionUnitCacheManager.GetTypedFriendsAsync(
            input.View,
            input.OwnerId,
            input.BoxId,
            //minScore: input.MinScore ?? double.NegativeInfinity,
            //maxScore: input.MaxScore ?? double.PositiveInfinity,
            //skip: input.SkipCount,
            //take: input.MaxResultCount,
            isDescending: true);


        //是否在线
        if (input.IsOnline.HasValue)
        {
            var firendIds = queryable.Select(x => x.DestinationId);
            var onlineMap = await OnlineManager.IsOnlineAsync(firendIds);
            onlineFriendIds = onlineMap.Where(x => x.Value).Select(x => x.Key).Distinct();
        }

        var query = queryable.AsQueryable()
            .WhereIf(input.FriendType.HasValue, x => x.DestinationObjectType == input.FriendType)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.SessionId.HasValue, x => x.SessionId == input.SessionId)
            .WhereIf(input.SessionUnitId.HasValue, x => x.Id == input.SessionUnitId)
            .WhereIf(input.MinScore > 0, x => x.Score > input.MinScore)
            .WhereIf(input.MaxScore > 0, x => x.Score < input.MaxScore || (
                x.Score == input.MaxScore &&
                x.Id.CompareTo(input.CursorId) < 0
            ))
            .WhereIf(input.MinTicks > 0, x => x.Ticks > input.MinTicks)
            .WhereIf(input.MaxTicks > 0, x => x.Ticks < input.MaxTicks)
            .WhereIf(input.IsOnline == true, x => onlineFriendIds.Contains(x.DestinationId))
            .WhereIf(input.IsOnline == false, x => !onlineFriendIds.Contains(x.DestinationId))
            ;

        //search 
        query = await ApplyFriendFilterAsync(query, input.OwnerId, input.Keyword);

        if (query == null)
        {
            return new ExtraPagedResultDto<SessionUnitFriendDto>(0, []);
        }

        var totalCount = query.Count(); //kvs.Length
        //var totalCount = await SessionUnitCacheManager.GetFirendsCountAsync(ownerId);

        // sorting desc, ticks desc ---此处可以不用，已经是按 score desc 排序了
        query = query
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Id)
            ;

        //paged
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var items = await GetManyWithScoreAsync(query);

        // hasNextPage
        var nextCursor = items.Count == input.MaxResultCount
            ? new { CursorScore = items.Last().Score, CursorId = items.Last().Id }
            : null;

        var result = new ExtraPagedResultDto<SessionUnitFriendDto>(totalCount, items)
        {
            Extra = new { NextCursor = nextCursor }
        };

        return result;
    }

    /// <summary>
    /// 获取好友会话
    /// </summary>
    public async Task<ExtraPagedResultDto<Guid>> GetFriendIdsAsync(SessionUnitFirendGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        //加载全部
        await LoadFriendsAsync(input.OwnerId);

        var stopwatch = Stopwatch.StartNew();

        var queryable = await SessionUnitCacheManager.GetTypedFriendsAsync(
            input.View,
            input.OwnerId,
            input.BoxId,
            //minScore: input.MinScore ?? double.NegativeInfinity,
            //maxScore: input.MaxScore ?? double.PositiveInfinity,
            //skip: input.SkipCount,
            //take: input.MaxResultCount,
            isDescending: true);

        var query = queryable.AsQueryable();

        var items = query.Select(x => x.Id).ToList();

        var result = new ExtraPagedResultDto<Guid>(items.Count, items);

        return result;
    }

    public async Task<Dictionary<string, List<Guid>>> GetFriendsIndexedAsync(long ownerId, ChatObjectTypeEnums? type)
    {
        // check owner
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName, ownerId));

        //加载全部
        await LoadFriendsAsync(ownerId);

        var kv = await SessionUnitCacheManager.GetFriendsIndexeAsync(ownerId);

        var result = kv
            .WhereIf(type.HasValue, x => x.Value.DestinationObjectType == type)
            .GroupBy(x => x.Key, g => g.Value)
            .ToDictionary(x => x.Key, g => g.Select(x => x.SessionUnitId).ToList());

        return result;
    }
    private async Task<List<SessionUnitFriendDto>> GetManyWithScoreAsync(IEnumerable<FriendModel> query)
    {
        var stopwatch = Stopwatch.StartNew();

        var unitIdScoreMap = query.ToDictionary(x => x.Id, x => x.Score);

        var items = await GetManyAsync(unitIdScoreMap.Keys.ToList());

        foreach (var item in items)
        {
            item.Score = unitIdScoreMap.GetValueOrDefault(item.Id);
        }

        Logger.LogInformation("FillScoreAsync, ReturnedCount={ReturnedCount}, Elapsed={Elapsed}ms",
            unitIdScoreMap.Count,
            stopwatch.ElapsedMilliseconds);

        return items;
    }



    /// <summary>
    /// 获取好友数量
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<FriendCountDto> GetFriendsCountAsync([Required] long ownerId)
    {
        await LoadFriendsAsync(ownerId);

        return new FriendCountDto()
        {
            TotalCount = await SessionUnitCacheManager.GetFriendsCountAsync(ownerId),
            CountMap = await SessionUnitCacheManager.GetFriendsCountMapAsync(ownerId),
            Boxes = await GetBoxFriendsCountAsync(ownerId),
        };
    }



    private static SessionUnitMemberSettingDto MapToSettingDto(SessionUnitCacheItem memberUnit, SessionUnitMemberSettingDto settingDto)
    {
        // 更新为缓存的值（最新）
        settingDto.LastSendMessageId = memberUnit.LastMessageId;
        settingDto.LastSendTime = memberUnit.LastSendTime;
        settingDto.IsCreator = memberUnit.IsCreator;
        settingDto.IsPublic = memberUnit.IsPublic;
        settingDto.IsStatic = memberUnit.IsStatic;
        settingDto.IsEnabled = memberUnit.IsEnabled;
        settingDto.IsVisible = memberUnit.IsVisible;
        settingDto.MemberName = memberUnit.MemberName;

        return settingDto;
    }

    /// <summary>
    /// 获取成员信息
    /// </summary>
    /// <param name="unitId"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<SessionUnitMemberDetailDto> GetMemberAsync(Guid unitId, SessionUnitGetMemberOptions options)
    {

        var unitIdList = new List<Guid> { unitId };
        if (options != null)
        {
            unitIdList.Add(options.VisitorId);
        }
        var unitList = await GetCacheManyAsync(unitIdList);
        var memberUnit = unitList[0].Value;

        var ownerId = memberUnit.OwnerId;

        var visitor = unitList.Length == 2 ? unitList[1].Value : null;

        Assert.If(memberUnit.SessionId != visitor?.SessionId, "不在同一个会话");

        //await CheckPolicyForUserAsync([unit.OwnerId], () => CheckPolicyAsync(GetListPolicyName));

        var allIds = new List<long?>() { ownerId, memberUnit.DestinationId }
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Distinct()
            .ToList();

        var chatObjectMap = (await ChatObjectManager.GetManyByCacheAsync(allIds))
            .ToDictionary(x => x.Id, x => x);

        var setting = await SessionUnitSettingManager.GetOrAddCacheAsync(unitId);

        var settingDto = ObjectMapper.Map<SessionUnitSettingCacheItem, SessionUnitMemberSettingDto>(setting);

        settingDto = MapToSettingDto(memberUnit, settingDto);

        var item = new SessionUnitMemberDetailDto()
        {
            Id = memberUnit.Id,
            SessionId = memberUnit.SessionId,
            MemberName = memberUnit.MemberName,
            Sorting = memberUnit.Sorting,
            Ticks = memberUnit.Ticks,

            // Owner
            Owner = chatObjectMap.GetValueOrDefault(memberUnit.OwnerId),
            OwnerId = memberUnit.OwnerId,
            OwnerObjectType = memberUnit.OwnerObjectType,

            // Destination
            Destination = memberUnit.DestinationId.HasValue ? chatObjectMap.GetValueOrDefault(memberUnit.DestinationId.Value) : null,
            DestinationId = memberUnit.DestinationId,
            DestinationObjectType = memberUnit.DestinationObjectType,

            // Setting
            Setting = settingDto,
        };

        if (options == null)
        {
            return item;
        }

        // 加载好友
        var friendsMap = await SessionUnitManager.LoadFriendsMapAsync([visitor.OwnerId]);

        var unitIdMap = friendsMap.GetOrDefault(visitor.OwnerId)?.ToDictionary(x => x.DestinationId, x => x.SessionUnitId);

        var friendshipSessionUnits = await SessionUnitCacheManager.GetManyAsync([.. unitIdMap.Values]);

        var friendMap = friendshipSessionUnits
            .Select(x => x.Value)
            .Where(x => x != null && x.DestinationId.HasValue)
            .DistinctBy(x => x!.DestinationId!.Value)
            .ToDictionary(x => x!.DestinationId!.Value, x => x!);

        var friendshipSessionUnit = friendMap.GetValueOrDefault(memberUnit.OwnerId);

        item.Friendship = friendshipSessionUnit != null ? SessionUnitFriendshipMapper.Map(friendshipSessionUnit) : new SessionUnitFriendshipDto();

        item.Friendship.VisitorId = options.VisitorId;

        return item;
    }


    /// <summary>
    /// 获取会话成员
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitMemberDto>> GetMembersAsync(SessionUnitMemberGetListInput input)
    {
        var unit = await GetCacheAsync(input.SessionUnitId);
        var ownerId = unit.OwnerId;
        var sessionId = unit.SessionId.Value;
        // check owner
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName, ownerId));

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            return await GetSearchMembersAsync(sessionId, input);
        }

        return await GetMembersInternalAsync(sessionId, input);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitIds"></param>
    /// <returns></returns>
    private async Task<List<SessionUnitMemberDto>> GetMemberListAsync(List<Guid> unitIds)
    {
        var list = await GetCacheManyAsync(unitIds);

        var allIds = list.Select(x => x.Value).Select(x => x.OwnerId).Distinct().ToList();

        var chatObjectMap = (await ChatObjectManager.GetManyByCacheAsync(allIds))
            .ToDictionary(x => x.Id, x => x);

        var items = list
            .Select(x => x.Value)
            //.Select(MapToMemberDto)
            .Select(x => new SessionUnitMemberDto()
            {
                Id = x.Id,
                SessionId = x.SessionId,
                MemberName = x.MemberName,
                Sorting = x.Sorting,
                Ticks = x.Ticks,

                // Owner
                Owner = chatObjectMap.GetValueOrDefault(x.OwnerId),
                OwnerId = x.OwnerId,
                OwnerObjectType = x.OwnerObjectType,

                // Destination
                //Destination = memberUnit.DestinationId.HasValue ? chatObjectMap.GetValueOrDefault(memberUnit.DestinationId.Value) : null,
                DestinationId = x.DestinationId,
                DestinationObjectType = x.DestinationObjectType,

                // Setting
                Setting = new SessionUnitMemberSettingDto()
                {
                    SessionUnitId = x.SessionId.GetValueOrDefault(),
                    IsEnabled = x.IsEnabled,
                    IsCreator = x.IsCreator,
                    IsPublic = x.IsPublic,
                    IsStatic = x.IsStatic,
                    IsVisible = x.IsVisible,
                    MemberName = x.MemberName,
                },
            })
            .ToList();

        await FillSessionTagAsync(items);

        return items;
    }

    /// <summary>
    /// 搜索成员
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    protected virtual async Task<PagedResultDto<SessionUnitMemberDto>> GetSearchMembersAsync(Guid sessionId, SessionUnitMemberGetListInput input)
    {
        //var unit = await GetCacheAsync(input.SessionUnitId);
        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && x.Setting.IsEnabled)
            //.WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            .WhereIf(input.IsStatic.HasValue, x => x.Setting.IsStatic == input.IsStatic)
            .WhereIf(input.IsCreator.HasValue, x => x.Setting.IsCreator == input.IsCreator)
            .WhereIf(input.IsPrivate.HasValue, x => x.Setting.IsPublic == !input.IsPrivate)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;

        var totalCount = query.Count(); //kvs.Length

        query = query
           .OrderByDescending(x => x.Setting.IsCreator)
           .ThenBy(x => x.CreationTime)
           ;

        // paged
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var unitIds = query.Select(x => x.Id).ToList();

        var items = await GetMemberListAsync(unitIds);

        return new PagedResultDto<SessionUnitMemberDto>(totalCount, items);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    protected virtual async Task<PagedResultDto<SessionUnitMemberDto>> GetMembersInternalAsync(Guid sessionId, SessionUnitMemberGetListInput input)
    {
        //加载全部
        await LoadMembersAsync(sessionId);

        var queryable = await SessionUnitCacheManager.GetMembersAsync(
            sessionId,
            isCreator: input.IsCreator,
            isPrivate: input.IsPrivate,
            isStatic: input.IsStatic,
            isDescending: true);

        var query = queryable.AsQueryable()
            .WhereIf(input.IsCreator.HasValue, x => x.IsCreator == input.IsCreator.Value)
            .WhereIf(input.OwnerObjectType.HasValue, x => x.OwnerObjectType == input.OwnerObjectType.Value)
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId.Value)
            .WhereIf(input.MinScore > 0, x => x.CreationTime.ToUnixTimeMilliseconds() > input.MinScore)
            .WhereIf(input.MaxScore > 0, x => x.CreationTime.ToUnixTimeMilliseconds() < input.MaxScore)
            ;

        if (query == null)
        {
            return new PagedResultDto<SessionUnitMemberDto>(0, []);
        }

        var totalCount = query.Count(); //kvs.Length

        // sorting
        query = query
            .OrderByDescending(x => x.IsCreator)
            .ThenBy(x => x.CreationTime)
            ;

        // paged
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var unitIds = query.Select(x => x.Id).ToList();

        var items = await GetMemberListAsync(unitIds);

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
        await LoadMembersAsync(sessionId);
        return new MemberCountDto()
        {
            SessionId = sessionId,
            TotalCount = await SessionUnitCacheManager.GetMembersCountAsync(sessionId),
        };
    }

    /// <summary>
    /// 获取聊天对象角标信息列表
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <returns></returns>
    public async Task<List<SessionUnitOwnerOverviewInfo>> GetOverviewOwnersAsync(List<long> ownerIds)
    {
        var items = await SessionUnitCacheManager.GetOwnerBadgeAsync(ownerIds);

        var boxInfoMap = await GetBoxOverviewAsync(ownerIds);

        foreach (var item in items)
        {
            item.Boxes = [.. boxInfoMap.GetValueOrDefault(item.OwnerId)];
        }

        return items;
    }


    /// <summary>
    /// 聊天对象角标总数
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<SessionUnitOwnerOverviewInfo> GetOverviewOwnerAsync(long ownerId)
    {
        // check owner
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName, ownerId));
        var items = await GetOverviewOwnersAsync([ownerId]);
        return items.FirstOrDefault();
    }

    /// <summary>
    /// 用户聊天对象角标列表
    /// </summary>
    /// <param name="userId"></param>
    public async Task<SessionUnitUserOverviewInfo> GetOverviewUserAsync([Required] Guid userId)
    {
        var ownerIds = await ChatObjectManager.GetIdListByUserIdAsync(userId);

        await CheckPolicyForUserAsync(ownerIds, () => CheckPolicyAsync(GetBadgePolicyName));

        var ownerOverviews = await GetOverviewOwnersAsync(ownerIds);

        return new SessionUnitUserOverviewInfo()
        {
            UserId = userId,
            Overviews = ownerOverviews,
            TotalOwnersCount = ownerOverviews.Count,
            TotalUnreadCount = ownerOverviews.Sum(x => x.TotalUnreadCount),
        };
    }

    /// <summary>
    /// 登录用户聊天对象角标列表
    /// </summary>
    /// <returns></returns>
    public Task<SessionUnitUserOverviewInfo> GetOverviewAsync()
    {
        return GetOverviewUserAsync(CurrentUser.GetId());
    }

    private async Task<List<BoxCountDto>> GetBoxFriendsCountAsync([Required] long ownerId)
    {
        var boxes = await BoxManager.GetCacheListByOwnerAsync(ownerId);

        var boxMap = boxes.List.ToDictionary(x => x.Id);

        var items = await SessionUnitCacheManager.GetBoxBadgeInfoAsync(ownerId);

        var result = items.Select(x =>
        {
            return new BoxCountDto()
            {
                Id = Guid.Parse(x.Id),
                Name = x.Name ?? boxMap.GetValueOrDefault(Guid.Parse(x.Id))?.Name,
                OwnerId = ownerId,
                Count = x.Count,
                Badge = x.Badge,
            };
        }).ToList();

        return result;
    }

    /// <summary>
    /// 获取盒子角标
    /// </summary>
    /// <param name="ownerIds"></param>
    /// <returns></returns>
    public async Task<Dictionary<long, IEnumerable<SessionUnitStatInfo>>> GetBoxOverviewAsync(List<long> ownerIds)
    {
        var boxInfoMap = await SessionUnitCacheManager.GetBoxBadgeInfoMapAsync(ownerIds);

        var boxMap = await BoxManager.GetCacheListByOwnersAsync(ownerIds);

        return boxMap.ToDictionary(x => x.Key, x => x.Value.Select(v =>
        {
            var info = boxInfoMap.GetValueOrDefault(x.Key)?.FirstOrDefault(d => d.Id == v.Id.ToString());
            return new SessionUnitStatInfo
            {
                Id = v.Id.ToString(),
                Name = v.Name,
                Badge = info?.Badge,
                Count = info?.Count,
            };
        }));
    }

}
