using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;
using Volo.Abp.Identity.MongoDB;

namespace IczpNet.Chat.MongoDB;

[DependsOn(
    typeof(ChatDomainModule),
    typeof(AbpMongoDbModule)
    )]
[DependsOn(typeof(AbpIdentityMongoDbModule))]
    public class ChatMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<ChatMongoDbContext>(options =>
        {
                /* Add custom repositories here. Example:
                 * distributedCacheEntryOptions.AddRepository<Question, MongoQuestionRepository>();
                 */
        });
    }
}
