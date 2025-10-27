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
    IDistributedCacheListSet<string, Guid> userConnectionIdListSetCache,
    IDistributedCacheListSet<string, string> connectIdListSetCache,
    IOptions<ConnectionOptions> options
    ) : DomainService, IConnectionPoolManager
{

    /// <summary>
    /// 连接缓存
    /// </summary>
    public IDistributedCacheListSet<string, string> ConnectIdListSetCache { get; } = connectIdListSetCache;
    /// <summary>
    /// 用户连接缓存
    /// </summary>
    public IDistributedCacheListSet<string, Guid> UserConnectionIdListSetCache { get; } = userConnectionIdListSetCache;

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

    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Config.ConnectionCacheExpirationSeconds)
    };

    protected async Task<List<string>> GetConnectionIdsAsync(CancellationToken token = default)
    {
        return (await ConnectIdListSetCache.GetAllAsync(ConnectionIdListSetCacheKey, token: token)).ToList();
    }

    protected async Task<List<string>> GetConnectionIdsAsync(string host, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return await GetConnectionIdsAsync(token);
        }

        return [.. (await CreateQueryableAsync(token)).Where(x => x.Host == host).Select(x => x.ConnectionId)];
    }


    protected virtual async Task<List<string>> AddUserConnetionIdsAsync(Guid userId, string connetionId, CancellationToken token = default)
    {
        var userConnectionIdList = (await UserConnectionIdListSetCache.AddAsync(userId, [connetionId], token: token)) ;

        Logger.LogInformation($"AddUserConnetionIdsAsync:UserId={userId}, connetionId={connetionId}, userConnectionIdList.Count={userConnectionIdList.Count()}");

        return userConnectionIdList.ToList();
    }

    protected virtual async Task<List<string>> RemoveUserConnetionIdsAsync(Guid userId, string connetionId, CancellationToken token = default)
    {
        var userConnectionIdList = (await UserConnectionIdListSetCache.RemoveAsync(userId, [connetionId], token: token));

        Logger.LogInformation($"RemoveUserConnetionIdsAsync:UserId={userId}, before remove userConnectionIdList.Count={userConnectionIdList.Count()}");

        Logger.LogInformation($"RemoveUserConnetionIdsAsync:UserId={userId}, connetionId={connetionId}, userConnectionIdList.Count={userConnectionIdList.Count()},userConnectionIdList Count=[{userConnectionIdList.Count()}]");

        return userConnectionIdList.ToList();
    }
    /// <inheritdoc />
    public async Task<bool> AddAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var connectionList = await ConnectIdListSetCache.AddAsync(ConnectionIdListSetCacheKey, [connectionPool.ConnectionId], token: token);

        Logger.LogInformation($"Add connection from DistributedCacheListSet Count: {connectionList.Count()}");

        await ConnectionPoolCache.SetAsync(connectionPool.ConnectionId, connectionPool, DistributedCacheEntryOptions, token: token);

        if (connectionPool.UserId.HasValue)
        {
            await AddUserConnetionIdsAsync(connectionPool.UserId.Value, connectionPool.ConnectionId, token);
            Logger.LogInformation($"AddUserConnetionIdsAsync UserId={connectionPool.UserId.Value}, Count={connectionList.Count()}");
        }

        Logger.LogInformation($"Add connection {connectionPool}");

        Logger.LogInformation($"Online totalCount {connectionList.Count()}");

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

        var connectionIdList = await ConnectIdListSetCache.RemoveAsync(ConnectionIdListSetCacheKey, [connectionPool.ConnectionId], token: token);

        Logger.LogInformation($"Remove connection from DistributedCacheListSet : {connectionIdList.JoinAsString(",")}");

        if (connectionPool != null && connectionPool.UserId.HasValue)
        {
            await RemoveUserConnetionIdsAsync(connectionPool.UserId.Value, connectionId, token);
        }

        //var connectionIdList = await GetConnectionIdsAsync(token: token);

        Logger.LogInformation($"Online totalCount before delete: {connectionIdList.Count()}");

        //if (!connectionIdList.Remove(connectionId))
        //{
        //    Logger.LogInformation($"删除失败： {connectionId}");
        //}

        //await SetConnectionIdsAsync(connectionIdList, token: token);

        await ConnectionPoolCache.RemoveAsync(connectionId, token: token);

        Logger.LogInformation($"Remove connection {connectionId}");

        Logger.LogInformation($"Online totalCount {connectionIdList.Count()}");

        return true;
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

        //var connectionIdList = await GetConnectionIdsAsync(token);

        var connectionIdList = await ConnectIdListSetCache.GetAllAsync(ConnectionIdListSetCacheKey, token: token);

        foreach (var connectionId in connectionIdList)
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
        await ConnectIdListSetCache.DeleteAsync(ConnectionIdListSetCacheKey, token: token);

        var connectionIdListByHost = (await CreateQueryableAsync(token))
            .WhereIf(!string.IsNullOrWhiteSpace(host), x => x.Host == host)
            .ToList();

        foreach (var connectionPool in connectionIdListByHost)
        {
            await RemoveAsync(connectionPool, token);

            if (connectionPool.UserId.HasValue)
            {
                await RemoveUserConnetionIdsAsync(connectionPool.UserId.Value, connectionPool.ConnectionId, token);
            }
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

        await ConnectIdListSetCache.ReplaceAsync(ConnectionIdListSetCacheKey, connectionIdList, () => DistributedCacheEntryOptions, token: token);

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
        return (await UserConnectionIdListSetCache.GetAllAsync(userId, token: token)).ToList();
    }

    /// <inheritdoc />
    public async Task<int> UpdateUserConnectionIdsAsync(Guid userId, CancellationToken token = default)
    {
        var userConnectionIds = (await CreateQueryableAsync(token))
            .Where(x => x.UserId == userId)
            .Select(x => x.ConnectionId)
            .ToList();

        await UserConnectionIdListSetCache.ReplaceAsync(userId, userConnectionIds, () => DistributedCacheEntryOptions, token: token);

        return userConnectionIds.Count;
    }

    public async Task<List<ConnectionPoolCacheItem>> GetListByChatObjectIdAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        return (await GetAllListAsync(token))
            .Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .ToList();
    }


}
