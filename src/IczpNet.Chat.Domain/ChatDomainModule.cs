using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ChatDomainSharedModule)
)]

[DependsOn(typeof(AbpCommonsDomainModule))]
[DependsOn(typeof(AbpTreesDomainModule))]
public class ChatDomainModule : AbpModule
{

}
