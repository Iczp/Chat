using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using IczpNet.AppDevice.Devices;
using Volo.Abp;
using Volo.Abp.Uow;
using Volo.Abp.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.ConnectionPools;

public class AbortService(IDistributedEventBus distributedEventBus) : DomainService, IAbortService
{
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;

    public Task AbortAsync(string connectionId, string reason)
    {
        return AbortAsync([connectionId], reason);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task AbortAsync(List<string> connectionIdList, string reason)
    {
        if (LazyServiceProvider.LazyGetRequiredService<IOptions<IczpNet.Chat.Devices.DeviceOptions>>().Value.UseAppDevice)
        {
            var devices = LazyServiceProvider.LazyGetRequiredService<IczpNet.AppDevice.Devices.IDeviceManager>();
            var principal = LazyServiceProvider.LazyGetRequiredService<ICurrentPrincipalAccessor>().Principal;
            var connections = await LazyServiceProvider.LazyGetRequiredService<IOnlineManager>().GetManyAsync(connectionIdList);
            using var tenant = CurrentTenant.Change(DeviceManager.ResolveTenantId(principal.Claims));
            foreach (var target in connections.Select(x => x.Value).Where(x => x?.UserId != null &&
                !string.IsNullOrWhiteSpace(x.AppId) && !string.IsNullOrWhiteSpace(x.DeviceId))
                .DistinctBy(x => (x.UserId, x.AppId, x.DeviceId))
                .OrderBy(x => x.AppId, StringComparer.Ordinal).ThenBy(x => x.DeviceId, StringComparer.Ordinal).ThenBy(x => x.UserId))
            {
                devices.RequireClientApp(principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value, target.AppId);
                try { await devices.LogoutAsync(CurrentTenant.Id, target.AppId, target.DeviceId, target.UserId, reason); }
                catch (BusinessException error) when (error.Code == "AppDevice:Unavailable")
                { /* A missing device/binding is already invalid to the device token validator. Still abort its old connection. */ }
            }
        }
        await DistributedEventBus.PublishAsync(new AbortEto()
        {
            ConnectionIdList = connectionIdList,
            Reason = reason,
        });
    }
}
