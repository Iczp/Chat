using IczpNet.Chat.Connections;
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

namespace IczpNet.Chat.ConnectionPools;

/// <inheritdoc />
public class ConnectionPoolManager(
    IDistributedCache<ConnectionPoolCacheItem, string> connectionPoolCache,
    IDistributedCacheListSet<string, IndexCacheKey> indexListSetCache,
    IDistributedCacheListSet<string, string> allConnectIdListSetCache,
    IOptions<ConnectionOptions> options
    ) : DomainService, IConnectionPoolManager
{

    /// <summary>
    /// 连接缓存
    /// </summary>
    public IDistributedCacheListSet<string, string> AllConnectIdListSetCache { get; } = allConnectIdListSetCache;


    protected IOptions<ConnectionOptions> Options { get; } = options;
    protected ConnectionOptions Config => Options.Value;

    /// <summary>
    /// 连接池缓存Key
    /// </summary>
    protected virtual string ConnectionIdListSetCacheKey => Config.ConnectionIdsCacheKey + ":DistributedCacheListSet";

    /// <summary>
    /// 连接池缓存
    /// </summary>
    public IDistributedCache<ConnectionPoolCacheItem, string> ConnectionPoolCache { get; } = connectionPoolCache;

    /// <summary>
    /// 索引缓存
    /// </summary>
    public IDistributedCacheListSet<string, IndexCacheKey> IndexListSetCache { get; } = indexListSetCache;

    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ConnectionCacheExpirationSeconds)
    };

    protected virtual IEnumerable<KeyValuePair<IndexCacheKey, string>> GetKeyValues(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        // ChatObjectId 索引
        var keyValues = connectionPool.ChatObjectIdList.Select(chatObjectId => new KeyValuePair<IndexCacheKey, string>(new IndexCacheKey(chatObjectId: chatObjectId), connectionPool.ConnectionId));

        if (connectionPool.UserId.HasValue)
        {
            // UserId 索引
            keyValues = keyValues.Append(new KeyValuePair<IndexCacheKey, string>(new IndexCacheKey(userId: connectionPool.UserId.Value), connectionPool.ConnectionId));
        }
        return keyValues;
    }
    protected virtual async Task<KeyValuePair<IndexCacheKey, long>[]> AddIndexAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var keyValues = GetKeyValues(connectionPool, token);

        var addedResult = await IndexListSetCache.AddManyAsync(keyValues, () => DistributedCacheEntryOptions, token: token);

        return addedResult;
    }

    protected virtual async Task<KeyValuePair<IndexCacheKey, long>[]> RemoveIndexAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var keyValues = GetKeyValues(connectionPool, token);

        var removed = await IndexListSetCache.RemoveManyAsync(keyValues, () => DistributedCacheEntryOptions, token: token);

        return removed;
    }

    protected virtual async Task DeleteIndexAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var keys = GetKeyValues(connectionPool, token).Select(x => x.Key);
        await IndexListSetCache.DeleteManyAsync(keys, token: token);
    }


    protected virtual async Task<List<string>> GetConnectionIdListAsync(CancellationToken token = default)
    {
        return (await AllConnectIdListSetCache.CreateQueryableAsync(ConnectionIdListSetCacheKey, token: token)).ToList();
    }

    /// <inheritdoc />
    public async Task<bool> ConnectedAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var addedCount = await AllConnectIdListSetCache.AddAsync(ConnectionIdListSetCacheKey, [connectionPool.ConnectionId], token: token);

        Logger.LogInformation($"Add connection from DistributedCacheListSet addedCount: {addedCount}");

        await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions, token: token);

        await AddIndexAsync(connectionPool, token);

        Logger.LogInformation($"Add connection {connectionPool}");

        return addedCount > 0;
    }

    /// <summary>
    /// 更新活动时间 activeTime
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<ConnectionPoolCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
    {
        // 刷新连接ID列表缓存
        await AllConnectIdListSetCache.RefreshAsync(ConnectionIdListSetCacheKey, () => DistributedCacheEntryOptions, token: token);

        var connectionPool = await ConnectionPoolCache.GetAsync(connectionId, token: token);

        if (connectionPool != null)
        {
            // 刷新索引缓存
            var indexKeys = GetKeyValues(connectionPool, token).Select(x => x.Key);

            await IndexListSetCache.RefreshManyAsync(indexKeys, () => DistributedCacheEntryOptions, token: token);

            // 更新活动时间
            connectionPool.ActiveTime = Clock.Now;

            await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions, token: token);

            return connectionPool;
        }
        else
        {
            Logger.LogWarning($"{nameof(UpdateActiveTimeAsync)} Fail: Not found, item:{connectionId}");

            await DisconnectedAsync(connectionId, token);

            return null;
        }
    }

    protected virtual async Task<bool> DisconnectedInternalAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        return await DisconnectedAsync(connectionPool.ConnectionId, token: token);
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectedAsync(string connectionId, CancellationToken token = default)
    {
        var removedCount = await AllConnectIdListSetCache.RemoveAsync(ConnectionIdListSetCacheKey, [connectionId], token: token);

        Logger.LogInformation($"Remove connection from DistributedCacheListSet removedCount={removedCount}");

        var connectionPool = await ConnectionPoolCache.GetAsync(connectionId, token: token);

        if (connectionPool != null)
        {
            await RemoveIndexAsync(connectionPool, token);
        }

        await ConnectionPoolCache.RemoveAsync(connectionId, token: token);

        Logger.LogInformation($"Remove connection {connectionId}");

        return true;
    }


    /// <inheritdoc />
    public async Task<int> GetTotalCountAsync(string host = null, CancellationToken token = default)
    {
        return (await CreateQueryableAsync(token))
             .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
             .Count();
    }

    /// <inheritdoc />
    public async Task<IQueryable<ConnectionPoolCacheItem>> GetAllListAsync(CancellationToken token = default)
    {
        return await CreateQueryableAsync(token);
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(string host, CancellationToken token = default)
    {
        var connectionIdListByHost = (await CreateQueryableAsync(token))
            .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
            .ToList();

        //foreach (var connectionPool in connectionIdListByHost)
        //{
        //    if (!string.IsNullOrWhiteSpace(host))
        //    {
        //        await DisconnectedInternalAsync(connectionPool, token);
        //    }
        //    else
        //    {
        //        await DeleteIndexAsync(connectionPool, token);
        //    }
        //}

        var keys = connectionIdListByHost.Select(x => GetKeyValues(x, token).Select(d => d.Key)).SelectMany(x => x).Distinct().ToList();

        await IndexListSetCache.DeleteManyAsync(keys, token: token);

        Logger.LogInformation($"ClearAllAsync host:{host}");

        await AllConnectIdListSetCache.DeleteAsync(ConnectionIdListSetCacheKey, token: token);
    }

    /// <inheritdoc />
    public async Task<IQueryable<ConnectionPoolCacheItem>> CreateQueryableAsync(CancellationToken token = default)
    {
        var connectionIdList = await AllConnectIdListSetCache.CreateQueryableAsync(ConnectionIdListSetCacheKey, token: token);

        var list = await ConnectionPoolCache.GetManyAsync(connectionIdList, token: token);

        return list.Where(x => x.Value != null).Select(x => x.Value).AsQueryable();
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

        await AllConnectIdListSetCache.ReplaceAsync(ConnectionIdListSetCacheKey, connectionIdList, () => DistributedCacheEntryOptions, token: token);

        return connectionIdList.Count;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserAsync(Guid userId, CancellationToken token = default)
    {
        var userConnectionIds = await IndexListSetCache.GetAsync(new IndexCacheKey(userId: userId), token: token);

        if (userConnectionIds == null || !userConnectionIds.Any())
        {
            return [];
        }
        var list = await ConnectionPoolCache.GetManyAsync(userConnectionIds, token: token);

        return list.Select(x => x.Value);
    }

    /// <inheritdoc />
    public async Task<List<string>> GetConnectionIdsByUserAsync(Guid userId, CancellationToken token = default)
    {
        return (await IndexListSetCache.CreateQueryableAsync(new IndexCacheKey(userId: userId), token: token)).ToList();
    }

    /// <inheritdoc />
    public async Task<int> UpdateUserConnectionIdsAsync(Guid userId, CancellationToken token = default)
    {
        return await UpdateIndexConnectionIdsAsync(new IndexCacheKey(userId: userId), token: token);
    }

    /// <inheritdoc />
    public async Task<int> UpdateChatObjectConnectionIdsAsync(long chatObjectId, CancellationToken token = default)
    {
        return await UpdateIndexConnectionIdsAsync(new IndexCacheKey(chatObjectId: chatObjectId), token: token);
    }

    protected virtual async Task<int> UpdateIndexConnectionIdsAsync(IndexCacheKey indexCacheKey, CancellationToken token = default)
    {
        //var userConnectionIds = (await CreateQueryableAsync(token))
        //    .Where(x => x.UserId == userId)
        //    .Select(x => x.ConnectionId)
        //    .ToList();

        //await IndexListSetCache.ReplaceAsync(new IndexCacheKey(userId: userId), userConnectionIds, () => DistributedCacheEntryOptions, token: token);

        //return userConnectionIds.Count;

        var connIdList = await IndexListSetCache.GetAsync(indexCacheKey, token: token);

        var connList = await ConnectionPoolCache.GetManyAsync(connIdList, token: token);

        var newConnIdList = connList
            .Where(x => x.Value != null)
            .Where(x => x.Value.ActiveTime > Clock.Now.AddSeconds(-Config.TimerPeriodSeconds))
            .Select(x => x.Value.ConnectionId)
            .ToList();

        await IndexListSetCache.ReplaceAsync(indexCacheKey, newConnIdList, () => DistributedCacheEntryOptions, token: token);

        return newConnIdList.Count;
    }

    public async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        var key = new IndexCacheKey(chatObjectId: chatObjectId);

        var connIdList = await IndexListSetCache.GetAsync(key, token: token);

        var res = await ConnectionPoolCache.GetManyAsync(connIdList, token: token);

        return res.Select(x => x.Value);
    }

    public async Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default)
    {
        var connIdList = await IndexListSetCache.GetAsync(new IndexCacheKey(userId: userId), token: token);

        return connIdList != null && connIdList.Any();
    }

    public async Task<bool> IsOnlineAsync(long chatObjectId, CancellationToken token = default)
    {
        var connIdList = await IndexListSetCache.GetAsync(new IndexCacheKey(chatObjectId: chatObjectId), token: token);

        return connIdList != null && connIdList.Any();
    }


}
