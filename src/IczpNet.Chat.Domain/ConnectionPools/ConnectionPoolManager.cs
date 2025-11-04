using IczpNet.Chat.Connections;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

/// <inheritdoc />
public class ConnectionPoolManager(
    IOptions<ConnectionOptions> options
    ) : ConnectionPoolManagerBase<ConnectionPoolCacheItem, IndexCacheKey>, IConnectionPoolManager
{

    protected ConnectionOptions Config => options.Value;

    protected override string ConnectionIdListSetCacheKey => Config.AllConnectionsCacheKey;

    protected override DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ConnectionCacheExpirationSeconds)
    };

    protected override IEnumerable<KeyValuePair<IndexCacheKey, string>> GetKeyValues(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
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


    protected virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListBySingleIndexInternalAsync(string propertyName, dynamic value, CancellationToken token = default)
    {
        var key = new IndexCacheKey(propertyName, value, IndexCacheValueType.ConnectionId);

        var connIdList = await IndexListSetCache.GetAsync(key, token: token);

        var res = await ConnectionPoolCache.GetManyAsync(connIdList, token: token);

        return res.Select(x => x.Value);
    }
    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByManyIndexInternalAsync(string propertyName, IEnumerable<dynamic> values, CancellationToken token = default)
    {
        var keyValues = values.Select(x => new IndexCacheKey(propertyName, x, IndexCacheValueType.ConnectionId));

        var connIdList = (await IndexListSetCache.GetManyAsync(keyValues, token: token)).SelectMany(x => x.Value);

        var res = await ConnectionPoolCache.GetManyAsync(connIdList, token: token);

        return res.Select(x => x.Value);
    }


    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        return await GetListBySingleIndexInternalAsync(nameof(IndexCacheKey.ChatObjectId), chatObjectId, token);
    }

    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByChatObjectAsync(IEnumerable<long> chatObjectIdList, CancellationToken token = default)
    {
        return await GetListBySingleIndexInternalAsync(nameof(IndexCacheKey.ChatObjectId), chatObjectIdList, token);
    }

    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await GetListBySingleIndexInternalAsync(nameof(IndexCacheKey.UserId), userId, token);
    }

    public virtual async Task<IEnumerable<ConnectionPoolCacheItem>> GetListByUserAsync(IEnumerable<Guid> userIdList, CancellationToken token = default)
    {
        return await GetListBySingleIndexInternalAsync(nameof(IndexCacheKey.UserId), userIdList, token);
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

    public virtual async Task<int> GetCountByUserAsync(Guid userId, CancellationToken token = default)
    {
        return (await GetDeviceTypesAsync(userId, token)).Count;
    }

    public virtual async Task<int> GetCountByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        return (await GetDeviceTypesAsync(chatObjectId, token)).Count;
    }
}
