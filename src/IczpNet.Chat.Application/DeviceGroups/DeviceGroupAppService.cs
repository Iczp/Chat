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
public class DeviceGroupAppService(
    IDeviceRepository deviceRepository,
    IDeviceGroupRepository repository) : CrudChatAppService<DeviceGroup, DeviceGroupDetailDto, DeviceGroupDto, Guid, DeviceGroupGetListInput, DeviceGroupCreateInput, DeviceGroupUpdateInput>(repository), IDeviceGroupAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.SetIsEnabled;
    protected virtual string SetDevicesPolicyName { get; set; } = ChatPermissions.DeviceGroupPermissions.SetDevices;
    public IDeviceRepository DeviceRepository { get; } = deviceRepository;

    /// <inheritdoc />
    protected override async Task<IQueryable<DeviceGroup>> CreateFilteredQueryAsync(DeviceGroupGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            ;
    }

    public async Task<int> SetDevicesAsync(Guid id, List<Guid> deviceIdList)
    {
        await CheckPolicyAsync(SetDevicesPolicyName);
        var entity = await Repository.GetAsync(id);
        return entity.SetDevices(deviceIdList);
    }

    protected async Task CheckExistAsync(List<Guid> deviceIdList)
    {
        var existIds = (await DeviceRepository.GetQueryableAsync()).Where(x => deviceIdList.Contains(x.Id)).Select(x => x.Id).ToList();

        // 差集：找出数据库中不存在的 ID
        var missingIds = deviceIdList.Except(existIds).ToList();

        Assert.If(missingIds.Count != 0, $"以下 deviceId 不存在: {string.Join(", ", missingIds)}");
    }
}
