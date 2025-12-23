using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections;

public class ConnectedDistributedEventHandler(
    IAbpDistributedLock abpDistributedLock,
    IUnitOfWorkManager unitOfWorkManager,
    ILocalEventBus localEventBus,
    IConnectionManager connectionManager) : DomainService, IDistributedEventHandler<ConnectedEto>, ITransientDependency
{
    public IAbpDistributedLock AbpDistributedLock { get; } = abpDistributedLock;
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public ILocalEventBus LocalEventBus { get; } = localEventBus;
    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();

    protected virtual string HandlerName => nameof(ConnectedDistributedEventHandler);

    //[UnitOfWork]
    public async Task HandleEventAsync(ConnectedEto eventData)
    {
        var lockerName = $"{HandlerName}-messageId-{eventData.ConnectionId}";

        await using var handle = await AbpDistributedLock.TryAcquireAsync(lockerName);

        Logger.LogInformation("Handle=={handle},LockerName={LockerName}", handle, lockerName);

        if (handle == null)
        {
            Logger.LogInformation("Handle==null");
            return;
        }
        //await ConnectionPoolManager.AddAsync(eventData);

        if (eventData.Host!= CurrentHosted.Name)
        {
            Logger.LogInformation($"{nameof(ConnectedDistributedEventHandler)} 不在同一主机，不处理，CurrentHosted.Name={CurrentHosted.Name}");
            return;
        }

        // 分布式事件要开启工作单元
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

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

        await uow.CompleteAsync();

    }
}
