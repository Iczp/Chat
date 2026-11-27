using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace IczpNet.Chat.Admin;

[DependsOn(
    typeof(ChatAdminApplicationContractsModule),
    typeof(ChatDomainModule),
    typeof(AbpDddApplicationModule)
)]
public class ChatAdminApplicationModule : AbpModule
{
}
