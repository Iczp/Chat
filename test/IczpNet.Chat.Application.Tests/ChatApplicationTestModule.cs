using Volo.Abp.Modularity;

namespace IczpNet.Chat;

[DependsOn(
    typeof(ChatApplicationModule),
    typeof(ChatDomainTestModule)
    )]
public class ChatApplicationTestModule : AbpModule
{

}
