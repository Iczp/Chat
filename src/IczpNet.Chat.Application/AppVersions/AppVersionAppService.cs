using DeviceDetectorNET;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Dtos;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Permissions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.AppVersions;

/// <inheritdoc />
public class AppVersionAppService(
    IDeviceGroupRepository deviceGroupRepository,
    IAppVersionRepository repository) : CrudChatAppService<AppVersion, AppVersionDetailDto, AppVersionDto, Guid, AppVersionGetListInput, AppVersionCreateInput, AppVersionUpdateInput>(repository), IAppVersionAppService
{
    protected override string GetPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.GetItem;
    protected override string GetListPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.GetList;
    protected override string CreatePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Create;
    protected override string UpdatePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Update;
    protected override string DeletePolicyName { get; set; } = ChatPermissions.AppVersionPermissions.Delete;
    protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.SetIsEnabled;
    protected virtual string SetSetGroupsPolicyName { get; set; } = ChatPermissions.AppVersionPermissions.SetGroups;
    public IDeviceGroupRepository DeviceGroupRepository { get; } = deviceGroupRepository;

    /// <inheritdoc />
    protected override async Task<IQueryable<AppVersion>> CreateFilteredQueryAsync(AppVersionGetListInput input)
    {
        return (await base.CreateFilteredQueryAsync(input))

            //.WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Name.Contains(input.Keyword))
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled)
            .WhereIf(input.IsForce.HasValue, x => x.IsForce == input.IsForce)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.IsWidget.HasValue, x => x.IsWidget == input.IsWidget)

            .WhereIf(!string.IsNullOrEmpty(input.AppId), x => x.AppId == input.AppId)
            .WhereIf(!string.IsNullOrEmpty(input.Platform), x => x.Platform == input.Platform)

            .WhereIf(input.VersionCodeStart.HasValue, x => x.VersionCode >= input.VersionCodeStart)
            .WhereIf(input.VersionCodeEnd.HasValue, x => x.VersionCode < input.VersionCodeEnd)
            ;
    }

    /// <summary>
    /// 设置分组（非公开时，只有分组才能访问）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="groupIdList"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 最新版
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual async Task<AppVersionDetailDto> GetLatest(GetLatestInput input)
    {
        var query = await Repository.GetQueryableAsync();

        var latest = await query
            .WhereIf(!string.IsNullOrWhiteSpace(input.AppId), x => x.AppId == input.AppId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Platform), x => x.Platform == input.Platform)
            .WhereIf(input.VersionCode >= 0, x => x.VersionCode > input.VersionCode)
            .Where(x =>
                x.IsPublic ||
                x.AppVersionDeviceGroupList.Any(d =>
                    d.DeviceGroup.DeviceGroupMapList.Any(f =>
                        f.Device.DeviceId == input.DeviceId
                    )
                )
            )
            .OrderByDescending(x => x.VersionCode)
            .FirstOrDefaultAsync();   // <-- 非常重要
        return ObjectMapper.Map<AppVersion, AppVersionDetailDto>(latest);
    }
}
