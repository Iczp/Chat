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
    typeof(ChatManagementApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule)
    )]
[DependsOn(typeof(AbpCommonsApplicationModule))]
[DependsOn(typeof(AbpTreesApplicationModule))]
[DependsOn(typeof(AbpIdentityApplicationModule))]
[DependsOn(typeof(AbpFluentValidationModule))]
[DependsOn(typeof(PusherApplicationModule))]

public class ChatManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatManagementApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ChatManagementApplicationModule>(validate: true);
        });
        Configure<AuthorizationOptions>(options =>
        {
            //TODO: Rename UpdatePolicy/DeletePolicy since it's candidate to conflicts with other modules!
            //options.AddPolicy("BloggingUpdatePolicy", policy => policy.Requirements.Add(CommonOperations.Update));
            //options.AddPolicy("BloggingDeletePolicy", policy => policy.Requirements.Add(CommonOperations.Delete));

          
        });

    }
}
