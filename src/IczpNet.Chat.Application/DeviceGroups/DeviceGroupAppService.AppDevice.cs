using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using IczpNet.Chat.Devices;
using DeviceOptions = IczpNet.Chat.Devices.DeviceOptions;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Security.Claims;
using TargetManager=IczpNet.AppDevice.Devices.IDeviceManager;
using TargetRepository=IczpNet.AppDevice.Devices.IDeviceRepository;

namespace IczpNet.Chat.DeviceGroups;

public partial class DeviceGroupAppService
{
    private void EnsureDeviceWritable()
    {
        if(!UseAppDevice && LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.MigrationReadOnly)
            throw new BusinessException("AppDevice:MigrationReadOnly");
    }
    private bool UseAppDevice=>LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.UseAppDevice;
    private TargetRepository Store=>LazyServiceProvider.LazyGetRequiredService<TargetRepository>();
    private IczpNet.AppDevice.DeviceGroups.IDeviceGroupManager Rules=>LazyServiceProvider.LazyGetRequiredService<IczpNet.AppDevice.DeviceGroups.IDeviceGroupManager>();
    private async Task<T> InDeviceScope<T>(string requested,Func<Guid?,string,Task<T>> action)
    {
        var app=IczpNet.AppDevice.Devices.Device.NormalizeIdentity(requested ?? LazyServiceProvider.LazyGetRequiredService<ICurrentDevice>().Get().AppId ??
            LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.DefaultAppId,64);
        var principal=LazyServiceProvider.LazyGetRequiredService<ICurrentPrincipalAccessor>().Principal;
        LazyServiceProvider.LazyGetRequiredService<TargetManager>().RequireClientApp(principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value,app);
        using var scope=CurrentTenant.Change(IczpNet.AppDevice.Devices.DeviceManager.ResolveTenantId(principal.Claims));return await action(CurrentTenant.Id,app);
    }
    private async Task<Guid> TargetId(Guid? t,string a,Guid id)=>(await Store.FindLegacyAsync(t,a,LegacyDeviceImport.ChatSource,"Group",id))?.TargetId ?? id;
    private async Task<List<DeviceGroupDetailDto>> Details(Guid? t,string a,IReadOnlyList<IczpNet.AppDevice.DeviceGroups.DeviceGroup> groups)
    {
        var ids=groups.Select(x=>x.Id).ToArray();
        var aliases=(await Store.LegacyTargetsAsync(t,a,LegacyDeviceImport.ChatSource,"Group",ids)).GroupBy(x=>x.TargetId).ToDictionary(x=>x.Key,x=>x.First().LegacyId);
        var counts=await Store.MemberCountsAsync(t,a,ids);
        return groups.Select(x=>new DeviceGroupDetailDto{Id=aliases.GetValueOrDefault(x.Id,x.Id),AppId=a,Name=x.Name,Description=x.Description,
            DeviceCount=counts.GetValueOrDefault(x.Id),ConcurrencyStamp=x.ConcurrencyStamp}).ToList();
    }
    public override async Task<DeviceGroupDetailDto> GetAsync(Guid id)
    {
        if(!UseAppDevice)return await base.GetAsync(id);await CheckGetPolicyAsync(id);
        return await InDeviceScope(null,async(t,a)=>(await Details(t,a,new[]{await Store.GetGroupAsync(t,a,await TargetId(t,a,id))})).Single());
    }
    public override async Task<List<DeviceGroupDetailDto>> GetManyAsync(List<Guid> ids)
    {
        if(!UseAppDevice)return await base.GetManyAsync(ids);await CheckGetPolicyAsync();
        if(ids.Count>200)throw new BusinessException("AppDevice:BatchLimit");
        return await InDeviceScope(null,async(t,a)=>await Details(t,a,await Store.ReadGroupsAsync(t,a,(await Store.ResolveLegacyIdsAsync(t,a,LegacyDeviceImport.ChatSource,"Group",ids)).Values.Distinct().ToArray())));
    }
    public override async Task<PagedResultDto<DeviceGroupDto>> GetListAsync(DeviceGroupGetListInput input)
    {
        if(!UseAppDevice)return await base.GetListAsync(input);await CheckGetListPolicyAsync(input);
        if(input.SkipCount is <0 or >1000 || input.MaxResultCount is <1 or >1000)throw new BusinessException("AppDevice:BatchLimit");
        return await InDeviceScope(input.AppId,async(t,a)=>
        {
            var rows=new List<DeviceGroupDto>();
            var groups=await Store.LegacyGroupPageAsync(t,a,input.SkipCount,input.MaxResultCount,input.Sorting,string.IsNullOrWhiteSpace(input.Keyword)?null:input.Keyword);
            foreach(var batch in groups.Chunk(200))rows.AddRange(await Details(t,a,batch));
            return new PagedResultDto<DeviceGroupDto>(await Store.GroupCountAsync(t,a,input.Keyword),rows);
        });
    }
    public override async Task<DeviceGroupDetailDto> CreateAsync(DeviceGroupCreateInput input)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice)return await base.CreateAsync(input);await CheckCreatePolicyAsync();
        return await InDeviceScope(input.AppId,async(t,a)=>
        {
            var group=await Rules.SaveGroupAsync(t,a,null,input.Name,input.Description,null,false);
            await UnitOfWorkManager.Current.SaveChangesAsync();return (await Details(t,a,new[]{group})).Single();
        });
    }
    public override async Task<DeviceGroupDetailDto> UpdateAsync(Guid id,DeviceGroupUpdateInput input)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice)return await base.UpdateAsync(id,input);await CheckUpdatePolicyAsync();
        return await InDeviceScope(input.AppId,async(t,a)=>
        {
            var group=await Rules.SaveGroupAsync(t,a,await TargetId(t,a,id),input.Name,input.Description,input.ConcurrencyStamp,false);
            await UnitOfWorkManager.Current.SaveChangesAsync();return (await Details(t,a,new[]{group})).Single();
        });
    }
    public override async Task DeleteAsync(Guid id)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice){await base.DeleteAsync(id);return;}await CheckDeletePolicyAsync();
        await InDeviceScope(null,async(t,a)=>{var group=await Store.GetGroupAsync(t,a,await TargetId(t,a,id));
            await Rules.SaveGroupAsync(t,a,group.Id,group.Name,group.Description,group.ConcurrencyStamp,delete:true);return true;});
    }
    public override async Task DeleteManyAsync(List<Guid> ids)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice){await base.DeleteManyAsync(ids);return;}if(ids.Count>200)throw new BusinessException("AppDevice:BatchLimit");
        foreach(var id in ids)await DeleteAsync(id);
    }
    private Task<int> SetAppDeviceMembersAsync(Guid id,List<Guid> devices)=>InDeviceScope(null,async(t,a)=>
    {
        if(devices.Count>1000)throw new BusinessException("AppDevice:BatchLimit");var targets=new List<Guid>();
        foreach(var batch in devices.Chunk(200))targets.AddRange((await Store.ResolveLegacyIdsAsync(t,a,LegacyDeviceImport.ChatSource,"Device",batch)).Values);
        return await Rules.ReplaceMembersAsync(t,a,await TargetId(t,a,id),targets);
    });
}
