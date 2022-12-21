using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using Volo.Abp.Identity;

namespace IczpNet.Chat;
[DependsOn(
    typeof(ChatDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
[DependsOn(typeof(AbpCommonsApplicationContractsModule))]
[DependsOn(typeof(AbpTreesApplicationContractsModule))]
[DependsOn(typeof(AbpIdentityApplicationContractsModule))]
public class ChatApplicationContractsModule : AbpModule
{

}
