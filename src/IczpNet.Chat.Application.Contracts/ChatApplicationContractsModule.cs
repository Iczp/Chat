using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(typeof(ChatDomainSharedModule), typeof(AbpDddApplicationContractsModule), typeof(AbpAuthorizationModule))]
[DependsOn(typeof(AbpCommonsApplicationContractsModule))]
[DependsOn(typeof(AbpTreesApplicationContractsModule))]
[DependsOn(typeof(AbpIdentityApplicationContractsModule))]
// PusherApplicationContractsModule retired: SignalR payloads are Chat contracts.
public class ChatApplicationContractsModule : AbpModule
{
}
