using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SetLists;

public interface IDistributedCacheListSet<TListItem, TKey>
{
    Task<long> AddAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task<long>  RemoveAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task<IEnumerable<TListItem>> ReplaceAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task DeleteAsync(TKey key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task<IQueryable<TListItem>> CreateQueryableAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

}
