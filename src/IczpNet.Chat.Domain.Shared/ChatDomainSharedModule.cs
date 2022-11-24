using Volo.Abp.Modularity;
using Volo.Abp.Localization;
using IczpNet.Chat.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;

namespace IczpNet.Chat;
[DependsOn(
    typeof(AbpValidationModule)
)]
[DependsOn(typeof(AbpCommonsDomainSharedModule))]
[DependsOn(typeof(AbpTreesDomainSharedModule))]
public class ChatDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
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
