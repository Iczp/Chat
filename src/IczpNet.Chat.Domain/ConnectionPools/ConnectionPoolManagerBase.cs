using IczpNet.Chat.Hosting;
using IczpNet.Chat.ListSets;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
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
public abstract class ConnectionPoolManagerBase<TCacheItem, TIndexCacheKey>() : DomainService, IConnectionPoolManagerBase<TCacheItem>, IHostedService
    where TCacheItem : class, IConnectionPool, new()
    where TIndexCacheKey : class
{

    /// <summary>
    /// 连接池缓存Key
    /// </summary>
    protected abstract string ConnectionIdListSetCacheKey { get; }

    protected abstract DistributedCacheEntryOptions DistributedCacheEntryOptions { get; }

    /// <summary>
    /// 连接缓存
    /// </summary>
    public IDistributedCacheListSet<string, string> AllConnectIdListSetCache { get; set; }

    /// <summary>
    /// 连接池缓存
    /// </summary>
    public IDistributedCache<TCacheItem, string> ConnectionPoolCache { get; set; }

    /// <summary>
    /// 设备索引
    /// </summary>
    public IDistributedCache<TCacheItem, DeviceIdCacheKey> DeviceIdCache { get; set; }

    protected virtual DeviceIdCacheKey BuildDeviceIdCacheKey(string devideId)
    {
        return new DeviceIdCacheKey()
        {
            Type = this.GetType().FullName,
            DeviceId = devideId,
        };
    }

    public ICurrentHosted CurrentHosted { get; set; }

    /// <summary>
    /// 索引缓存
    /// </summary>
    public IDistributedCacheListSet<string, TIndexCacheKey> IndexListSetCache { get; set; }

    protected virtual IEnumerable<KeyValuePair<TIndexCacheKey, string>> GetKeyValues(TCacheItem connectionPool, CancellationToken token = default)
    {
        var keyValues = new List<KeyValuePair<TIndexCacheKey, string>>();

        return keyValues;
    }

    protected virtual async Task<KeyValuePair<TIndexCacheKey, long>[]> AddIndexAsync(TCacheItem connectionPool, CancellationToken token = default)
    {
        var keyValues = GetKeyValues(connectionPool, token);

        if (!keyValues.Any())
        {
            return [];
        }

        var addedResult = await IndexListSetCache.AddManyAsync(keyValues, () => DistributedCacheEntryOptions, token: token);

        return addedResult;
    }

    protected virtual async Task<KeyValuePair<TIndexCacheKey, long>[]> RemoveIndexAsync(TCacheItem connectionPool, CancellationToken token = default)
    {
        var keyValues = GetKeyValues(connectionPool, token);

        if (!keyValues.Any())
        {
            return [];
        }

        var removed = await IndexListSetCache.RemoveManyAsync(keyValues, () => DistributedCacheEntryOptions, token: token);

        return removed;
    }

    protected virtual async Task DeleteIndexAsync(TCacheItem connectionPool, CancellationToken token = default)
    {
        var keyValues = GetKeyValues(connectionPool, token).Select(x => x.Key);

        if (!keyValues.Any())
        {
            return;
        }

        await IndexListSetCache.DeleteManyAsync(keyValues, token: token);
    }


    protected virtual async Task<List<string>> GetConnectionIdListAsync(CancellationToken token = default)
    {
        return (await AllConnectIdListSetCache.CreateQueryableAsync(ConnectionIdListSetCacheKey, token: token)).ToList();
    }

    /// <inheritdoc />
    public virtual async Task<bool> ConnectedAsync(TCacheItem connectionPool, CancellationToken token = default)
    {
        var addedCount = await AllConnectIdListSetCache.AddAsync(ConnectionIdListSetCacheKey, [connectionPool.ConnectionId], token: token);

        Logger.LogInformation($"{this.GetType().FullName}.Add connection from DistributedCacheListSet addedCount: {addedCount}");

        await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions, token: token);

        if (!string.IsNullOrWhiteSpace(connectionPool.DeviceId))
        {
            await DeviceIdCache.SetAsync(BuildDeviceIdCacheKey(connectionPool.DeviceId), connectionPool, DistributedCacheEntryOptions, token: token);
        }

        await AddIndexAsync(connectionPool, token);

        Logger.LogInformation($"{this.GetType().FullName}.Add connection {connectionPool}");

        return addedCount > 0;
    }

    /// <summary>
    /// 更新活动时间 activeTime
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public virtual async Task<TCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
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

            await DeviceIdCache.SetAsync(BuildDeviceIdCacheKey(connectionPool.DeviceId), connectionPool, DistributedCacheEntryOptions, token: token);

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

            if (!string.IsNullOrEmpty(connectionPool.DeviceId))
            {
                await DeviceIdCache.RemoveAsync(BuildDeviceIdCacheKey(connectionPool.DeviceId), token: token);
            }
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
        Logger.LogInformation($"{this.GetType().FullName}.ClearAllAsync[{reason}] Start:{host},time:{Clock.Now}");

        var connectionIdListByHost = (await CreateQueryableAsync(token))
            .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
            .ToList();

        var keys = connectionIdListByHost.Select(x => GetKeyValues(x, token).Select(d => d.Key)).SelectMany(x => x).Distinct().ToList();

        await IndexListSetCache.DeleteManyAsync(keys, token: token);

        Logger.LogInformation($"{this.GetType().FullName}.ClearAllAsync[{reason}] [{host}] {nameof(IndexListSetCache)} delete keyValues[{keys.Count}]:{keys.JoinAsString(",")}");

        await AllConnectIdListSetCache.DeleteAsync(ConnectionIdListSetCacheKey, token: token);

        Logger.LogInformation($"{this.GetType().FullName}.ClearAllAsync[{reason}] [{host}] {nameof(AllConnectIdListSetCache)} delete key:{ConnectionIdListSetCacheKey}");

        //remove deivceId list
        var deivceIdList = connectionIdListByHost.Where(x => !string.IsNullOrWhiteSpace(x.DeviceId)).Distinct().Select(x => BuildDeviceIdCacheKey(x.DeviceId)).ToList();

        if (deivceIdList.Count > 0)
        {
            await DeviceIdCache.RemoveManyAsync(deivceIdList, token: token);
        }

        // remove conn list
        var connIdList = connectionIdListByHost.Select(x => x.ConnectionId).ToList();

        await ConnectionPoolCache.RemoveManyAsync(connIdList, token: token);

        Logger.LogInformation($"{this.GetType().FullName}.ClearAllAsync[{reason}] [{host}] {nameof(ConnectionPoolCache)} delete  keyValues[{connIdList.Count}]:{connIdList.JoinAsString(",")}");

        Logger.LogInformation($"{this.GetType().FullName}.ClearAllAsync[{reason}] End:{host},time:{Clock.Now}");
    }

    /// <inheritdoc />
    public virtual async Task<IQueryable<TCacheItem>> CreateQueryableAsync(CancellationToken token = default)
    {
        var connectionIdList = await AllConnectIdListSetCache.CreateQueryableAsync(ConnectionIdListSetCacheKey, token: token);

        var list = await ConnectionPoolCache.GetManyAsync(connectionIdList, token: token);

        return list.Where(x => x.Value != null).Select(x => x.Value).AsQueryable();
    }

    /// <inheritdoc />
    public virtual async Task<TCacheItem> GetAsync(string connectionId, CancellationToken token = default)
    {
        return await ConnectionPoolCache.GetAsync(connectionId, token: token);
    }

    /// <inheritdoc />
    public virtual async Task<int> UpdateAllConnectionIdsAsync(CancellationToken token = default)
    {
        Logger.LogWarning($"修复总连接数索引(异常中断) {this.GetType().FullName}.{nameof(UpdateAllConnectionIdsAsync)} Start");

        var connectionIdList = (await CreateQueryableAsync(token)).Select(x => x.ConnectionId).ToList();

        await AllConnectIdListSetCache.ReplaceAsync(ConnectionIdListSetCacheKey, connectionIdList, () => DistributedCacheEntryOptions, token: token);

        Logger.LogWarning($"修复总连接数索引(异常中断) {this.GetType().FullName}.{nameof(UpdateAllConnectionIdsAsync)} End : {connectionIdList.Count}");

        return connectionIdList.Count;
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"{this.GetType().FullName} App Start,HostName:{CurrentHosted.Name}");

        await ClearAllAsync(CurrentHosted.Name, "App Start", cancellationToken);

        // 修复总连接数索引(异常中断)
        await UpdateAllConnectionIdsAsync(cancellationToken);

        await Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"{this.GetType().FullName} App Stop,HostName:{CurrentHosted.Name}");

        await ClearAllAsync(CurrentHosted.Name, "App Stop", cancellationToken);

        await Task.CompletedTask;
    }
}
