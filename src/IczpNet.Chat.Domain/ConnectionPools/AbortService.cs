using System.Collections.Generic;
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

    public Task AbortAsync(List<string> connectionIdList, string reason)
    {
      return  DistributedEventBus.PublishAsync(new AbortEto()
        {
            ConnectionIdList = connectionIdList,
            Reason = reason,
        });
    }
}
