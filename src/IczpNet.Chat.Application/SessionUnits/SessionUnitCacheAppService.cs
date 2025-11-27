using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


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

    public async Task<PagedResultDto<SessionUnitCacheDto>> GetListAsync(SessionUnitCacheItemGetListInput input)
    {

        //var followingList = await FollowManager.GetFollowingIdListAsync(input.OwnerId);
        var cacheList = await SessionUnitCacheManager.GetOrSetListByOwnerAsync(
            input.OwnerId,
            async (ownerId) =>
                await SessionUnitManager.GetListByOwnerIdAsync(ownerId));

        var list = ObjectMapper.Map<List<SessionUnitCacheItem>, List<SessionUnitCacheDto>>(cacheList.ToList());

        var unitIds = list.Select(x => x.Id).Distinct().ToList();

        var settingMap = (await SessionUnitSettingManager.GetManyCacheAsync(unitIds)).ToDictionary(x => x.Key, x => x.Value);

        var destIdList = list.Where(x => x.DestinationId.HasValue).Select(x => x.DestinationId.Value).Distinct().ToList();

        var destList = await ChatObjectManager.GetManyByCacheAsync(destIdList);

        var destMap = destList.ToDictionary(x => x.Id, x => x);

        foreach (var item in list)
        {
            item.Destination = item.DestinationId.HasValue ? destMap[item.DestinationId.Value] : null;
            item.Settings = settingMap[item.Id] ?? null;
        }

        var query = list.AsQueryable()
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.DestinationObjectType == input.DestinationObjectType)
            //.WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            //.WhereIf(input.IsCreator.HasValue, x => x.Setting.IsCreator == input.IsCreator)
            .WhereIf(input.MinMessageId.HasValue, x => x.LastMessageId >= input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.LastMessageId < input.MaxMessageId)
            .WhereIf(input.MinTicks.HasValue, x => x.Ticks >= input.MinTicks)
            .WhereIf(input.MaxTicks.HasValue, x => x.Ticks < input.MaxTicks)
            //.WhereIf(input.IsTopping == true, x => x.Sorting > 0)
            //.WhereIf(input.IsTopping == false, x => x.Sorting == 0)
            //.WhereIf(input.IsContacts.HasValue, x => x.Setting.IsContacts == input.IsContacts)
            //.WhereIf(input.IsImmersed.HasValue, x => x.Setting.IsImmersed == input.IsImmersed)
            .WhereIf(input.IsBadge.HasValue, x => x.PublicBadge > 0)

            //@我
            .WhereIf(input.IsRemind == true, x => (x.RemindAllCount + x.RemindMeCount) > 0)
            .WhereIf(input.IsRemind == false, x => (x.RemindAllCount + x.RemindMeCount) == 0)


            //我关注的
            //.WhereIf(input.IsFollowing.HasValue, x => x.FollowingCount > 0)
            //.WhereIf(input.IsFollowing == true, x => x.FollowingList.Count > 0)
            //.WhereIf(input.IsFollowing == false, x => x.FollowingList.Count == 0)

            //关注我的
            //.WhereIf(input.IsFollower == true, x => x.FollowerList.Count > 0)
            //.WhereIf(input.IsFollower == false, x => x.FollowerList.Count == 0)
            ;

        var pagedList = await GetPagedListAsync(query, input);



        //foreach (var item in pagedList.Items)
        //{
        //    item.Settings = settingMap[item.Id] ?? null;
        //    item.Destination = item.Item.DestinationId.HasValue ? destMap[item.Item.DestinationId.Value] ?? null : null;
        //}

        return pagedList;

    }
}
