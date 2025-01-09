using IczpNet.Chat.Localization;
using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Identity;
using Volo.Abp.Imaging;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
//[DependsOn(typeof(AbpCommonsHttpApiModule))]
[DependsOn(typeof(AbpIdentityHttpApiModule))]
[DependsOn(typeof(AbpImagingImageSharpModule))]
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
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<ChatResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
