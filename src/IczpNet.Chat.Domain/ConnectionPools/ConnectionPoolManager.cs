using IczpNet.Chat.Connections;
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

namespace IczpNet.Chat.ConnectionPools;

/// <inheritdoc />
public class ConnectionPoolManager(
    IDistributedCache<ConnectionPoolCacheItem, string> connectionPoolCache,
    IDistributedCache<List<string>> connectionIdsCache,
    IDistributedCache<List<string>, Guid> userConnectionIdsCache,
    IOptions<ConnectionOptions> options
    ) : DomainService, IConnectionPoolManager
{

    /// <summary>
    /// 连接缓存
    /// </summary>
    protected IDistributedCache<List<string>> ConnectionIdsCache { get; } = connectionIdsCache;
    /// <summary>
    /// 用户连接缓存
    /// </summary>
    public IDistributedCache<List<string>, Guid> UserConnectionIdsCache { get; } = userConnectionIdsCache;
    protected IOptions<ConnectionOptions> Options { get; } = options;
    protected ConnectionOptions Config => Options.Value;

    /// <summary>
    /// 连接池缓存Key
    /// </summary>
    protected virtual string ConnectionIdsCacheKey => Config.ConnectionIdsCacheKey;

    /// <summary>
    /// 连接池缓存
    /// </summary>
    public IDistributedCache<ConnectionPoolCacheItem, string> ConnectionPoolCache { get; } = connectionPoolCache;

    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ConnectionCacheExpirationSeconds)
    };

    protected async Task<List<string>> GetConnectionIdsAsync(CancellationToken token = default)
    {
        return (await ConnectionIdsCache.GetAsync(ConnectionIdsCacheKey, token: token)) ?? [];
    }

    protected async Task<List<string>> GetConnectionIdsAsync(string host, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return await GetConnectionIdsAsync(token);
        }

        return [.. (await CreateQueryableAsync(token)).Where(x => x.Host == host).Select(x => x.ConnectionId)];
    }

    protected async Task SetConnectionIdsAsync(List<string> connectionIdList, CancellationToken token = default)
    {
        await ConnectionIdsCache.SetAsync(ConnectionIdsCacheKey, connectionIdList, DistributedCacheEntryOptions, token: token);
    }

    protected virtual Task SetUserContionIdsAsync(Guid userId, List<string> connectionIds, CancellationToken token = default)
    {
        return UserConnectionIdsCache.SetAsync(userId, connectionIds, DistributedCacheEntryOptions, token: token);
    }

    protected virtual async Task<List<string>> AddUserConnetionIdsAsync(Guid userId, string connetionId, CancellationToken token = default)
    {
        var userConnectionIds = (await UserConnectionIdsCache.GetAsync(userId, token: token)) ?? [];

        if (!userConnectionIds.Contains(connetionId))
        {
            userConnectionIds.Add(connetionId);
        }

        await SetUserContionIdsAsync(userId, userConnectionIds, token);

        Logger.LogInformation($"AddUserConnetionIdsAsync:UserId={userId}, connetionId={connetionId}, userConnectionIds.Count={userConnectionIds.Count}");

        return userConnectionIds;
    }

    protected virtual async Task<List<string>> RemoveUserConnetionIdsAsync(Guid userId, string connetionId, CancellationToken token = default)
    {
        var userConnectionIds = (await UserConnectionIdsCache.GetAsync(userId, token: token)) ?? [];

        Logger.LogInformation($"RemoveUserConnetionIdsAsync:UserId={userId}, before remove userConnectionIds.Count={userConnectionIds.Count}");

        userConnectionIds.Remove(connetionId);

        await SetUserContionIdsAsync(userId, userConnectionIds, token);

        Logger.LogInformation($"RemoveUserConnetionIdsAsync:UserId={userId}, connetionId={connetionId}, userConnectionIds.Count={userConnectionIds.Count},userConnectionIds=[{userConnectionIds.JoinAsString(",")}]");

        return userConnectionIds;
    }
    /// <inheritdoc />
    public async Task<bool> AddAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var connectionList = await GetConnectionIdsAsync(token);

        connectionList.Add(connectionPool.ConnectionId);

        await SetConnectionIdsAsync(connectionList, token);

        await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions, token: token);

        if (connectionPool.UserId.HasValue)
        {
            await AddUserConnetionIdsAsync(connectionPool.UserId.Value, connectionPool.ConnectionId, token);
        }

        Logger.LogInformation($"Add connection {connectionPool}");

        Logger.LogInformation($"Online totalCount {connectionList.Count}");

        return true;
    }

    /// <summary>
    /// 更新活动时间 activeTime
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<ConnectionPoolCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
    {
        var connectionPool = await ConnectionPoolCache.GetAsync(connectionId, token: token);
        if (connectionPool != null)
        {
            connectionPool.ActiveTime = Clock.Now;
            await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions, token: token);
            return connectionPool;
        }
        else
        {
            Logger.LogWarning($"{nameof(UpdateActiveTimeAsync)} Fail: Not found, connectionId:{connectionId}");
            await RemoveAsync(connectionId, token);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        return await RemoveAsync(connectionPool.ConnectionId, token: token);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string connectionId, CancellationToken token = default)
    {
        var connectionPool = await ConnectionPoolCache.GetAsync(connectionId, token: token);

        if (connectionPool != null && connectionPool.UserId.HasValue)
        {
            await RemoveUserConnetionIdsAsync(connectionPool.UserId.Value, connectionId, token);
        }

        var connectionIdList = await GetConnectionIdsAsync(token: token);

        Logger.LogInformation($"Online totalCount before delete: {connectionIdList.Count}");

        if (!connectionIdList.Remove(connectionId))
        {
            Logger.LogInformation($"删除失败： {connectionId}");
        }

        await SetConnectionIdsAsync(connectionIdList, token: token);

        await ConnectionPoolCache.RemoveAsync(connectionId, token: token);

        Logger.LogInformation($"Remove connection {connectionId}");

        Logger.LogInformation($"Online totalCount {connectionIdList.Count}");

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
    public async Task<int> GetTotalCountAsync(string host, CancellationToken token = default)
    {
        var list = await GetConnectionIdsAsync(host, token);
        return list.Count;
    }

    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(CancellationToken token = default)
    {
        var list = await GetConnectionIdsAsync(token);
        return list.Count;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPoolCacheItem>> GetAllListAsync(CancellationToken token = default)
    {
        var poolList = new List<ConnectionPoolCacheItem>();

        var list = await GetConnectionIdsAsync(token);

        foreach (var connectionId in list)
        {
            var pool = await ConnectionPoolCache.GetAsync(connectionId, token: token);

            if (pool == null)
            {
                continue;
            }
            poolList.Add(pool);
        }
        return poolList;
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(string host, CancellationToken token = default)
    {
        var list = (await CreateQueryableAsync(token))
            .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
            .ToList();

        foreach (var connectionPool in list)
        {
            await RemoveAsync(connectionPool, token);
        }
        Logger.LogInformation($"ClearAllAsync host:{host}");
    }

    /// <inheritdoc />
    public async Task<IQueryable<ConnectionPoolCacheItem>> CreateQueryableAsync(CancellationToken token = default)
    {
        return (await GetAllListAsync(token)).AsQueryable();
    }

    /// <inheritdoc />
    public async Task<ConnectionPoolCacheItem> GetAsync(string connectionId, CancellationToken token = default)
    {
        return await ConnectionPoolCache.GetAsync(connectionId, token: token);
    }

    /// <inheritdoc />
    public async Task<int> UpdateConnectionIdsAsync(CancellationToken token = default)
    {
        var connectionIdList = (await CreateQueryableAsync(token)).Select(x => x.ConnectionId).ToList();

        await SetConnectionIdsAsync(connectionIdList, token);

        return connectionIdList.Count;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return [.. (await CreateQueryableAsync(token)).Where(x => x.UserId == userId)];
    }

    /// <inheritdoc />
    public async Task<List<string>> GetConnectionIdsByUserIdAsync(Guid userId, CancellationToken token = default)
    {
        return await UserConnectionIdsCache.GetAsync(userId, token: token);
    }

    /// <inheritdoc />
    public async Task<int> UpdateUserConnectionIdsAsync(Guid userId, CancellationToken token = default)
    {
        var userConnectionIds = (await CreateQueryableAsync(token))
            .Where(x => x.UserId == userId)
            .Select(x => x.ConnectionId)
            .ToList();

        await SetUserContionIdsAsync(userId, userConnectionIds, token);

        return userConnectionIds.Count;
    }

    public async Task<List<ConnectionPoolCacheItem>> GetListByChatObjectIdAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        return (await GetAllListAsync(token))
            .Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .ToList();
    }


}
