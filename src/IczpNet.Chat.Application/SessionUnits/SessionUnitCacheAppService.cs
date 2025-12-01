using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Users;


namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话单元
/// </summary>
public class SessionUnitCacheAppService(
    IMessageRepository messageRepository,
    ISessionUnitSettingManager sessionUnitSettingManager,
    IFollowManager followManager,
    ISessionUnitCacheManager sessionUnitCacheManager) : ChatAppService, ISessionUnitCacheAppService
{
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ISessionUnitSettingManager SessionUnitSettingManager { get; } = sessionUnitSettingManager;
    public IFollowManager FollowManager { get; } = followManager;
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
               f.StartsWith($"{nameof(SessionUnitCacheDto.Setting)}.")
            || f.StartsWith($"{nameof(SessionUnitCacheDto.Destination)}.")
        );

        return Task.FromResult(
               !string.IsNullOrWhiteSpace(input.Keyword)  // 🔥搜索 Keyword 时一定需要全量加载
            || needLoadBySorting
        );
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

            // 🤩 提前填充
            foreach (var item in result)
            {
                //item.Setting = settingMap.GetValueOrDefault(item.Id);
                item.Destination = item.DestinationId.HasValue
                    ? destMap.GetValueOrDefault(item.DestinationId.Value)
                    : null;
                item.SearchText = string.Join(" ",
                    item.Destination?.Name ?? "",
                    item.Setting?.MemberName ?? "",
                    item.Setting?.Rename ?? ""
                ).Replace("  ", " ").ToLower();
            }
        }
        return result;
    }


    public async Task<PagedResultDto<SessionUnitCacheDto>> GetListAsync(SessionUnitCacheItemGetListInput input)
    {
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
        await FillSettingAsync(pagedList);

        await FillDestinationAsync(pagedList);

        await FillLastMessageAsync(pagedList);

        return pagedList;
    }

    private async Task FillLastMessageAsync(PagedResultDto<SessionUnitCacheDto> pagedList)
    {
        // fill Setting
        var messageIdList = pagedList.Items
            .Where(x => x.LastMessageId.HasValue)
            .Select(x => x.LastMessageId)
            .ToList();
        if (messageIdList.Count == 0)
        {
            return;
        }
        var queryable = await MessageRepository.GetQueryableAsync();
        var messages = await MessageRepository.GetListAsync(x => messageIdList.Contains(x.Id));
        var messageMap = messages
            .Select(ObjectMapper.Map<Message, MessageOwnerDto>)
            .ToDictionary(x => x.Id, x => x);
        foreach (var item in pagedList.Items)
        {
            item.LastMessage = item.LastMessageId.HasValue ? messageMap.GetValueOrDefault(item.LastMessageId.Value) : null;
        }
    }

    private async Task FillSettingAsync(PagedResultDto<SessionUnitCacheDto> pagedList)
    {
        // fill Setting
        var nullSettingsItems = pagedList.Items
            .Where(x => x.Setting == null)
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

    private async Task FillDestinationAsync(PagedResultDto<SessionUnitCacheDto> pagedList)
    {
        // fill Destination
        var nullDestItems = pagedList.Items
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

        var items = list.Select(x => x.Value)
            .Where(x => x != null)
            .Select(MapToDto)
            .ToList();

        return items;
    }


    public async Task<SessionUnitCacheDto> GetAsync(Guid id)
    {
        var items = await GetManyAsync([id]);

        var item = items.FirstOrDefault();

        Assert.If(item == null, $"No such cache id:{id}");

        return MapToDto(item);
    }

    /// <summary>
    /// 聊天对象角标总数
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public async Task<long> GetBadgeAsync(long ownerId)
    {
        var totalBadge = await SessionUnitCacheManager.GetTotalBadgeAsync(ownerId);

        if (totalBadge.HasValue)
        {
            return totalBadge.Value;
        }
        var allList = await GetAllListAsync(ownerId);

        var hasBadgeList = allList.Where(x => x.PublicBadge > 0 || x.PrivateBadge > 0).ToList();

        totalBadge = allList.Sum(x => x.PublicBadge + x.PrivateBadge);

        await SessionUnitCacheManager.SetTotalBadgeAsync(ownerId, totalBadge.Value);

        return totalBadge.Value;
    }

    /// <summary>
    /// 用户聊天对象角标列表
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<BadgeDto>> GetBadgeByUserIdAsync([Required] Guid userId, bool? isImmersed = null)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(userId);

        await CheckPolicyForUserAsync(chatObjectIdList, () => CheckPolicyAsync(GetBadgePolicyName));

        var result = new List<BadgeDto>();

        foreach (var chatObjectId in chatObjectIdList)
        {
            result.Add(new BadgeDto()
            {
                AppUserId = userId,
                ChatObjectId = chatObjectId,
                Badge = (int)await GetBadgeAsync(chatObjectId)
            });
        }

        return result;
    }

    /// <summary>
    /// 登录用户聊天对象角标列表
    /// </summary>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    public Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null)
    {
        return GetBadgeByUserIdAsync(CurrentUser.GetId(), isImmersed);
    }

    /// <summary>
    /// UnitTest
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Task<SessionUnitCacheItem> UnitTestAsync()
    {
        return SessionUnitCacheManager.UnitTestAsync();
    }
}
