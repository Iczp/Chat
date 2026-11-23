using IczpNet.Chat.Localization;
using Microsoft.AspNetCore.SignalR;
using IczpNet.Chat.ChatHubs;
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
        Configure<Volo.Abp.AspNetCore.ExceptionHandling.AbpExceptionHttpStatusCodeOptions>(options =>
        {
            options.Map(ChatErrorCodes.TemplateMessages.TemplateNotFound, System.Net.HttpStatusCode.NotFound);
            options.Map(ChatErrorCodes.TemplateMessages.TemplateForbidden, System.Net.HttpStatusCode.Forbidden);
            foreach (var code in new[] { ChatErrorCodes.TemplateMessages.TemplateDisabled,
                ChatErrorCodes.TemplateMessages.TemplateVersionNotPublished, ChatErrorCodes.TemplateMessages.TemplateConcurrencyConflict })
                options.Map(code, System.Net.HttpStatusCode.Conflict);
            foreach (var code in new[] { ChatErrorCodes.TemplateMessages.TemplateSchemaUnsupported,
                ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid, ChatErrorCodes.TemplateMessages.TemplateDataInvalid,
                ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge })
                options.Map(code, System.Net.HttpStatusCode.BadRequest);
        });
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
