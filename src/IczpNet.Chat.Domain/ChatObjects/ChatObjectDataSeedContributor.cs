using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectDataSeedContributor(
    IConfiguration configuration,
    ICurrentTenant currentTenant,
    ILogger<ChatObjectDataSeedContributor> logger,
    IChatObjectManager chatObjectManager) : IDataSeedContributor, ITransientDependency
{
    protected ILogger<ChatObjectDataSeedContributor> Logger { get; } = logger;
    protected IConfiguration Configuration { get; } = configuration;
    protected ICurrentTenant CurrentTenant { get; } = currentTenant;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
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

        await ChatObjectManager.GetOrAddNewsAsync();

        await ChatObjectManager.GetOrAddNotifyAsync();
    }
}
