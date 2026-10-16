using IczpNet.AppDevice;
using IczpNet.AppDevice.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatHttpApiHostModule),typeof(AppDeviceEntityFrameworkCoreModule))]
public class ChatAppDeviceHttpApiHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbContextOptions>(o=>o.Configure<AppDeviceDbContext>(c=>c.DbContextOptions.UseSqlServer(c.ConnectionString)));
        Configure<Volo.Abp.AspNetCore.ExceptionHandling.AbpExceptionHttpStatusCodeOptions>(o=>
        {
            foreach(var code in new[]{"InvalidIdentity","InvalidProfile","BatchLimit","InvalidSorting","UseBatchOperation"})
                o.Map("AppDevice:"+code,System.Net.HttpStatusCode.BadRequest);
            o.Map("AppDevice:ApplicationNotAllowed",System.Net.HttpStatusCode.Forbidden);
            o.Map("AppDevice:Unavailable",System.Net.HttpStatusCode.NotFound);
            o.Map("AppDevice:ConcurrencyConflict",System.Net.HttpStatusCode.Conflict);
            o.Map("AppDevice:GroupNameExists",System.Net.HttpStatusCode.Conflict);
        });
    }
}
