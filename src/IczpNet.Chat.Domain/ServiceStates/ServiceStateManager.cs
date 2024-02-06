using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ServiceStates;

public class ServiceStateManager : DomainService, IServiceStateManager
{
    protected IChatObjectManager ChatObjectManager { get; }
    protected IDistributedCache<ServiceStatusCacheItem, long> Cache { get; }

    public ServiceStateManager(IChatObjectManager chatObjectManager,
        IDistributedCache<ServiceStatusCacheItem, long> cache)
    {
        ChatObjectManager = chatObjectManager;
        Cache = cache;
    }

    public async Task<ServiceStatusCacheItem> GetAsync(long chatObjectId)
    {
        return await Cache.GetAsync(chatObjectId);
    }

    public async Task<ServiceStatusCacheItem> SetAsync(long chatObjectId, ServiceStatus status)
    {
        var val = new ServiceStatusCacheItem(chatObjectId, status);

        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        };
        await Cache.SetAsync(chatObjectId, val, options);

        Logger.LogInformation($"Set chatObjectId:{chatObjectId},ServiceStatus:{status}");

        Logger.LogInformation($"ServiceStatusCacheItem Set:{val}");

        return val;
    }

    public Task RemoveAsync(long chatObjectId)
    {
        return Cache.RemoveAsync(chatObjectId);
    }

    public async Task SetAppUserIdAsync(Guid appUserId, ServiceStatus status)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(appUserId);

        foreach (var chatObjectId in chatObjectIdList)
        {
            await SetAsync(chatObjectId, status);
            //await Cache.GetOrAddAsync(chatObjectId, () => Task.FromResult(new ServiceStatusCacheItem(status)));
        }
    }
}
