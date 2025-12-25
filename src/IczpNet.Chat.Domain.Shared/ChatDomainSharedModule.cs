using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.Chat.Localization;
using IczpNet.Chat.Options;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace IczpNet.Chat;
[DependsOn(
    typeof(AbpValidationModule)
)]
[DependsOn(typeof(AbpCommonsDomainSharedModule))]
[DependsOn(typeof(AbpTreesDomainSharedModule))]
[DependsOn(typeof(AbpIdentityDomainSharedModule))]
    public class ChatDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.ConfigureConventionOptions<ChatDomainSharedModule>();
        // 自动扫描所有已加载程序集，注册所有 IConventionOptions
        context.Services.ConfigureAllConventionOptions();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ChatDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ChatResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Chat");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Chat", typeof(ChatResource));
        });
    }
}
