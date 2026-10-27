using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Devices;

/// <inheritdoc />
public partial class DeviceAppService(
    IczpNet.AppDevice.Devices.IDeviceRepository repository) : CrudChatAppService<IczpNet.AppDevice.Devices.Device, DeviceDetailDto, DeviceDto, Guid, DeviceGetListInput, DeviceCreateInput, DeviceUpdateInput>(repository), IDeviceAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.DevicePermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.DevicePermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.DevicePermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.DevicePermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.DevicePermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DevicePermissions.SetIsEnabled;
    protected virtual string SetSetGroupsPolicyName { get; set; } = ChatPermissions.DevicePermissions.SetGroups;

    public virtual async Task<int> SetGroupsAsync(Guid id, List<Guid> groupIdList)
    {
        await CheckPolicyAsync(SetSetGroupsPolicyName);
        return await SetAppDeviceGroupsAsync(id, groupIdList);
    }
    /// <summary>
    /// 注册设备
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[AllowAnonymous]
    [Volo.Abp.Auditing.DisableAuditing]
    public virtual async Task<DeviceDetailDto> RegisterAsync(DeviceCreateOrUpdateInput input)
    {

        return await RegisterAppDeviceAsync(input);
    }

    /// <summary>
    /// 当前用户登录设备
    /// </summary>
    /// <returns></returns>
    public virtual Task<PagedResultDto<DeviceDto>> GetListByCurrentUserAsync()
    {
        Assert.If(!CurrentUser.Id.HasValue, "未登录");

        return GetListAsync(new DeviceGetListInput()
        {
            UserId = DeviceUser() ?? throw new Volo.Abp.BusinessException("AppDevice:InvalidIdentity"),
            MaxResultCount = 999
        });
    }

    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <param name="deivceId"></param>
    /// <returns></returns>
    public virtual async Task<DeviceDetailDto> GetByDeviceIdAsync(string deivceId)
    {
        return await GetAppDeviceByRawIdAsync(deivceId);
    }
}
