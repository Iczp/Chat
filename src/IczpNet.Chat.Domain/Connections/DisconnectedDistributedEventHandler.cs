using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class DisconnectedDistributedEventHandler(
    ILocalEventBus localEventBus,
    IConnectionManager connectionManager) : DomainService, IDistributedEventHandler<DisconnectedEto>, ITransientDependency
{
    public ILocalEventBus LocalEventBus { get; } = localEventBus;
    public IConnectionManager ConnectionManager { get; } = connectionManager;

    [UnitOfWork]
    public async Task HandleEventAsync(DisconnectedEto eventData)
    {
        //await ConnectionPoolManager.AddAsync(eventData);
        //发布本地事件
        //await LocalEventBus.PublishAsync(eventData);

        Logger.LogWarning($"{nameof(DisconnectedDistributedEventHandler)} 处理事件[{nameof(DisconnectedEto)}] Strat：{eventData.ConnectionId}");

        await ConnectionManager.RemoveAsync(eventData.ConnectionId);

        Logger.LogWarning($"{nameof(DisconnectedDistributedEventHandler)} 处理事件[{nameof(DisconnectedEto)}] End：{eventData.ConnectionId}");
    }
}
