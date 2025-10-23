using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.ChatHubs;

public class AbortDistributedEventHandler(ICallerContextManager callerContextManager) : DomainService, IDistributedEventHandler<AbortEto> //, ILocalEventHandler<AbortEto>
{
    public ICallerContextManager CallerContextManager { get; } = callerContextManager;

    public async Task HandleEventAsync(AbortEto eventData)
    {
        var connectionIdList = eventData.ConnectionIdList;

        var reason = eventData.Reason;

        Logger.LogInformation($"收到踢出事件 {nameof(eventData.ConnectionIdList)}:{connectionIdList}");

        foreach (var connectionId in connectionIdList)
        {
            await CallerContextManager.AbortAsync(connectionId, reason);
        }
    }
}
