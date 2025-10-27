using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SetLists;

public interface IDistributedCacheListSet<TListItem, TKey>
{
    Task<IEnumerable<TListItem>> AddAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task<IEnumerable<TListItem>>  RemoveAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task DeleteAsync(TKey key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

    Task<IEnumerable<TListItem>> GetAllAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

}
