using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace IczpNet.Chat.Devices;

/// <inheritdoc />
public class DeviceAppService(
    IDeviceGroupRepository deviceGroupRepository,
    IDeviceRepository repository) : CrudChatAppService<Device, DeviceDetailDto, DeviceDto, Guid, DeviceGetListInput, DeviceCreateInput, DeviceUpdateInput>(repository), IDeviceAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.DevicePermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.DevicePermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.DevicePermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.DevicePermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.DevicePermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DevicePermissions.SetIsEnabled;
    protected virtual string SetSetGroupsPolicyName { get; set; } = ChatPermissions.DevicePermissions.SetGroups;

    public IDeviceGroupRepository DeviceGroupRepository { get; } = deviceGroupRepository;

    /// <inheritdoc />
    protected override async Task<IQueryable<Device>> CreateFilteredQueryAsync(DeviceGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
            .WhereIf(input.DeviceGroupId.HasValue, x => x.DeviceGroupMapList.Any(d => d.DeviceGroupId == input.DeviceGroupId))
            .WhereIf(!string.IsNullOrEmpty(input.DeviceId), x => x.DeviceId == input.DeviceId)
            .WhereIf(!string.IsNullOrEmpty(input.Platform), x => x.Platform == input.Platform)
            .WhereIf(!string.IsNullOrEmpty(input.AppId), x => x.AppId == input.AppId)
            ;
    }

    public async Task<int> SetGroupsAsync(Guid id, List<Guid> groupIdList)
    {
        await CheckPolicyAsync(SetSetGroupsPolicyName);

        await CheckExistAsync(groupIdList);

        var entity = await Repository.GetAsync(id);

        return entity.SetGroups(groupIdList);
    }

    protected async Task CheckExistAsync(List<Guid> groupIdList)
    {
        var existIds = (await DeviceGroupRepository.GetQueryableAsync()).Where(x => groupIdList.Contains(x.Id)).Select(x => x.Id).ToList();

        // 差集：找出数据库中不存在的 ID
        var missingIds = groupIdList.Except(existIds).ToList();

        Assert.If(missingIds.Count != 0, $"以下 GroupId 不存在: {string.Join(", ", missingIds)}");
    }
}
