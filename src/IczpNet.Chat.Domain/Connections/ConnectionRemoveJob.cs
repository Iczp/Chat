using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class ConnectionRemoveJob(
    IConnectionManager connectionManager,
    IConnectionPoolManager connectionPoolManager) : DomainService, IAsyncBackgroundJob<ConnectionRemoveJobArgs>
{
    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;

    [UnitOfWork]
    public async Task ExecuteAsync(ConnectionRemoveJobArgs args)
    {
        Logger.LogInformation($"Deleted ConnectionId:{args.ConnectionId}");

        await ConnectionManager.RemoveAsync(args.ConnectionId);

        //await ConnectionPoolManager.RemoveAsync(args.ConnectionId);
    }
}
