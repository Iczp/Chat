using IczpNet.Chat.Connections;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionRemovedEventHandler(IBackgroundJobManager backgroundJobManager) : DomainService, ILocalEventHandler<ConnectionRemovedEventData>
{
    public IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;

    public async Task HandleEventAsync(ConnectionRemovedEventData eventData)
    {
        Logger.LogInformation($"[{nameof(ConnectionRemovedEventHandler)}] Deleted ConnectionId:{eventData.ConnectionId}");
        await BackgroundJobManager.EnqueueAsync(new ConnectionRemoveJobArgs(eventData.ConnectionId), BackgroundJobPriority.High);
    }
}
