using IczpNet.Chat.ChatObjectTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        protected ILogger<ChatObjectDataSeedContributor> Logger { get; }
        protected IConfiguration Configuration { get; }
        protected ICurrentTenant CurrentTenant { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        public ChatObjectDataSeedContributor(
            IConfiguration configuration,
            ICurrentTenant currentTenant,
            ILogger<ChatObjectDataSeedContributor> logger,
            IChatObjectManager chatObjectManager)
        {
            Configuration = configuration;
            CurrentTenant = currentTenant;
            Logger = logger;
            ChatObjectManager = chatObjectManager;
        }

        [UnitOfWork]
        public async Task SeedAsync(DataSeedContext context)
        {
            using (CurrentTenant.Change(context?.TenantId))
            {
                await CreateAsync();
            }
        }

        private async Task CreateAsync()
        {
            await ChatObjectManager.GetOrAddGroupAssistantAsync();

            await ChatObjectManager.GetOrAddPrivateAssistantAsync();
        }
    }
}
