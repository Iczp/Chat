using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
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
        await LazyServiceProvider.LazyGetRequiredService<IChatDeviceLogout>().LogoutConnectionsAsync(connectionIdList, reason);
        await DistributedEventBus.PublishAsync(new AbortEto()
        {
            ConnectionIdList = connectionIdList,
            Reason = reason,
        });
    }
}
