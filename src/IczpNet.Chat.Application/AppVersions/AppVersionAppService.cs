using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Permissions;
using IczpNet.AppDevice.AppVersions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Uow;
using TargetVersion = IczpNet.AppDevice.AppVersions.AppVersion;
using TargetVersionManager = IczpNet.AppDevice.AppVersions.AppVersionManager;

namespace IczpNet.Chat.AppVersions;

public partial class AppVersionAppService(IczpNet.AppDevice.AppVersions.IAppVersionRepository repository, TargetVersionManager appVersionManager)
    : CrudChatAppService<TargetVersion, AppVersionDetailDto, AppVersionDto, Guid, AppVersionGetListInput, AppVersionCreateInput, AppVersionUpdateInput>(repository), IAppVersionAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Delete;
    protected virtual string SetSetGroupsPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.SetGroups;
    public TargetVersionManager AppVersionManager { get; } = appVersionManager;

    protected override async Task<IQueryable<TargetVersion>> CreateFilteredQueryAsync(AppVersionGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input)).Include(x => x.DeviceGroups)
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
            .WhereIf(input.IsForce.HasValue, x => x.IsForce == input.IsForce)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.IsWidget.HasValue, x => x.IsWidget == input.IsWidget)
            .WhereIf(!string.IsNullOrEmpty(input.AppId), x => x.AppId == input.AppId)
            .WhereIf(!string.IsNullOrEmpty(input.Platform), x => x.Platform == input.Platform)
            .WhereIf(input.VersionCodeStart.HasValue, x => x.VersionCode >= input.VersionCodeStart)
            .WhereIf(input.VersionCodeEnd.HasValue, x => x.VersionCode < input.VersionCodeEnd);
    }

    protected override async Task<TargetVersion> GetEntityByIdAsync(Guid id)
    {
        var item = await repository.GetAsync(id);
        var tenant = VersionTenant(item.AppId);
        if (tenant != item.TenantId || tenant != CurrentTenant.Id)
            throw new EntityNotFoundException(typeof(TargetVersion), id);
        return await repository.GetScopedAsync(tenant, item.AppId, id);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task<int> SetGroupsAsync(Guid id, List<Guid> groupIdList)
    {
        await CheckPolicyAsync(SetSetGroupsPolicyName);
        var item = await repository.GetAsync(id);
        var tenant = VersionTenant(item.AppId);
        if (tenant != item.TenantId || tenant != CurrentTenant.Id)
            throw new EntityNotFoundException(typeof(TargetVersion), id);
        return await AppVersionManager.SetGroupsAsync(tenant, item.AppId, id, await ResolveAppDeviceGroupsAsync(item.AppId, groupIdList));
    }

    protected override async Task<TargetVersion> MapToEntityAsync(AppVersionCreateInput input)
    {
        input.AppId = IczpNet.AppDevice.Devices.Device.NormalizeIdentity(input.AppId, 50);
        var tenant = VersionTenant(input.AppId);
        if (tenant != CurrentTenant.Id) throw new BusinessException("AppDevice:InvalidIdentity");
        var groups = await ResolveAppDeviceGroupsAsync(input.AppId, input.DeviceGroupIdList);
        await AppVersionManager.ValidateGroupsAsync(tenant, input.AppId, groups);
        var item = new TargetVersion(GuidGenerator.Create(), tenant);
        ObjectMapper.Map<AppVersionCreateInput, TargetVersion>(input, item);
        item.SetGroups(groups);
        return item;
    }

    public override async Task<AppVersionDetailDto> UpdateAsync(Guid id, AppVersionUpdateInput input)
    {
        await CheckUpdatePolicyAsync();
        var existing = await GetEntityByIdAsync(id);
        input.AppId = IczpNet.AppDevice.Devices.Device.NormalizeIdentity(input.AppId, 50);
        if (!string.Equals(existing.AppId, input.AppId, StringComparison.Ordinal))
            throw new BusinessException("AppDevice:InvalidIdentity");
        return await base.UpdateAsync(id, input);
    }

    public virtual async Task<AppVersionDetailDto> GetLatest(GetLatestInput input)
    {
        input.AppId = IczpNet.AppDevice.Devices.Device.NormalizeIdentity(input.AppId, 50);
        var tenant = VersionTenant(input.AppId);
        if (tenant != CurrentTenant.Id) throw new BusinessException("AppDevice:InvalidIdentity");
        var latest = await AppVersionManager.GetLatestAsync(tenant, input.AppId, input.Platform, input.VersionCode, input.DeviceId);
        return latest == null ? null : await MapToGetOutputDtoAsync(latest);
    }
}
