using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Devices;
using IczpNet.Chat.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.DeviceGroups;

/// <inheritdoc />
public partial class DeviceGroupAppService(
    IczpNet.AppDevice.DeviceGroups.IDeviceGroupRepository repository) : CrudChatAppService<IczpNet.AppDevice.DeviceGroups.DeviceGroup, DeviceGroupDetailDto, DeviceGroupDto, Guid, DeviceGroupGetListInput, DeviceGroupCreateInput, DeviceGroupUpdateInput>(repository), IDeviceGroupAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.SetIsEnabled;
    protected virtual string SetDevicesPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.SetDevices;
    public virtual async Task<int> SetDevicesAsync(Guid id, List<Guid> deviceIdList)
    {
        await CheckPolicyAsync(SetDevicesPolicyName);
        return await SetAppDeviceMembersAsync(id,deviceIdList);
    }

}
