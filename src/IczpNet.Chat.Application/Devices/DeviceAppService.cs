using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.Devices;

/// <inheritdoc />
public class DeviceAppService(IDeviceRepository repository) : CrudChatAppService<Device, DeviceDetailDto, DeviceDto, Guid, DeviceGetListInput, DeviceCreateInput, DeviceUpdateInput>(repository), IDeviceAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.DevicePermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.DevicePermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.DevicePermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.DevicePermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.DevicePermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DevicePermissions.SetIsEnabled;

    /// <inheritdoc />
    protected override async Task<IQueryable<Device>> CreateFilteredQueryAsync(DeviceGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))
            //.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            ;
    }
}
