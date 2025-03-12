using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class OnConnectedDistributedEventHandler(
    IConnectionManager connectionManager) : DomainService, IDistributedEventHandler<OnConnectedEto>, ITransientDependency
{
    public IConnectionManager ConnectionManager { get; } = connectionManager;


    [UnitOfWork]
    public async Task HandleEventAsync(OnConnectedEto eventData)
    {
        //await ConnectionPoolManager.AddAsync(eventData);

        await ConnectionManager.CreateAsync(new Connection(eventData.ConnectionId, eventData.ChatObjectIdList)
        {
            AppUserId = eventData.AppUserId,
            IpAddress = eventData.IpAddress,
            ServerHostId = eventData.Host,
            ClientId = eventData.ClientId,
            DeviceId = eventData.DeviceId,
            BrowserInfo = eventData.BrowserInfo,
            DeviceInfo = eventData.DeviceInfo,
        });

        Logger.LogInformation($"处理事件：{eventData}");
    }
}
