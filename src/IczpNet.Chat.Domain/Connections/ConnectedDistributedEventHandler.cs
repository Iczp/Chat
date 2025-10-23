using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class ConnectedDistributedEventHandler(

    ILocalEventBus localEventBus,
    IConnectionManager connectionManager) : DomainService, IDistributedEventHandler<ConnectedEto>, ITransientDependency
{
    public ILocalEventBus LocalEventBus { get; } = localEventBus;
    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();

    [UnitOfWork]
    public async Task HandleEventAsync(ConnectedEto eventData)
    {
        //await ConnectionPoolManager.AddAsync(eventData);

        if(eventData.Host!= CurrentHosted.Name)
        {
            Logger.LogInformation($"{nameof(ConnectedDistributedEventHandler)} 不在同一主机，不处理，CurrentHosted.Name={CurrentHosted.Name}");
            return;
        }

        await ConnectionManager.CreateAsync(new Connection(eventData.ConnectionId, eventData.ChatObjectIdList)
        {
            AppUserId = eventData.UserId,
            IpAddress = eventData.IpAddress,
            ServerHostId = eventData.Host,
            ClientId = eventData.ClientId,
            DeviceId = eventData.DeviceId,
            BrowserInfo = eventData.BrowserInfo,
            DeviceInfo = eventData.DeviceInfo,
        });

        Logger.LogInformation($"{nameof(ConnectedDistributedEventHandler)} 处理事件：{eventData}");

    }
}
