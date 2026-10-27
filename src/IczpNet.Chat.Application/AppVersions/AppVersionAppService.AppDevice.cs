using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using IczpNet.Chat.DeviceGroups;
using Volo.Abp;
using Volo.Abp.Security.Claims;
using TargetManager = IczpNet.AppDevice.Devices.IDeviceManager;
using TargetRepository = IczpNet.AppDevice.Devices.IDeviceRepository;
using TargetVersion = IczpNet.AppDevice.AppVersions.AppVersion;

namespace IczpNet.Chat.AppVersions;

public partial class AppVersionAppService
{
    private TargetRepository Store => LazyServiceProvider.LazyGetRequiredService<TargetRepository>();
    private Guid? VersionTenant(string app)
    {
        app = IczpNet.AppDevice.Devices.Device.NormalizeIdentity(app, 50);
        var principal = LazyServiceProvider.LazyGetRequiredService<ICurrentPrincipalAccessor>().Principal;
        LazyServiceProvider.LazyGetRequiredService<TargetManager>().RequireClientApp(principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value, app);
        return DeviceExtensions.ResolveTenantId(principal.Claims);
    }

    private async Task<List<Guid>> ResolveAppDeviceGroupsAsync(string app, List<Guid> ids)
    {
        if (ids.Count > 200) throw new BusinessException("AppDevice:BatchLimit");
        using var scope = CurrentTenant.Change(VersionTenant(app));
        var targets = (await Store.ResolveLegacyIdsAsync(CurrentTenant.Id, app, LegacyDeviceImport.ChatSource, "Group", ids)).Values.Distinct().ToArray();
        return targets.ToList();
    }

    protected override async Task<AppVersionDetailDto> MapToGetOutputDtoAsync(TargetVersion entity)
    {
        var dto = await base.MapToGetOutputDtoAsync(entity);
        await FillGroups(new[] { (entity, (AppVersionDto)dto) });
        return dto;
    }

    protected override async Task<List<AppVersionDto>> MapToGetListOutputDtosAsync(List<TargetVersion> entities)
    {
        var rows = await base.MapToGetListOutputDtosAsync(entities);
        await FillGroups(entities.Zip(rows, (entity, dto) => (entity, dto)));
        return rows;
    }

    private async Task FillGroups(IEnumerable<(TargetVersion Entity, AppVersionDto Dto)> pairs)
    {
        foreach (var app in pairs.GroupBy(x => x.Entity.AppId))
        {
            var tenant = VersionTenant(app.Key);
            if (tenant != CurrentTenant.Id || app.Any(x => x.Entity.TenantId != tenant))
                throw new BusinessException("AppDevice:InvalidIdentity");
            var mapped = new Dictionary<Guid, DeviceGroupSampleDto>();
            foreach (var batch in app.SelectMany(x => x.Entity.DeviceGroups.Select(g => g.DeviceGroupId)).Distinct().Chunk(200))
            {
                var aliases = (await Store.LegacyTargetsAsync(tenant, app.Key, LegacyDeviceImport.ChatSource, "Group", batch))
                    .GroupBy(x => x.TargetId).ToDictionary(x => x.Key, x => x.OrderBy(v => v.LegacyId).First().LegacyId);
                foreach (var group in await Store.ReadGroupsAsync(tenant, app.Key, batch))
                    mapped[group.Id] = new DeviceGroupSampleDto { Id = aliases.GetValueOrDefault(group.Id, group.Id), Name = group.Name };
            }
            foreach (var pair in app)
                pair.Dto.Groups = pair.Entity.DeviceGroups.Select(x => x.DeviceGroupId).Where(mapped.ContainsKey).Select(x => mapped[x]).ToList();
        }
    }
}
