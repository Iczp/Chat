using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using IczpNet.Chat.DeviceGroups;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Security.Claims;
using TargetDevice = IczpNet.AppDevice.Devices.Device;
using TargetManager = IczpNet.AppDevice.Devices.DeviceManager;
using TargetRepository = IczpNet.AppDevice.Devices.IDeviceRepository;

namespace IczpNet.Chat.Devices;

public partial class DeviceAppService
{
    private void EnsureDeviceWritable()
    {
        if(!UseAppDevice && LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.MigrationReadOnly)
            throw new BusinessException("AppDevice:MigrationReadOnly");
    }
    private bool UseAppDevice => LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.UseAppDevice;
    private TargetRepository DeviceStore => LazyServiceProvider.LazyGetRequiredService<TargetRepository>();
    private TargetManager DeviceRules => LazyServiceProvider.LazyGetRequiredService<TargetManager>();
    private string App(string requested = null) => TargetDevice.NormalizeIdentity(requested ??
        LazyServiceProvider.LazyGetRequiredService<ICurrentDevice>().Get().AppId ??
        LazyServiceProvider.LazyGetRequiredService<IOptions<DeviceOptions>>().Value.DefaultAppId, 64);
    private async Task<T> InDeviceScope<T>(string app, Func<Guid?,string,Task<T>> action)
    {
        var principal = LazyServiceProvider.LazyGetRequiredService<ICurrentPrincipalAccessor>().Principal;
        DeviceRules.RequireClientApp(principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value, app);
        using var scope = CurrentTenant.Change(TargetManager.ResolveTenantId(principal.Claims));
        return await action(CurrentTenant.Id, app);
    }
    private Guid? DeviceUser()
    {
        var principal=LazyServiceProvider.LazyGetRequiredService<ICurrentPrincipalAccessor>().Principal;
        var subject=principal.FindFirst("sub")?.Value ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var client=principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value;
        return subject != client && Guid.TryParse(subject,out var id) ? id : null;
    }
    private async Task<Guid> TargetId(Guid? tenant,string app,Guid id,string kind="Device")
        => (await DeviceStore.FindLegacyAsync(tenant,app,LegacyDeviceImport.ChatSource,kind,id))?.TargetId ?? id;
    private static DeviceProfile Profile(DeviceCreateInput x) => new(x.Name,x.Platform,x.DeviceType,x.DeviceBrand ?? x.Brand,x.DeviceModel ?? x.Model,x.OsVersion,x.AppVersion);
    private static string LegacyJson(DeviceCreateInput input)
    {
        // Credential material is never placed in profile JSON or audit payloads.
        var properties=typeof(DeviceCreateInput).GetProperties().Where(p=>p.Name is not ("RegistrationProof" or "ConcurrencyStamp" or "IsEnabled"));
        return JsonSerializer.Serialize(properties.ToDictionary(p=>p.Name,p=>p.GetValue(input)));
    }
    private async Task<List<DeviceDetailDto>> Details(Guid? tenant,string app,IReadOnlyList<DeviceSnapshot> snapshots)
    {
        var result=new List<DeviceDetailDto>();
        foreach(var batch in snapshots.Chunk(200))
        {
            var ids=batch.Select(x=>x.Id).ToArray();
            var entities=(await DeviceStore.ReadEntitiesAsync(tenant,app,ids)).ToDictionary(x=>x.Id);
            var aliases=(await DeviceStore.LegacyTargetsAsync(tenant,app,LegacyDeviceImport.ChatSource,"Device",ids))
                .GroupBy(x=>x.TargetId).ToDictionary(x=>x.Key,x=>x.OrderBy(v=>v.LegacyId).First().LegacyId);
            var groups=new Dictionary<Guid,DeviceGroupSampleDto>();
            foreach(var groupBatch in batch.SelectMany(x=>x.GroupIds).Distinct().Chunk(200))
            {
                var old=(await DeviceStore.LegacyTargetsAsync(tenant,app,LegacyDeviceImport.ChatSource,"Group",groupBatch))
                    .GroupBy(x=>x.TargetId).ToDictionary(x=>x.Key,x=>x.First().LegacyId);
                foreach(var group in await DeviceStore.ReadGroupsAsync(tenant,app,groupBatch))
                    groups[group.Id]=new DeviceGroupSampleDto{Id=old.GetValueOrDefault(group.Id,group.Id),Name=group.Name};
            }
            foreach(var snapshot in batch)
            {
                var entity=entities[snapshot.Id];
                var dto=entity.LegacyProfileJson==null ? new DeviceDetailDto() : JsonSerializer.Deserialize<DeviceDetailDto>(entity.LegacyProfileJson);
                dto.Id=aliases.GetValueOrDefault(entity.Id,entity.Id);dto.AppId=app;dto.DeviceId=entity.DeviceId;dto.Name=entity.Name;dto.Platform=entity.Platform;
                dto.DeviceType=entity.DeviceType;dto.Brand=entity.Brand;dto.DeviceBrand=entity.Brand;dto.Model=entity.Model;dto.DeviceModel=entity.Model;
                dto.AppVersion=entity.AppVersion;dto.OsVersion=entity.OsVersion;dto.IsEnabled=entity.IsEnabled;dto.ConcurrencyStamp=entity.ConcurrencyStamp;
                dto.RegistrationProof=null;dto.LastModificationTime=entity.LastModificationTime;
                dto.Groups=snapshot.GroupIds.Where(groups.ContainsKey).Select(x=>groups[x]).ToList();result.Add(dto);
            }
        }
        return result;
    }
    private async Task<DeviceDetailDto> Detail(Guid? tenant,string app,Guid id)
        => (await Details(tenant,app,await DeviceStore.ReadAsync(tenant,app,new[]{id}))).SingleOrDefault()
            ?? throw new BusinessException("AppDevice:Unavailable");
    private async Task<DeviceDetailDto> RegisterAppDeviceAsync(DeviceCreateOrUpdateInput input)
        => await InDeviceScope(App(input.AppId),async(tenant,app)=>
        {
            var result=await DeviceRules.RegisterAsync(tenant,app,input.DeviceId,Profile(input),input.RegistrationProof,DeviceUser(),false);
            result.Device.SetLegacyProfile(LegacyJson(input));
            await UnitOfWorkManager.Current.SaveChangesAsync();
            var dto=await Detail(tenant,app,result.Device.Id);dto.RegistrationProof=result.Proof;return dto;
        });
    private Task<DeviceDetailDto> GetAppDeviceByRawIdAsync(string raw)
        => InDeviceScope(App(),async(tenant,app)=>
        {
            await CheckGetPolicyAsync();var device=await DeviceStore.FindIdentityAsync(tenant,app,raw);
            if(device==null)throw new BusinessException("AppDevice:Unavailable");
            return await Detail(tenant,app,device.Id);
        });
    public override async Task<DeviceDetailDto> GetAsync(Guid id)
    {
        if(!UseAppDevice)return await base.GetAsync(id);await CheckGetPolicyAsync(id);
        return await InDeviceScope(App(),async(t,a)=>await Detail(t,a,await TargetId(t,a,id)));
    }
    public override async Task<List<DeviceDetailDto>> GetManyAsync(List<Guid> ids)
    {
        if(!UseAppDevice)return await base.GetManyAsync(ids);
        if(ids.Count>200)throw new BusinessException("AppDevice:BatchLimit");
        await CheckGetPolicyAsync();
        return await InDeviceScope(App(),async(t,a)=>
        {
            var targets=(await DeviceStore.ResolveLegacyIdsAsync(t,a,LegacyDeviceImport.ChatSource,"Device",ids)).Values.Distinct().ToArray();
            return await Details(t,a,await DeviceStore.ReadAsync(t,a,targets));
        });
    }
    public override async Task<PagedResultDto<DeviceDto>> GetListAsync(DeviceGetListInput input)
    {
        if(!UseAppDevice)return await base.GetListAsync(input);await CheckGetListPolicyAsync(input);
        return await InDeviceScope(App(input.AppId),async(t,a)=>
        {
            Guid? group=input.DeviceGroupId.HasValue ? await TargetId(t,a,input.DeviceGroupId.Value,"Group") : null;
            var keyword=string.IsNullOrWhiteSpace(input.Keyword)?null:input.Keyword;
            var platform=string.IsNullOrWhiteSpace(input.Platform)?null:input.Platform;
            var raw=string.IsNullOrWhiteSpace(input.DeviceId)?null:input.DeviceId;
            var rows=await DeviceStore.LegacyPageAsync(t,a,input.SkipCount,input.MaxResultCount,input.Sorting,keyword,raw,platform,input.IsEnabled,input.UserId,group);
            var total=await DeviceStore.CountAsync(t,a,keyword,platform,input.IsEnabled,input.UserId,group,rawId:raw);
            return new PagedResultDto<DeviceDto>(total,(await Details(t,a,rows)).Cast<DeviceDto>().ToList());
        });
    }
    public override async Task<DeviceDetailDto> CreateAsync(DeviceCreateInput input)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice)return await base.CreateAsync(input);await CheckCreatePolicyAsync();
        return await InDeviceScope(App(input.AppId),async(t,a)=>
        {
            var result=await DeviceRules.RegisterAsync(t,a,input.DeviceId,Profile(input),null,null,false);
            result.Device.SetEnabled(input.IsEnabled);result.Device.SetLegacyProfile(LegacyJson(input));
            await UnitOfWorkManager.Current.SaveChangesAsync();return await Detail(t,a,result.Device.Id);
        });
    }
    public override async Task<DeviceDetailDto> UpdateAsync(Guid id,DeviceUpdateInput input)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice)return await base.UpdateAsync(id,input);await CheckUpdatePolicyAsync();
        return await InDeviceScope(App(input.AppId),async(t,a)=>
        {
            var target=await TargetId(t,a,id);
            await DeviceRules.EditAsync(t,a,target,input.ConcurrencyStamp,Profile(input),input.IsEnabled);
            (await DeviceStore.GetScopedAsync(t,a,target)).SetLegacyProfile(LegacyJson(input));
            await UnitOfWorkManager.Current.SaveChangesAsync();return await Detail(t,a,target);
        });
    }
    public override async Task DeleteAsync(Guid id)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice){await base.DeleteAsync(id);return;}await CheckDeletePolicyAsync();
        await InDeviceScope(App(),async(t,a)=>{var target=await TargetId(t,a,id);var item=await DeviceStore.GetScopedAsync(t,a,target);
            await DeviceRules.EditAsync(t,a,target,item.ConcurrencyStamp,null,null,delete:true);return true;});
    }
    public override async Task DeleteManyAsync(List<Guid> ids)
    {
        EnsureDeviceWritable();
        if(!UseAppDevice){await base.DeleteManyAsync(ids);return;}if(ids.Count>200)throw new BusinessException("AppDevice:BatchLimit");
        foreach(var id in ids)await DeleteAsync(id);
    }
    private Task<int> SetAppDeviceGroupsAsync(Guid id,List<Guid> groups)
        => InDeviceScope(App(),async(t,a)=>
        {
            if(groups.Count>200)throw new BusinessException("AppDevice:BatchLimit");
            var target=await TargetId(t,a,id);
            var wanted=(await DeviceStore.ResolveLegacyIdsAsync(t,a,LegacyDeviceImport.ChatSource,"Group",groups)).Values.Distinct().ToArray();
            return await DeviceRules.ReplaceGroupsAsync(t,a,target,wanted);
        });
}
