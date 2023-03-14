using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using Volo.Abp.Identity;
using Volo.Abp.FluentValidation;
using IczpNet.Pusher;

namespace IczpNet.Chat;
[DependsOn(
    typeof(ChatDomainModule),
    typeof(ChatApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule)
    )]
[DependsOn(typeof(AbpCommonsApplicationModule))]
[DependsOn(typeof(AbpTreesApplicationModule))]
[DependsOn(typeof(AbpIdentityApplicationModule))]
[DependsOn(typeof(AbpFluentValidationModule))]
[DependsOn(typeof(PusherApplicationModule))]

public class ChatApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ChatApplicationModule>(validate: true);
        });
    }
}
