using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.DeviceGroups;

/// <inheritdoc />
public class DeviceGroupAppService(IDeviceGroupRepository repository) : CrudChatAppService<DeviceGroup, DeviceGroupDetailDto, DeviceGroupDto, Guid, DeviceGroupGetListInput, DeviceGroupCreateInput, DeviceGroupUpdateInput>(repository), IDeviceGroupAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.SetIsEnabled;

    /// <inheritdoc />
    protected override async Task<IQueryable<DeviceGroup>> CreateFilteredQueryAsync(DeviceGroupGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            ;
    }
}
