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
        var keyValues = new List<KeyValuePair<IndexCacheKey, string>>();

        var valueTypes = new List<IndexCacheValueType>() { IndexCacheValueType.ConnectionId, IndexCacheValueType.DeviceType };

        foreach (var item in valueTypes)
        {
            // ChatObjectId 索引
            var value = item == IndexCacheValueType.ConnectionId ? connectionPool.ConnectionId : connectionPool.DeviceType;

            var key1 = connectionPool.ChatObjectIdList.Select(chatObjectId => new KeyValuePair<IndexCacheKey, string>(new IndexCacheKey(chatObjectId: chatObjectId, item), value));

            keyValues = keyValues.Concat(key1).ToList();

            // UserId 索引
            if (connectionPool.UserId.HasValue)
            {
                keyValues = keyValues.Append(new KeyValuePair<IndexCacheKey, string>(new IndexCacheKey(userId: connectionPool.UserId.Value, item), value)).ToList();
            }
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
    public virtual async Task<bool> ConnectedAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
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
    public virtual async Task<ConnectionPoolCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
    {
        //// 刷新连接ID列表缓存
        //await AllConnectIdListSetCache.RefreshAsync(ConnectionIdListSetCacheKey, () => DistributedCacheEntryOptions, token: token);

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

    /// <inheritdoc />
    public virtual async Task<bool> DisconnectedAsync(string connectionId, CancellationToken token = default)
    {
        var removedCount = await AllConnectIdListSetCache.RemoveAsync(ConnectionIdListSetCacheKey, [connectionId], token: token);

        Logger.LogInformation($"DisconnectedAsync Remove connection from {nameof(AllConnectIdListSetCache)} removedCount={removedCount}");

        var connectionPool = await ConnectionPoolCache.GetAsync(connectionId, token: token);

        if (connectionPool != null)
        {
            await RemoveIndexAsync(connectionPool, token);
        }

        await ConnectionPoolCache.RemoveAsync(connectionId, token: token);

        Logger.LogInformation($"DisconnectedAsync Remove connection {connectionId}");

        return true;
    }


    /// <inheritdoc />
    public virtual async Task<int> GetTotalCountAsync(string host = null, CancellationToken token = default)
    {
        return (await CreateQueryableAsync(token))
             .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
             .Count();
    }

    /// <inheritdoc />
    public virtual async Task ClearAllAsync(string host, string reason, CancellationToken token = default)
    {
        Logger.LogInformation($"ClearAllAsync[{reason}] Start:{host},time:{Clock.Now}");

        var connectionIdListByHost = (await CreateQueryableAsync(token))
            .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
            .ToList();

        var keys = connectionIdListByHost.Select(x => GetKeyValues(x, token).Select(d => d.Key)).SelectMany(x => x).Distinct().ToList();

        await IndexListSetCache.DeleteManyAsync(keys, token: token);

        Logger.LogInformation($"ClearAllAsync[{reason}] [{host}] {nameof(IndexListSetCache)} delete keys[{keys.Count}]:{keys.JoinAsString(",")}");

        await AllConnectIdListSetCache.DeleteAsync(ConnectionIdListSetCacheKey, token: token);

        Logger.LogInformation($"ClearAllAsync[{reason}] [{host}] {nameof(AllConnectIdListSetCache)} delete key:{ConnectionIdListSetCacheKey}");

        var connIdList = connectionIdListByHost.Select(x => x.ConnectionId).ToList();

        await ConnectionPoolCache.RemoveManyAsync(connIdList, token: token);

        Logger.LogInformation($"ClearAllAsync[{reason}] [{host}] {nameof(ConnectionPoolCache)} delete  keys[{connIdList.Count}]:{connIdList.JoinAsString(",")}");

        Logger.LogInformation($"ClearAllAsync[{reason}] End:{host},time:{Clock.Now}");
    }

    /// <inheritdoc />
    public virtual async Task<IQueryable<ConnectionPoolCacheItem>> CreateQueryableAsync(CancellationToken token = default)
    {
        var connectionIdList = await AllConnectIdListSetCache.CreateQueryableAsync(ConnectionIdListSetCacheKey, token: token);

        var list = await ConnectionPoolCache.GetManyAsync(connectionIdList, token: token);

        return list.Where(x => x.Value != null).Select(x => x.Value).AsQueryable();
    }

    /// <inheritdoc />
    public virtual async Task<ConnectionPoolCacheItem> GetAsync(string connectionId, CancellationToken token = default)
    {
        return await ConnectionPoolCache.GetAsync(connectionId, token: token);
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAllConnectionIdsAsync(CancellationToken token = default)
    {
        var connectionIdList = (await CreateQueryableAsync(token)).Select(x => x.ConnectionId).ToList();

        await AllConnectIdListSetCache.ReplaceAsync(ConnectionIdListSetCacheKey, connectionIdList, () => DistributedCacheEntryOptions, token: token);

        return connectionIdList.Count;
    }

    /// <inheritdoc />
    public virtual async Task<List<string>> GetConnectionIdsByUserAsync(Guid userId, CancellationToken token = default)
    {
        return (await IndexListSetCache.CreateQueryableAsync(new IndexCacheKey(userId: userId, IndexCacheValueType.ConnectionId), token: token)).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateIndexByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await UpdateIndexInternalAsync(nameof(IndexCacheKey.UserId), userId, token: token);
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateIndexByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        return await UpdateIndexInternalAsync(nameof(IndexCacheKey.ChatObjectId), chatObjectId, token: token);
    }

    /// <summary>
    /// nameof(IndexCacheKey.ChatObjectId) | nameof(IndexCacheKey.UserId)
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"> ChatObjectId | UserId</param>
    /// <param name="token"></param>
    /// <returns></returns>
    protected virtual async Task<int> UpdateIndexInternalAsync(string propertyName, dynamic value, CancellationToken token = default)
    {
        // connectionId
        var connKey = new IndexCacheKey(propertyName, value, IndexCacheValueType.ConnectionId);

        var connIdList = await IndexListSetCache.GetAsync(connKey, token: token);

        var connList = await ConnectionPoolCache.GetManyAsync(connIdList, token: token);

        var query = connList
            .Where(x => x.Value != null)
            .Where(x => !x.Value.ActiveTime.HasValue || x.Value.ActiveTime >= Clock.Now.AddSeconds(-Config.InactiveSeconds))
            ;

        var newConnIdList = query.Select(x => x.Value.ConnectionId).Distinct().ToList();

        await IndexListSetCache.ReplaceAsync(connKey, newConnIdList, () => DistributedCacheEntryOptions, token: token);

        // deviceType
        var deviceTypeKey = new IndexCacheKey(propertyName, value, IndexCacheValueType.DeviceType);

        var newDeviceTypeList = query.Select(x => x.Value.DeviceType).Distinct().ToList();

        await IndexListSetCache.ReplaceAsync(deviceTypeKey, newDeviceTypeList, () => DistributedCacheEntryOptions, token: token);

        return newConnIdList.Count;
    }


    protected virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByIndexInternalAsync(string propertyName, dynamic value, CancellationToken token = default)
    {
        var key = new IndexCacheKey(propertyName, value, IndexCacheValueType.ConnectionId);

        var connIdList = await IndexListSetCache.GetAsync(key, token: token);

        var res = await ConnectionPoolCache.GetManyAsync(connIdList, token: token);

        return res.Select(x => x.Value);
    }


    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        return await GetListByIndexInternalAsync(nameof(IndexCacheKey.ChatObjectId), chatObjectId, token);
    }

    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await GetListByIndexInternalAsync(nameof(IndexCacheKey.UserId), userId, token);
    }

    public virtual async Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default)
    {
        var connIdList = await IndexListSetCache.GetAsync(new IndexCacheKey(userId: userId, IndexCacheValueType.ConnectionId), token: token);

        return connIdList != null && connIdList.Any();
    }

    public virtual async Task<bool> IsOnlineAsync(long chatObjectId, CancellationToken token = default)
    {
        var connIdList = await IndexListSetCache.GetAsync(new IndexCacheKey(chatObjectId: chatObjectId, IndexCacheValueType.ConnectionId), token: token);

        return connIdList != null && connIdList.Any();
    }

    public virtual async Task<List<string>> GetDeviceTypesAsync(long chatObjectId, CancellationToken token = default)
    {
        return [.. await IndexListSetCache.GetAsync(new IndexCacheKey(chatObjectId: chatObjectId, IndexCacheValueType.DeviceType), token: token)];
    }

    public virtual async Task<List<string>> GetDeviceTypesAsync(Guid userId, CancellationToken token = default)
    {
        return [.. (await IndexListSetCache.GetAsync(new IndexCacheKey(userId: userId, IndexCacheValueType.DeviceType), token: token))];
    }

    public async Task<int> GetCountByUserAsync(Guid userId, CancellationToken token = default)
    {
        return (await GetDeviceTypesAsync(userId, token)).Count;
    }

    public async Task<int> GetCountByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        return (await GetDeviceTypesAsync(chatObjectId, token)).Count;
    }
}
