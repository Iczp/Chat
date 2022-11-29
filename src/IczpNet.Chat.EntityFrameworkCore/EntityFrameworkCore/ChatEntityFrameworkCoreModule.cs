using IczpNet.AbpCommons.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace IczpNet.Chat.EntityFrameworkCore;

[DependsOn(
    typeof(ChatDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
[DependsOn(typeof(AbpCommonsEntityFrameworkCoreModule))]
[DependsOn(typeof(AbpIdentityEntityFrameworkCoreModule))]
public class ChatEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        base.PreConfigureServices(context);
        ChatEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ChatDbContext>(options =>
        {
            /* Add custom repositories here. Example:
             * options.AddRepository<Question, EfCoreQuestionRepository>();
             */
            options.AddDefaultRepositories(includeAllEntities: true);
        });
        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also IMMigrationsDbContextFactory for EF Core tooling. */
            //options.UseSqlServer();
            options.PreConfigure<ChatDbContext>(opts =>
            {
                // 1.安装 Microsoft.EntityFrameworkCore.Proxies 包到你的项目(通常是 EF Core 集成项目)
                // 2.为你的 DbContext 配置 UseLazyLoadingProxies(在 EF Core 项目的模块的 ConfigureServices 方法中). 
                // 3.https://docs.abp.io/zh-Hans/abp/latest/Entity-Framework-Core
                // 启用延时加载
                object value = opts.DbContextOptions.UseLazyLoadingProxies();
            });
        });
    }
}
