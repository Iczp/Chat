using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace IczpNet.Chat.Permissions;

public class ChatObjectPermissionManagementProvider : PermissionManagementProvider
{
    public ChatObjectPermissionManagementProvider(
        IPermissionGrantRepository permissionGrantRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
        : base(permissionGrantRepository, guidGenerator, currentTenant)
    {
    }

    public override string Name => "ChatObject";
}
