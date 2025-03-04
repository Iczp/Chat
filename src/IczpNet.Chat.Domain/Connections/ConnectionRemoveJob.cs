using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class ConnectionRemoveJob(IConnectionManager connectionManager) : DomainService, IAsyncBackgroundJob<ConnectionRemoveJobArgs>
{
    public IConnectionManager ConnectionManager { get; } = connectionManager;

    [UnitOfWork]
    public async Task ExecuteAsync(ConnectionRemoveJobArgs args)
    {
        Logger.LogInformation($"Delete ConnectionId:{args.ConnectionId}");

        await ConnectionManager.RemoveAsync(args.ConnectionId);
    }
}
