using IczpNet.Chat.SetLists;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SetLists
{
    /// <summary>
    /// 基于 IDistributedCache《IEnumerable《TListItem》, TKey》 的实现（非 Redis 原子），
    /// 用于在不直接操作 Redis 时复用已有分布式缓存实现。
    /// </summary>
    public class DistributedCacheListSet<TListItem, TKey> : DomainService, IDistributedCacheListSet<TListItem, TKey>
        where TListItem : class
    {
        public IDistributedCache<IEnumerable<TListItem>, TKey> DistributedCache { get; }

        public DistributedCacheListSet(IDistributedCache<IEnumerable<TListItem>, TKey> distributedCache)
        {
            DistributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public async Task<long> AddAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (items == null) return 0;

            // Get or empty
            var current = await DistributedCache.GetAsync(key, hideErrors, considerUow, token).ConfigureAwait(false) ?? Enumerable.Empty<TListItem>();
            var toAdd = items.Except(current).ToArray();
            if (toAdd.Length == 0) return 0;

            var newList = current.Concat(toAdd).Distinct().ToArray();
            await DistributedCache.SetAsync(key, newList, optionsFactory?.Invoke(), hideErrors, considerUow, token).ConfigureAwait(false);
            return toAdd.Length;
        }

        public async Task<KeyValuePair<TKey, long>[]> AddManyAsync(IEnumerable<KeyValuePair<TKey, TListItem>> keyValues, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (keyValues == null) return Array.Empty<KeyValuePair<TKey, long>>();

            // group preserving first seen order
            var groups = keyValues
                .GroupBy(kv => kv.Key)
                .Select(g => new { Key = g.Key, Items = g.Select(kv => kv.Value).ToArray() })
                .ToArray();

            var results = new KeyValuePair<TKey, long>[groups.Length];

            for (int i = 0; i < groups.Length; i++)
            {
                var key = groups[i].Key;
                var items = groups[i].Items;
                var added = await AddAsync(key, items, optionsFactory, hideErrors, considerUow, token).ConfigureAwait(false);
                results[i] = new KeyValuePair<TKey, long>(key, added);
            }

            return results;
        }

        public async Task<long> RemoveAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (items == null) return 0;

            var current = await DistributedCache.GetAsync(key, hideErrors, considerUow, token).ConfigureAwait(false) ?? Enumerable.Empty<TListItem>();
            var toRemove = items.Intersect(current).ToArray();
            if (toRemove.Length == 0) return 0;

            var newList = current.Except(toRemove).Distinct().ToArray();
            await DistributedCache.SetAsync(key, newList, optionsFactory?.Invoke(), hideErrors, considerUow, token).ConfigureAwait(false);
            return toRemove.Length;
        }

        public async Task<KeyValuePair<TKey, long>[]> RemoveManyAsync(IEnumerable<KeyValuePair<TKey, TListItem>> keyValues, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (keyValues == null) return Array.Empty<KeyValuePair<TKey, long>>();

            var groups = keyValues
                .GroupBy(kv => kv.Key)
                .Select(g => new { Key = g.Key, Items = g.Select(kv => kv.Value).ToArray() })
                .ToArray();

            var results = new KeyValuePair<TKey, long>[groups.Length];
            for (int i = 0; i < groups.Length; i++)
            {
                var key = groups[i].Key;
                var items = groups[i].Items;
                var removed = await RemoveAsync(key, items, optionsFactory, hideErrors, considerUow, token).ConfigureAwait(false);
                results[i] = new KeyValuePair<TKey, long>(key, removed);
            }
            return results;
        }

        public async Task<IEnumerable<TListItem>> GetAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var list = await DistributedCache.GetAsync(key, hideErrors, considerUow, token).ConfigureAwait(false);
            return list ?? Enumerable.Empty<TListItem>();
        }

        public async Task<KeyValuePair<TKey, IEnumerable<TListItem>>[]> GetManyAsync(IEnumerable<TKey> keys, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (keys == null) return Array.Empty<KeyValuePair<TKey, IEnumerable<TListItem>>>();

            // Use DistributedCache.GetManyAsync if available to batch fetch; otherwise fetch individually.
            try
            {
                var keyArray = keys.ToArray();
                var fetched = await DistributedCache.GetManyAsync(keyArray, hideErrors, considerUow, token).ConfigureAwait(false);
                // fetched is KeyValuePair<TKey, TCacheItem?>[] (per Volo.Abp IDistributedCache contract)
                var map = fetched.ToDictionary(kv => kv.Key, kv => kv.Value ?? Enumerable.Empty<TListItem>());
                var results = keyArray.Select(k => new KeyValuePair<TKey, IEnumerable<TListItem>>(k, map.ContainsKey(k) ? map[k] : Enumerable.Empty<TListItem>())).ToArray();
                return results;
            }
            catch
            {
                // Fallback: individual gets (preserve order)
                var keyList = keys.ToArray();
                var results = new KeyValuePair<TKey, IEnumerable<TListItem>>[keyList.Length];
                for (int i = 0; i < keyList.Length; i++)
                {
                    var k = keyList[i];
                    var v = await GetAsync(k, optionsFactory, hideErrors, considerUow, token).ConfigureAwait(false);
                    results[i] = new KeyValuePair<TKey, IEnumerable<TListItem>>(k, v);
                }
                return results;
            }
        }

        public async Task<bool> RefreshAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (optionsFactory == null) return false;

            var options = optionsFactory();
            if (options == null) return false;

            try
            {
                await DistributedCache.SetAsync(key, await DistributedCache.GetAsync(key, hideErrors, considerUow, token).ConfigureAwait(false) ?? Enumerable.Empty<TListItem>(), options, hideErrors, considerUow, token).ConfigureAwait(false);
                return true;
            }
            catch
            {
                if (hideErrors == true) return false;
                throw;
            }
        }

        public async Task<KeyValuePair<TKey, bool>[]> RefreshManyAsync(IEnumerable<TKey> keys, Func<DistributedCacheEntryOptions> optionsFactory, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (keys == null) return Array.Empty<KeyValuePair<TKey, bool>>();
            if (optionsFactory == null) return keys.Select(k => new KeyValuePair<TKey, bool>(k, false)).ToArray();

            var options = optionsFactory();
            if (options == null) return keys.Select(k => new KeyValuePair<TKey, bool>(k, false)).ToArray();

            var keyList = keys.ToArray();
            var results = new KeyValuePair<TKey, bool>[keyList.Length];
            for (int i = 0; i < keyList.Length; i++)
            {
                var k = keyList[i];
                try
                {
                    var value = await DistributedCache.GetAsync(k, hideErrors, considerUow, token).ConfigureAwait(false) ?? Enumerable.Empty<TListItem>();
                    await DistributedCache.SetAsync(k, value, options, hideErrors, considerUow, token).ConfigureAwait(false);
                    results[i] = new KeyValuePair<TKey, bool>(k, true);
                }
                catch
                {
                    if (hideErrors == true) results[i] = new KeyValuePair<TKey, bool>(k, false);
                    else throw;
                }
            }
            return results;
        }

        public async Task<IEnumerable<TListItem>> ReplaceAsync(TKey key, IEnumerable<TListItem> items, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var list = (items ?? Enumerable.Empty<TListItem>()).Distinct().ToArray();
            await DistributedCache.SetAsync(key, list, optionsFactory?.Invoke(), hideErrors, considerUow, token).ConfigureAwait(false);
            return list;
        }

        public Task DeleteAsync(TKey key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return DistributedCache.RemoveAsync(key, hideErrors, considerUow, token);
        }

        public Task<IQueryable<TListItem>> CreateQueryableAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return CreateQueryableInternalAsync(key, optionsFactory, hideErrors, considerUow, token);
        }

        internal async Task<IQueryable<TListItem>> CreateQueryableInternalAsync(TKey key, Func<DistributedCacheEntryOptions> optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            var list = await DistributedCache.GetOrAddAsync(key, () => Task.FromResult<IEnumerable<TListItem>>(Enumerable.Empty<TListItem>()), optionsFactory, hideErrors, considerUow, token).ConfigureAwait(false);
            return (list ?? Enumerable.Empty<TListItem>()).AsQueryable();
        }

        public Task DeleteManyAsync(IEnumerable<TKey> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return DistributedCache.RemoveManyAsync(keys, hideErrors, considerUow, token);
        }
    }
}