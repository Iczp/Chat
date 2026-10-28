using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.SignalR;
using IczpNet.Chat.ChatHubs;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Identity;
using Volo.Abp.Imaging;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(IczpNet.OpenIddict.OpenIddictAspNetCoreModule),
    typeof(AbpAspNetCoreMvcModule))]
//[DependsOn(typeof(AbpCommonsHttpApiModule))]
[DependsOn(typeof(AbpIdentityHttpApiModule))]
[DependsOn(typeof(IczpNet.AbpCommons.AbpCommonsDomainModule))]
[DependsOn(typeof(AbpAspNetCoreSignalRModule))]
public class ChatHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ChatHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<DeviceSessionHubFilter>();
        Configure<HubOptions<ChatHub>>(options => options.AddFilter<DeviceSessionHubFilter>());
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<ChatResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
