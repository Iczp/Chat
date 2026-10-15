using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using IczpNet.Chat.DeviceGroups;
using IczpNet.Chat.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Security.Claims;
using TargetManager=IczpNet.AppDevice.Devices.DeviceManager;
using TargetRepository=IczpNet.AppDevice.Devices.IDeviceRepository;

namespace IczpNet.Chat.AppVersions;

public partial class AppVersionAppService
{
    private bool UseAppDevice=>LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.UseAppDevice;
    private TargetRepository Store=>LazyServiceProvider.LazyGetRequiredService<TargetRepository>();
    private Guid? VersionTenant(string app)
    {
        var principal=LazyServiceProvider.LazyGetRequiredService<ICurrentPrincipalAccessor>().Principal;
        LazyServiceProvider.LazyGetRequiredService<TargetManager>().RequireClientApp(principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value,app);
        return TargetManager.ResolveTenantId(principal.Claims);
    }
    private async Task CheckAppDeviceGroupsAsync(string app,List<Guid> ids)
    {
        if(ids.Count>200)throw new BusinessException("AppDevice:BatchLimit");
        using var scope=CurrentTenant.Change(VersionTenant(app));
        var targets=(await Store.ResolveLegacyIdsAsync(CurrentTenant.Id,app,LegacyDeviceImport.ChatSource,"Group",ids)).Values.Distinct().ToArray();
        if((await Store.ReadGroupsAsync(CurrentTenant.Id,app,targets)).Count!=targets.Length)throw new BusinessException("AppDevice:Unavailable");
    }
    private async Task<List<Guid>> VersionGroupsForDeviceAsync(string app,string raw)
    {
        if(!UseAppDevice)
        {
            var groups=await DeviceGroupRepository.GetQueryableAsync();
            return await groups.Where(g=>g.DeviceGroupMapList.Any(m=>m.Device.DeviceId==raw)).Select(g=>g.Id).ToListAsync();
        }
        using var scope=CurrentTenant.Change(VersionTenant(app));
        if(string.IsNullOrWhiteSpace(raw))return new List<Guid>();
        var ids=await Store.ResolveIdsAsync(CurrentTenant.Id,app,new[]{raw});
        var device=(await Store.ReadAsync(CurrentTenant.Id,app,ids.Values.ToArray())).SingleOrDefault();
        if(device==null || !device.IsEnabled)return new List<Guid>();
        var allowed=device.GroupIds.ToList();
        foreach(var batch in device.GroupIds.Chunk(200))allowed.AddRange((await Store.LegacyTargetsAsync(CurrentTenant.Id,app,LegacyDeviceImport.ChatSource,"Group",batch)).Select(x=>x.LegacyId));
        if(allowed.Count>1000)throw new BusinessException("AppDevice:BatchLimit");
        return allowed.Distinct().ToList();
    }
    protected override async Task<AppVersionDetailDto> MapToGetOutputDtoAsync(AppVersion entity)
    {
        var dto=await base.MapToGetOutputDtoAsync(entity);await FillGroups(new[]{(entity,(AppVersionDto)dto)});return dto;
    }
    protected override async Task<List<AppVersionDto>> MapToGetListOutputDtosAsync(List<AppVersion> entities)
    {
        var rows=await base.MapToGetListOutputDtosAsync(entities);
        await FillGroups(entities.Zip(rows,(entity,dto)=>(entity,dto)));return rows;
    }
    private async Task FillGroups(IEnumerable<(AppVersion Entity,AppVersionDto Dto)> pairs)
    {
        foreach(var app in pairs.GroupBy(x=>x.Entity.AppId))
        {
            var mapped=new Dictionary<Guid,DeviceGroupSampleDto>();
            foreach(var batch in app.SelectMany(x=>x.Entity.AppVersionDeviceGroupList.Select(g=>g.DeviceGroupId)).Distinct().Chunk(200))
            {
                if(UseAppDevice)
                {
                    using var scope=CurrentTenant.Change(VersionTenant(app.Key));
                    var targets=await Store.ResolveLegacyIdsAsync(CurrentTenant.Id,app.Key,LegacyDeviceImport.ChatSource,"Group",batch);
                    var groups=(await Store.ReadGroupsAsync(CurrentTenant.Id,app.Key,targets.Values.Distinct().ToArray())).ToDictionary(x=>x.Id);
                    foreach(var id in batch)if(groups.TryGetValue(targets[id],out var group))
                        mapped[id]=new DeviceGroupSampleDto{Id=id,Name=group.Name};
                }
                else
                {
                    var groups=await DeviceGroupRepository.GetQueryableAsync();
                    foreach(var group in await groups.Where(x=>batch.Contains(x.Id)).ToListAsync())mapped[group.Id]=new DeviceGroupSampleDto{Id=group.Id,Name=group.Name};
                }
            }
            foreach(var pair in app)pair.Dto.Groups=pair.Entity.AppVersionDeviceGroupList.Select(x=>x.DeviceGroupId).Where(mapped.ContainsKey).Select(x=>mapped[x]).ToList();
        }
    }
}
