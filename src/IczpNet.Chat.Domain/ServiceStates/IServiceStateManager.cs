using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.ServiceStates;

public interface IServiceStateManager : ITransientDependency
{
    Task<ServiceStatusCacheItem> GetAsync(long chatObjectId);

    Task<ServiceStatusCacheItem> SetAsync(long chatObjectId, ServiceStatus status);

    Task RemoveAsync(long chatObjectId);

    Task SetAppUserIdAsync(Guid appUserId, ServiceStatus status);
}
