using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
namespace IczpNet.Chat.ChatHubs;

public sealed class DeviceLogoutEventHandler(ICallerContextManager callers)
    : IDistributedEventHandler<DeviceChangedEto>, ITransientDependency
{
    public Task HandleEventAsync(DeviceChangedEto eventData) => callers.AbortDeviceSessionsAsync(eventData);
}
