using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话单元
/// </summary>
public class SessionUnitCacheAppService(
    ISessionUnitCacheManager sessionUnitCacheManager) : ChatAppService, ISessionUnitCacheAppService
{
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected override string GetPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected virtual string GetDetailPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.MessageBus;
    protected virtual string GetListForSameSessionPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetSameSession;
    protected virtual string GetItemForSameSessionPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetSameSession;
    protected virtual string GetBadgePolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetBadge;
    protected virtual string FindPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.Find;
    protected virtual string GetCounterPolicyName { get; set; } = ChatPermissions.SessionUnitPermissions.GetCounter;

    public async Task<PagedResultDto<SessionUnitCacheItem>> GetListAsync(SessionUnitCacheItemGetListInput input)
    {
        var list = await SessionUnitCacheManager.GetOrSetListByOwnerAsync(
            input.OwnerId,
            async (ownerId) => 
                await SessionUnitManager.GetListByOwnerIdAsync(ownerId));

        var query = list.AsQueryable();

        return await GetPagedListAsync(query, input);

    }
}
