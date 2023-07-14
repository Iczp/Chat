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
using IczpNet.Chat.Authorizations;
using IczpNet.Chat.Permissions;
using Volo.Abp.PermissionManagement;
using FluentValidation;
using System.Linq;

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
//[DependsOn(typeof(PusherApplicationModule))]

public class ChatApplicationModule : AbpModule
{


    private static void ConfigureValidator(IServiceCollection services)
    {
        //services.AddValidatorsFromAssemblyContaining<UserValidator>();
        //services.AddValidatorsFromAssemblyContaining<PersonValidator>();
        //services.AddScoped<IValidator<ConfigInput>, ConfigInputValidator>();

        var entityTypes = typeof(ChatApplicationModule).Assembly.GetTypes()//.GetExportedTypes()
               .Where(t => !t.IsAbstract && typeof(IValidator).IsAssignableFrom(t))
               .ToList();

        foreach (var entityType in entityTypes)
        {
            services.AddValidatorsFromAssemblyContaining(entityType);
        }
    }


    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatApplicationModule>();

        //ConfigureValidator(context.Services);

        Configure<PermissionManagementOptions>(options =>
        {
            //options.ManagementProviders.Add<ChatObjectPermissionManagementProvider>();
        });

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ChatApplicationModule>(validate: true);
        });

        Configure<AuthorizationOptions>(options =>
        {
            //TODO: Rename UpdatePolicy/DeletePolicy since it's candidate to conflicts with other modules!
            //options.AddPolicy("BloggingUpdatePolicy", policy => policy.Requirements.Add(CommonOperations.Update));
            //options.AddPolicy("BloggingDeletePolicy", policy => policy.Requirements.Add(CommonOperations.Delete));

            foreach (var item in SessionPermissionDefinitionConsts.GetAll())
            {
                var requirement = new SessionUnitPermissionRequirement(item);
                options.AddPolicy(item, policy => policy.Requirements.Add(requirement));
            }

            foreach (var item in ChatObjectPermissionDefinitionConsts.GetAll())
            {
                var requirement = new ChatObjectRequirement(item);
                options.AddPolicy(item, policy => policy.Requirements.Add(requirement));
            }
        });

        context.Services.AddSingleton<IAuthorizationHandler, SessionUnitAuthorizationHandler>();
        context.Services.AddSingleton<IAuthorizationHandler, ChatObjectAuthorizationHandler>();
    }
}
