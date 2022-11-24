using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using IczpNet.AbpCommons;
using IczpNet.AbpTrees;

namespace IczpNet.Chat;
[DependsOn(
    typeof(ChatDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
[DependsOn(typeof(AbpCommonsApplicationContractsModule))]
[DependsOn(typeof(AbpTreesApplicationContractsModule))]
public class ChatApplicationContractsModule : AbpModule
{

}
