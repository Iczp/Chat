using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ConnectionPools;

/// <inheritdoc />
public class ConnectionPoolManager(
    IDistributedCache<ConnectionPoolCacheItem, string> connectionPoolCache,
    IDistributedCache<List<string>> connectionIdsCache
    ) : DomainService, IConnectionPoolManager
{

    /// <summary>
    /// 连接缓存
    /// </summary>
    public IDistributedCache<List<string>> ConnectionIdsCache { get; } = connectionIdsCache;
    /// <summary>
    /// 连接池缓存
    /// </summary>
    protected virtual string ConnectionIdsCacheKey => "ConnectionIds_v0.1";
    /// <summary>
    /// 连接池缓存
    /// </summary>
    public IDistributedCache<ConnectionPoolCacheItem, string> ConnectionPoolCache { get; } = connectionPoolCache;

    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions { get; } = new DistributedCacheEntryOptions()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
    };

    /// <inheritdoc />
    public Task ActiveAsync(ConnectionPoolCacheItem connectionPool, string message)
    {
        throw new System.NotImplementedException();
    }

    protected async Task<List<string>> GetConnectionIdsAsync(CancellationToken token = default)
    {
        return (await ConnectionIdsCache.GetAsync(ConnectionIdsCacheKey, token: token)) ?? [];
    }

    protected async Task SetConnectionIdsAsync(List<string> connectionIdList, CancellationToken token = default)
    {
        await ConnectionIdsCache.SetAsync(ConnectionIdsCacheKey, connectionIdList, DistributedCacheEntryOptions, token: token);
    }

    /// <inheritdoc />
    public async Task<bool> AddAsync(ConnectionPoolCacheItem connectionPool)
    {
        var connectionList = await GetConnectionIdsAsync();

        connectionList.Add(connectionPool.ConnectionId);

        await SetConnectionIdsAsync(connectionList);

        await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions);

        Logger.LogInformation($"Add connection {connectionPool}");

        Logger.LogInformation($"Online totalCount {connectionList.Count}");

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        return await RemoveAsync(connectionPool.ConnectionId, token: token);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string connectionId, CancellationToken token = default)
    {
        var connectionList = await GetConnectionIdsAsync(token: token);

        Logger.LogInformation($"Online totalCount before delete: {connectionList.Count}");

        if (!connectionList.Remove(connectionId))
        {
            Logger.LogInformation($"删除失败： {connectionId}");
        }

        await SetConnectionIdsAsync(connectionList, token: token);

        await ConnectionPoolCache.RemoveAsync(connectionId, token: token);

        Logger.LogInformation($"Remove connection {connectionId}");

        Logger.LogInformation($"Online totalCount {connectionList.Count}");

        return true;
    }
    /// <inheritdoc />
    public void Remove(string connectionId)
    {
        //var token = new System.Threading.CancellationToken();

        var connectionIdList = ConnectionIdsCache.Get(ConnectionIdsCacheKey);

        Logger.LogInformation($"Online totalCount before delete: {connectionIdList.Count}");

        if (!connectionIdList.Remove(connectionId))
        {
            Logger.LogInformation($"删除失败： {connectionId}");
        }

        ConnectionIdsCache.Set(ConnectionIdsCacheKey, connectionIdList, DistributedCacheEntryOptions);

        ConnectionPoolCache.Remove(connectionId);

        Logger.LogInformation($"Remove connection {connectionId}");

        Logger.LogInformation($"Online totalCount {connectionIdList.Count}");
    }

    /// <inheritdoc />
    public async Task<int> TotalCountAsync(string host)
    {
        var list = await GetConnectionIdsAsync();
        return list.Count;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPoolCacheItem>> GetAllListAsync()
    {
        var poolList = new List<ConnectionPoolCacheItem>();

        var list = await GetConnectionIdsAsync();

        foreach (var connectionId in list)
        {
            var pool = await ConnectionPoolCache.GetAsync(connectionId);

            if (pool == null)
            {
                continue;
            }
            poolList.Add(pool);
        }
        return poolList;
    }

    /// <inheritdoc />
    public Task<List<ConnectionPoolCacheItem>> GetListAsync(string connectionId)
    {
        throw new System.NotImplementedException();
    }



    /// <inheritdoc />
    public Task<bool> SendMessageAsync(ConnectionPoolCacheItem connectionPool, string message)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public Task SendToAllAsync(string message)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(string host)
    {
        var list = (await GetAllListAsync()).AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
            .ToList();

        foreach (var connectionPool in list)
        {
            await RemoveAsync(connectionPool.ConnectionId);
        }
    }
}
