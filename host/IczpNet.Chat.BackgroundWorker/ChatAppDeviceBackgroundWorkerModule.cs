using IczpNet.AppDevice.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatBackgroundWorkerModule), typeof(AppDeviceEntityFrameworkCoreModule))]
public class ChatAppDeviceBackgroundWorkerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
        => Configure<AbpDbContextOptions>(options => options.Configure<AppDeviceDbContext>(
            config => config.DbContextOptions.UseSqlServer(config.ConnectionString)));
}
