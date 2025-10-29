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

    public async Task<long> AddAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = await CreateQueryableInternalAsync(key, optionsFactory, hideErrors, considerUow, token);

        var addedCount = items.Except(list).Count();

        list = list.Union(items).Distinct();

        await DistributedCache.SetAsync(key, list, optionsFactory?.Invoke(), hideErrors, considerUow, token);

        return addedCount;
    }

    public async Task<long> RemoveAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = await CreateQueryableInternalAsync(key, optionsFactory, hideErrors, considerUow, token);

        list = list.Except(items).Distinct();

        await DistributedCache.SetAsync(key, list, optionsFactory?.Invoke(), hideErrors, considerUow, token);

        return items.Count();
    }

    internal async Task<IQueryable<TListItem>> CreateQueryableInternalAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = await DistributedCache.GetOrAddAsync(key, () =>
        {
            return Task.FromResult(new List<TListItem>().AsEnumerable());
        }, optionsFactory, hideErrors, considerUow, token);
        return list.AsQueryable();
    }

    public Task<IQueryable<TListItem>> CreateQueryableAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return CreateQueryableInternalAsync(key, optionsFactory, hideErrors, considerUow, token);
    }

    public Task DeleteAsync(TKey key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return DistributedCache.RemoveAsync(key, hideErrors, considerUow, token);
    }

    public async Task<IEnumerable<TListItem>> ReplaceAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        var list = items.Distinct();

        await DistributedCache.SetAsync(key, list, optionsFactory?.Invoke(), hideErrors, considerUow, token);

        return list.AsEnumerable();
    }

    public Task<KeyValuePair<TKey, long>[]> AddManyAsync(IEnumerable<KeyValuePair<TKey, TListItem>> keyValues, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<KeyValuePair<TKey, long>[]> RemoveManyAsync(IEnumerable<KeyValuePair<TKey, TListItem>> keyValues, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TListItem>> GetAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<KeyValuePair<TKey, IEnumerable<TListItem>>[]> GetManyAsync(IEnumerable<TKey> keys, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RefreshAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<KeyValuePair<TKey, bool>[]> RefreshManyAsync(IEnumerable<TKey> keys, Func<DistributedCacheEntryOptions> optionsFactory, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
