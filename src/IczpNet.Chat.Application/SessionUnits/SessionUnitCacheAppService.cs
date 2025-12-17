using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    IMessageRepository messageRepository,
    ISessionUnitSettingManager sessionUnitSettingManager,
    ISessionUnitRepository sessionUnitRepository,
    IFollowManager followManager,
    IDistributedCache<SessionUnitSearchCacheItem, SessionUnitSearchCacheKey> searchCache,
    ISessionUnitCacheManager sessionUnitCacheManager) : ChatAppService, ISessionUnitCacheAppService
{
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ISessionUnitSettingManager SessionUnitSettingManager { get; } = sessionUnitSettingManager;
    public ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    public IFollowManager FollowManager { get; } = followManager;
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
               f.StartsWith($"{nameof(SessionUnitCacheDto.Destination)}.")
        );

        return Task.FromResult(
               !string.IsNullOrWhiteSpace(input.Keyword)  // 搜索 Keyword 时一定需要全量加载
            || needLoadBySorting
        );
    }

    protected virtual async Task LoadAllByOwnerIfNotExistsAsync(long ownerId)
    {
        await SessionUnitCacheManager.SetListByOwnerIfNotExistsAsync(ownerId,
           async (ownerId) =>
               await SessionUnitManager.GetListByOwnerIdAsync(ownerId));
    }

    protected virtual async Task<IEnumerable<SessionUnitCacheItem>> GetAllListAsync(long ownerId)
    {
        return await SessionUnitCacheManager.GetOrSetListByOwnerAsync(
           ownerId,
           async (ownerId) =>
               await SessionUnitManager.GetListByOwnerIdAsync(ownerId));
    }

    protected Task<SessionUnitCacheItem> MapToCacheItemAsync(SessionUnit entity)
    {
        return Task.FromResult(MapToCacheItem(entity));
    }

    protected virtual SessionUnitCacheItem MapToCacheItem(SessionUnit entity)
    {
        return ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(entity);
    }

    protected virtual SessionUnitCacheDto MapToDto(SessionUnitCacheItem item)
    {
        return ObjectMapper.Map<SessionUnitCacheItem, SessionUnitCacheDto>(item);
    }

    protected virtual async Task<IQueryable<SessionUnitCacheDto>> CreateQueryableAsync(SessionUnitCacheItemGetListInput input)
    {
        var allList = await GetAllListAsync(input.OwnerId);

        var result = allList
            .Select(MapToDto)
            .ToList()
            .AsQueryable();

        var shouldLoadAll = await ShouldLoadAllAsync(input);

        if (shouldLoadAll)
        {
            //var allUnitIds = result.Select(x => x.Id).Distinct().ToList();
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
                //item.Setting = settingMap.GetValueOrDefault(item.Id);
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


    public async Task<PagedResultDto<SessionUnitCacheDto>> GetListAsync(SessionUnitCacheItemGetListInput input)
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



    private async Task FillLastMessageAsync(IEnumerable<SessionUnitCacheDto> items)
    {
        // fill Setting
        var messageIdList = items
            .Where(x => x.LastMessageId.HasValue)
            .Select(x => x.LastMessageId)
            .ToList();
        if (messageIdList.Count == 0)
        {
            return;
        }

        var messages = await MessageRepository.GetListAsync(x => messageIdList.Contains(x.Id));
        var messageMap = messages
            .Select(ObjectMapper.Map<Message, MessageOwnerDto>)
            .ToDictionary(x => x.Id, x => x);
        foreach (var item in items)
        {
            item.LastMessage = item.LastMessageId.HasValue ? messageMap.GetValueOrDefault(item.LastMessageId.Value) : null;
        }
    }

    private async Task FillSettingAsync(IEnumerable<SessionUnitCacheDto> items)
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
        var settingMap = (await SessionUnitSettingManager.GetManyCacheAsync(unitIds))
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var item in nullSettingsItems)
        {
            item.Setting = settingMap.GetValueOrDefault(item.Id);
        }
    }

    private async Task FillDestinationAsync(IEnumerable<SessionUnitCacheDto> items)
    {
        // fill Destination
        var nullDestItems = items
            .Where(x => x.DestinationId.HasValue && x.Destination == null)
            .ToList();
        if (nullDestItems.Count == 0)
        {
            return;
        }
        var destIds = nullDestItems.Select(x => x.DestinationId.Value).Distinct().ToList();
        var destMap = (await ChatObjectManager.GetManyByCacheAsync(destIds))
            .ToDictionary(x => x.Id, x => x);

        foreach (var item in nullDestItems)
        {
            item.Destination = destMap.GetValueOrDefault(item.DestinationId.Value);
        }
    }

    public async Task<List<SessionUnitCacheDto>> GetManyAsync(List<Guid> unitIds)
    {
        var list = await SessionUnitCacheManager.GetOrSetManyAsync(unitIds, async (keys) =>
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
    /// 获取最新消息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitCacheDto>> GetLatestAsync(SessionUnitCacheScoreGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        Assert.If(input.MaxResultCount > 100, $"params:{nameof(input.MaxResultCount)} max value: 100");

        var minScore = input.MinScore ?? 0;

        var queryable = await SessionUnitCacheManager.GetSortedSetQueryableByOwnerAsync(input.OwnerId, minScore: minScore, skip: input.SkipCount, take: Math.Min(input.MaxResultCount, 100));

        var unitIds = queryable
            .Where(x => x.Ticks > minScore)
            .OrderByDescending(x => x.Ticks)
            .Select(x => x.UnitId)
            .ToList();

        var items = await GetManyAsync(unitIds);

        return new PagedResultDto<SessionUnitCacheDto>(items.Count, items);

    }


    public async IAsyncEnumerable<Guid> BatchSearchAsync(
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
    private async Task<IQueryable<(Guid UnitId, double Sorting, double Ticks)>> ApplyFilterAsync(IQueryable<(Guid UnitId, double Sorting, double Ticks)> query, long ownerId, string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return query;
        }
        var allUnitIds = query.Select(x => x.UnitId).ToList();

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
            //.Where(x => allUnitIds.Contains(x.Id))
            //;
            //var searchUnitIds = querySearch.Select(x => x.Id).ToList();
            //return new SessionUnitSearchCacheItem(searchUnitIds);
        });

        var searchUnitIds = searchResult.UnitIds;

        if (searchUnitIds.Count == 0)
        {
            return null;
        }

        // 加入查询条件
        query = query.WhereIf(searchUnitIds.Count > 0, x => searchUnitIds.Contains(x.UnitId));

        return query;
    }

    /// <summary>
    /// 获取会话(Linq)
    /// </summary>
    /// <returns></returns>
    public async Task<PagedResultDto<SessionUnitCacheDto>> GetHistoryAsync(SessionUnitCacheScoreGetListInput input)
    {
        // check owner
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName, input.OwnerId));

        //加载全部
        await LoadAllByOwnerIfNotExistsAsync(input.OwnerId);

        var queryable = await SessionUnitCacheManager.GetSortedSetQueryableByOwnerAsync(input.OwnerId);

        var query = queryable
            .WhereIf(input.MinScore > 0, x => x.Ticks > input.MinScore)
            .WhereIf(input.MaxScore > 0, x => x.Ticks < input.MaxScore)
            ;

        //search 
        query = await ApplyFilterAsync(query, input.OwnerId, input.Keyword);

        if (query == null)
        {
            return new PagedResultDto<SessionUnitCacheDto>(0, []);
        }

        var totalCount = query.Count(); //kvs.Length
        //var totalCount = await SessionUnitCacheManager.GetTotalCountByOwnerAsync(ownerId);

        // sorting
        query = query
            .OrderByDescending(x => x.Sorting)
            .ThenByDescending(x => x.Ticks)
            ;

        // paged
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);

        var unitIds = query.Select(x => x.UnitId).ToList();

        var items = await GetManyAsync(unitIds);

        return new PagedResultDto<SessionUnitCacheDto>(totalCount, items);
    }

    /// <summary>
    /// 获取会话
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SessionUnitCacheDto> GetAsync(Guid id)
    {
        var items = await GetManyAsync([id]);

        var item = items.FirstOrDefault();

        Assert.If(item == null, $"No such cache id:{id}");

        return item;
    }

    /// <summary>
    /// 聊天对象角标总数
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<SessionUnitStatistic> GetStatisticAsync(long ownerId)
    {
        // check owner
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName, ownerId));

        //加载全部
        await LoadAllByOwnerIfNotExistsAsync(ownerId);

        var totalBadge = await SessionUnitCacheManager.GetStatisticAsync(ownerId);

        return totalBadge;
    }

    /// <summary>
    /// 用户聊天对象角标列表
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<BadgeDto>> GetStatisticByUserIdAsync([Required] Guid userId, bool? isImmersed = null)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(userId);

        await CheckPolicyForUserAsync(chatObjectIdList, () => CheckPolicyAsync(GetBadgePolicyName));

        var result = new List<BadgeDto>();

        foreach (var ownerId in chatObjectIdList)
        {
            //加载全部
            await LoadAllByOwnerIfNotExistsAsync(ownerId);

            var stat = await GetStatisticAsync(ownerId);

            result.Add(new BadgeDto()
            {
                AppUserId = userId,
                ChatObjectId = ownerId,
                Statistic = stat,
            });
        }

        return result;
    }

    /// <summary>
    /// 登录用户聊天对象角标列表
    /// </summary>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    public Task<List<BadgeDto>> GetStatisticByCurrentUserAsync(bool? isImmersed = null)
    {
        return GetStatisticByUserIdAsync(CurrentUser.GetId(), isImmersed);
    }
}
