using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace IczpNet.Chat.Permissions;

public class ChatObjectPermissionManagementProvider(
    IPermissionGrantRepository permissionGrantRepository,
    IGuidGenerator guidGenerator,
    ICurrentTenant currentTenant) : PermissionManagementProvider(permissionGrantRepository, guidGenerator, currentTenant)
{
    public override string Name => "ChatObject";
}
