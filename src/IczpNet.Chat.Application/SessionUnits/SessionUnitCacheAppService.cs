using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
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
        var list = await SessionUnitCacheManager.GetOrSetListByOwnerAsync(
            input.OwnerId,
            async (ownerId) =>
                await SessionUnitManager.GetListByOwnerIdAsync(ownerId));

        var query = list.Select(x => new SessionUnitCacheDto()
        {
            Id = x.Id,
            Item = x,
        }).AsQueryable()
            .WhereIf(input.DestinationId.HasValue, x => x.Item.DestinationId == input.DestinationId)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.Item.DestinationObjectType == input.DestinationObjectType)
            //.WhereIf(input.IsKilled.HasValue, x => x.Item.Setting.IsKilled == input.IsKilled)
            //.WhereIf(input.IsCreator.HasValue, x => x.Item.Setting.IsCreator == input.IsCreator)
            .WhereIf(input.MinMessageId.HasValue, x => x.Item.LastMessageId >= input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.Item.LastMessageId < input.MaxMessageId)
            .WhereIf(input.MinTicks.HasValue, x => x.Item.Ticks >= input.MinTicks)
            .WhereIf(input.MaxTicks.HasValue, x => x.Item.Ticks < input.MaxTicks)
            //.WhereIf(input.IsTopping == true, x => x.Item.Sorting > 0)
            //.WhereIf(input.IsTopping == false, x => x.Item.Sorting == 0)
            //.WhereIf(input.IsContacts.HasValue, x => x.Item.Setting.IsContacts == input.IsContacts)
            //.WhereIf(input.IsImmersed.HasValue, x => x.Item.Setting.IsImmersed == input.IsImmersed)
            .WhereIf(input.IsBadge.HasValue, x => x.Item.PublicBadge > 0)

            //@我
            .WhereIf(input.IsRemind == true, x => (x.Item.RemindAllCount + x.Item.RemindMeCount) > 0)
            .WhereIf(input.IsRemind == false, x => (x.Item.RemindAllCount + x.Item.RemindMeCount) == 0)


            //我关注的
            //.WhereIf(input.IsFollowing.HasValue, x => x.Item.FollowingCount > 0)
            //.WhereIf(input.IsFollowing == true, x => x.Item.FollowingList.Count > 0)
            //.WhereIf(input.IsFollowing == false, x => x.Item.FollowingList.Count == 0)

            //关注我的
            //.WhereIf(input.IsFollower == true, x => x.Item.FollowerList.Count > 0)
            //.WhereIf(input.IsFollower == false, x => x.Item.FollowerList.Count == 0)
            ;

        var pagedList = await GetPagedListAsync(query, input);

        var unitIds = pagedList.Items.Select(x => x.Id).ToList();

        var settingMap = await SessionUnitSettingManager.GetCacheManyAsync(unitIds);

        var destIdList = pagedList.Items.Where(x => x.Item.DestinationId.HasValue).Select(x => x.Item.DestinationId.Value).Distinct().ToList();

        var destList = await ChatObjectManager.GetManyByCacheAsync(destIdList);

        var destMap = destList.ToDictionary(x => x.Id, x => x);

        foreach (var item in pagedList.Items)
        {
            item.Settings = settingMap[item.Id] ?? null;
            item.Destination = item.Item.DestinationId.HasValue ? destMap[item.Item.DestinationId.Value] ?? null : null;
        }

        return pagedList;

    }
}
