using AutoMapper.Internal.Mappers;
using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Follows;
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
    ISessionUnitSettingManager sessionUnitSettingManager,
    IFollowManager followManager,
    ISessionUnitCacheManager sessionUnitCacheManager) : ChatAppService, ISessionUnitCacheAppService
{
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


    public SessionUnitCacheDto MapToDto(SessionUnitCacheItem item)
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

        return pagedList;
    }

    private async Task<PagedResultDto<SessionUnitCacheDto>> FillSettingAsync(PagedResultDto<SessionUnitCacheDto> pagedList)
    {
        // fill Setting
        var nullSettingsItems = pagedList.Items
            .Where(x => x.Setting == null)
            .ToList();

        if (nullSettingsItems.Count != 0)
        {
            var unitIds = nullSettingsItems.Select(x => x.Id).Distinct().ToList();
            var settingMap = (await SessionUnitSettingManager.GetManyCacheAsync(unitIds))
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var item in nullSettingsItems)
            {
                item.Setting = settingMap.GetValueOrDefault(item.Id);
            }
        }
        return pagedList;
    }

    private async Task<PagedResultDto<SessionUnitCacheDto>> FillDestinationAsync(PagedResultDto<SessionUnitCacheDto> pagedList)
    {
        // fill Destination
        var nullDestItems = pagedList.Items
            .Where(x => x.DestinationId.HasValue && x.Destination == null)
            .ToList();

        if (nullDestItems.Count != 0)
        {
            var destIds = nullDestItems.Select(x => x.DestinationId.Value).Distinct().ToList();
            var destMap = (await ChatObjectManager.GetManyByCacheAsync(destIds))
                .ToDictionary(x => x.Id, x => x);

            foreach (var item in nullDestItems)
            {
                item.Destination = destMap.GetValueOrDefault(item.DestinationId.Value);
            }
        }
        return pagedList;
    }

    public async Task<SessionUnitCacheDto> GetAsync(Guid id)
    {
        var items = await SessionUnitCacheManager.GetManyAsync([id]);

        var item = items.FirstOrDefault().Value;

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
        var allList = await GetAllListAsync(ownerId);

        var hasBadgeList = allList.Where(x => x.PublicBadge > 0 || x.PrivateBadge > 0).ToList();

        return allList.Sum(x => x.PublicBadge + x.PrivateBadge);
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
