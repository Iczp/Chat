using IczpNet.Chat.Localization;
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
    typeof(IczpNet.AppDevice.AppDeviceAspNetCoreModule),
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
        Configure<IczpNet.AppDevice.DeviceParsingOptions>(options =>
        {
            // Browser SignalR clients send device metadata in the hub query string.
            options.AllowQuery = context.Services.GetConfiguration().GetValue("AppDevice:Parsing:AllowQuery", true);
        });
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<ChatResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
