using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.ShortIds;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Domain;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.Identity;

namespace IczpNet.Chat;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ChatDomainSharedModule),
    typeof(AbpAutoMapperModule)
)]

[DependsOn(typeof(AbpCommonsDomainModule))]
[DependsOn(typeof(AbpTreesDomainModule))]
[DependsOn(typeof(AbpIdentityDomainModule))]
[DependsOn(typeof(AbpPermissionManagementDomainIdentityModule))]
public class ChatDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<ChatDomainModule>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ChatDomainModule>(validate: true);
        });

        Configure<ShortIdOptions>(options =>
        {
            options.Length = 10;
            options.UseNumbers = false;
            options.UseSpecialCharacters = false;
        });

    }
}
