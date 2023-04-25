using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using Volo.Abp.Identity;
using Volo.Abp.FluentValidation;
using IczpNet.Pusher;
using Microsoft.AspNetCore.Authorization;

namespace IczpNet.Chat;
[DependsOn(
    typeof(ChatDomainModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule)
    )]


[DependsOn(typeof(ChatAdminApplicationContractsModule))]
[DependsOn(typeof(AbpCommonsApplicationModule))]
[DependsOn(typeof(AbpTreesApplicationModule))]
[DependsOn(typeof(AbpIdentityApplicationModule))]
[DependsOn(typeof(AbpFluentValidationModule))]
[DependsOn(typeof(PusherApplicationModule))]

public class ChatAdminApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatAdminApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ChatAdminApplicationModule>(validate: true);
        });
        Configure<AuthorizationOptions>(options =>
        {

        });
    }
}
