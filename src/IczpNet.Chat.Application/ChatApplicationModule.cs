using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using Volo.Abp.Identity;
using Volo.Abp.FluentValidation;
using Microsoft.AspNetCore.Authorization;
using IczpNet.Chat.Authorizations;
using IczpNet.Chat.Permissions;
using Volo.Abp.PermissionManagement;
using FluentValidation;
using System.Linq;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatDomainModule), typeof(ChatApplicationContractsModule), typeof(AbpDddApplicationModule), typeof(AbpAutoMapperModule))]
[DependsOn(typeof(AbpCommonsApplicationModule))]
[DependsOn(typeof(AbpTreesApplicationModule))]
[DependsOn(typeof(AbpIdentityApplicationModule))]
[DependsOn(typeof(AbpFluentValidationModule))]
// PusherApplicationModule retired: Chat uses SignalR plus Redis distributed events.
public class ChatApplicationModule : AbpModule
{
    private static void ConfigureValidator(IServiceCollection services)
    {
        var entityTypes = typeof(ChatApplicationModule).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IValidator).IsAssignableFrom(t)).ToList();
        foreach (var entityType in entityTypes) services.AddValidatorsFromAssemblyContaining(entityType);
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatApplicationModule>();
        Configure<PermissionManagementOptions>(options => { });
        Configure<AbpAutoMapperOptions>(options => options.AddMaps<ChatApplicationModule>(validate: true));
        Configure<AuthorizationOptions>(options =>
        {
            foreach (var item in SessionPermissionDefinitionConsts.GetAll()) options.AddPolicy(item, policy => policy.Requirements.Add(new SessionUnitPermissionRequirement(item)));
            foreach (var item in ChatObjectPermissionDefinitionConsts.GetAll()) options.AddPolicy(item, policy => policy.Requirements.Add(new ChatObjectRequirement(item)));
        });
        context.Services.AddSingleton<IAuthorizationHandler, SessionUnitAuthorizationHandler>();
        context.Services.AddSingleton<IAuthorizationHandler, ChatObjectAuthorizationHandler>();
    }
}
