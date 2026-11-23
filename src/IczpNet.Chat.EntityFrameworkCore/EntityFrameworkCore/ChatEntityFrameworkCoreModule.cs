using IczpNet.AbpCommons.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Domain.Entities.Events;

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
        Configure<AbpEntityChangeOptions>(options =>
        {
            // 附件授权等关联变化不代表消息内容更新。实际消息更新由 Message 自身字段
            // （ContentJson、MessageChangeId、撤回状态等）的变化触发正常审计、版本递增和事件。
            options.IgnoredNavigationEntitySelectors.Add(new NamedTypeSelector(
                "Chat.Message.NavigationOnlyChanges", type => typeof(Message).IsAssignableFrom(type)));
        });

        context.Services.AddAbpDbContext<ChatDbContext>(options =>
        {
            /* Add custom repositories here. Example:
             * distributedCacheEntryOptions.AddRepository<Question, EfCoreQuestionRepository>();
             */
            options.AddDefaultRepositories(includeAllEntities: true);
        });
        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also IMMigrationsDbContextFactory for EF Core tooling. */
            //distributedCacheEntryOptions.UseSqlServer();
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
