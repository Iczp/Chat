using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SetLists;

public class DistributedCacheListSet<TListItem, TKey>(
    IDistributedCache<IEnumerable<TListItem>, TKey> distributedCache) : DomainService, IDistributedCacheListSet<TListItem, TKey> where TListItem : class
{
    public IDistributedCache<IEnumerable<TListItem>, TKey> DistributedCache { get; } = distributedCache;

    public async Task<IEnumerable<TListItem>> AddAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = await GetAllInternalAsync(key, optionsFactory, hideErrors, considerUow, token);

        list = list.Union(items).Distinct();

        await DistributedCache.SetAsync(key, list, optionsFactory?.Invoke(), hideErrors, considerUow, token);

        return list.AsEnumerable();
    }

    public async Task<IEnumerable<TListItem>> RemoveAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = await GetAllInternalAsync(key, optionsFactory, hideErrors, considerUow, token);

        list = list.Except(items).Distinct();

        await DistributedCache.SetAsync(key, list, optionsFactory?.Invoke(), hideErrors, considerUow, token);

        return list.AsEnumerable();
    }

    internal async Task<IEnumerable<TListItem>> GetAllInternalAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = await DistributedCache.GetOrAddAsync(key, () =>
        {
            return Task.FromResult(new List<TListItem>().AsEnumerable());
        }, optionsFactory, hideErrors, considerUow, token);
        return list.AsEnumerable();
    }

    public Task<IEnumerable<TListItem>> GetAllAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return GetAllInternalAsync(key, optionsFactory, hideErrors, considerUow, token);
    }

    public Task DeleteAsync(TKey key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return DistributedCache.RemoveAsync(key, hideErrors, considerUow, token);
    }
}
