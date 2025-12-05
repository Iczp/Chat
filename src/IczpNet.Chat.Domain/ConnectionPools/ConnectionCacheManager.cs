using IczpNet.Chat.Connections;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionCacheManager : DomainService, IConnectionCacheManager//, IHostedService
{
    public IOptions<AbpDistributedCacheOptions> Options => LazyServiceProvider.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>();

    public IOptions<ConnectionOptions> ConnectionOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<ConnectionOptions>>();

    public IConnectionMultiplexer ConnectionMultiplexer => LazyServiceProvider.LazyGetRequiredService<IConnectionMultiplexer>();

    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();

    protected IDatabase Db => ConnectionMultiplexer.GetDatabase();

    protected virtual TimeSpan? CacheExpire => TimeSpan.FromSeconds(ConnectionOptions.Value.ConnectionCacheExpirationSeconds);

    protected virtual string Prefix => $"{Options.Value.KeyPrefix}{ConnectionOptions.Value.AllConnectionsCacheKey}:";

    // connKey 
    private string ConnKey(string connectionId)
        => $"{Prefix}Conns:ConnId-{connectionId}";
    // host -> sorted set of connection ids (score = ticks)
    private string HostConnKey(string hostName)
        => $"{Prefix}Hosts:{hostName}";
    private string SessionConnKey(Guid sessionId)
       => $"{Prefix}Sessions:SessionId-{sessionId}";

    private string OwnerDeviceKey(long chatObjectId)
        => $"{Prefix}Owners:Devices:OwnerId-{chatObjectId}";
    private string OwnerSessionKey(long chatObjectId)
        => $"{Prefix}Owners:Sessions:OwnerId-{chatObjectId}";

    private string UserConnKey(Guid userId)
        => $"{Prefix}Users:userId-{userId}";

    private string AllHostKey()
        => $"{Prefix}AllHosts";

    private static HashEntry[] MapToHashEntries(ConnectionPoolCacheItem connectionPool)
    {
        var entries = RedisMapper.ToHashEntries(connectionPool);

        var newList = entries.ToList();

        newList.Add(new HashEntry(nameof(connectionPool.ChatObjectIdList), connectionPool.ChatObjectIdList.JoinAsString(",")));

        return newList.ToArray();
    }
    protected virtual async Task<T> MeasureAsync<T>(string name, Func<Task<T>> func)
    {
        var sw = Stopwatch.StartNew();
        var result = await func();
        Logger.LogInformation($"[{this.GetType().FullName}] [{name}] Elapsed Time: {sw.ElapsedMilliseconds} ms");
        sw.Stop();
        return result;
    }

    private void Expire(IBatch batch, string key)
    {
        if (CacheExpire.HasValue)
        {
            _ = batch.KeyExpireAsync(key, CacheExpire);
        }
    }

    /// <summary>
    /// Build dictionary: sessionId -> list of ownerIds
    /// More efficient than repeated LINQ GroupBy
    /// </summary>
    private static Dictionary<Guid, List<long>> BuildSessionChatObjects(Dictionary<long, List<Guid>> ownerSessions)
    {
        var dict = new Dictionary<Guid, List<long>>(ownerSessions.Count * 2);
        foreach (var kv in ownerSessions)
        {
            var ownerId = kv.Key;
            var sessionList = kv.Value;
            if (sessionList == null) continue;
            foreach (var sid in sessionList)
            {
                if (!dict.TryGetValue(sid, out var owners))
                {
                    owners = new List<long>(4);
                    dict[sid] = owners;
                }
                owners.Add(ownerId);
            }
        }
        return dict;
    }

    /// <summary>
    /// Try to get owner->sessions from Redis. For owners with empty sets, fetch from SessionUnitManager and write back to Redis.
    /// All Redis reads are batched; writes are batched separately.
    /// </summary>
    private async Task<Dictionary<long, List<Guid>>> GetOrSetSessionsAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        // Schedule SetMembersAsync for all keys
        Logger.LogInformation($"[GetOrSetSessionsAsync] chatObjectIdList:{chatObjectIdList.JoinAsString(",")}");

        var batchForReads = Db.CreateBatch();
        var setTasks = new Dictionary<long, Task<RedisValue[]>>(chatObjectIdList.Count);
        foreach (var id in chatObjectIdList)
        {
            var key = OwnerSessionKey(id);
            setTasks[id] = batchForReads.SetMembersAsync(key);
        }

        // Execute the read batch
        batchForReads.Execute();

        // Await read results
        var result = new Dictionary<long, List<Guid>>(chatObjectIdList.Count);
        var missingOwnerIds = new List<long>();
        foreach (var id in chatObjectIdList)
        {
            var vals = await setTasks[id];
            if (vals == null || vals.Length == 0)
            {
                missingOwnerIds.Add(id);
            }
            else
            {
                var list = vals.Select(x => Guid.Parse(x)).ToList();
                result[id] = list;
            }
        }

        Logger.LogInformation($"[GetOrSetSessionsAsync] missingOwnerIds:{missingOwnerIds.JoinAsString(",")}");

        if (missingOwnerIds.Count == 0)
        {
            return result;
        }

        // Fetch missing owners' sessions from DB (single batch call)
        var sessionDict = await SessionUnitManager.GetSessionsByChatObjectAsync(missingOwnerIds);

        // Write missing session sets back to Redis in a new batch (non-blocking)
        Logger.LogInformation($"[GetOrSetSessionsAsync] batchForWrites Db.CreateBatch()");
        var batchForWrites = Db.CreateBatch();
        foreach (var ownerId in missingOwnerIds)
        {
            var sessions = sessionDict.GetValueOrDefault(ownerId);
            if (sessions == null || sessions.Count == 0)
            {
                // keep as empty - skip
                continue;
            }

            var ownerKey = OwnerSessionKey(ownerId);
            // Add each member
            foreach (var sid in sessions)
            {
                _ = batchForWrites.SetAddAsync(ownerKey, sid.ToString());
            }
            Expire(batchForWrites, ownerKey);

            result[ownerId] = sessions;
        }
        batchForWrites.Execute();

        Logger.LogInformation($"[GetOrSetSessionsAsync] batchForWrites.Execute()");

        return result;
    }

    [UnitOfWork]
    public Task<bool> ConnectedAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        return MeasureAsync(nameof(CreateAsync), () => CreateAsync(connectionPool, token));
    }

    protected async Task<bool> CreateAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        // Read batch: try to get owner sessions from redis
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        var connectionId = connectionPool.ConnectionId;
        var userId = connectionPool.UserId;
        Logger.LogInformation($"[CreateAsync] connectionId:{connectionId},userId:{userId},ownerIds:{ownerIds.JoinAsString(",")}");

        var chatObjectSessions = await GetOrSetSessionsAsync(ownerIds, token);

        // Build session -> owners mapping efficiently
        var sessionChatObjects = BuildSessionChatObjects(chatObjectSessions);

        // Now create a write batch to set all required keys

        Logger.LogInformation($"[CreateAsync] writeBatch:Db.CreateBatch()");
        var writeBatch = Db.CreateBatch();

        // Host: use sorted set for timestamped connections
        var hostConnKey = HostConnKey(connectionPool.Host);
        _ = writeBatch.SortedSetAddAsync(hostConnKey, connectionId, Clock.Now.Ticks);
        Expire(writeBatch, hostConnKey);

        //User
        if (userId.HasValue)
        {
            var userConnKey = UserConnKey(userId.Value);
            _ = writeBatch.HashSetAsync(userConnKey, connectionId, ownerIds.JoinAsString(","));
            Expire(writeBatch, userConnKey);
        }

        // connectionPool hash
        var hashEntries = MapToHashEntries(connectionPool);
        _ = writeBatch.HashSetAsync(ConnKey(connectionId), hashEntries);
        Expire(writeBatch, ConnKey(connectionId));

        // owner session sets only if not exists - to avoid race we'll set expire and add members if sessions exist in memory
        foreach (var ownerId in ownerIds)
        {
            var ownerKey = OwnerSessionKey(ownerId);
            // if chatObjectSessions contains it, we assume set was exists or just created. To avoid extra KeyExists roundtrip, only set expire
            // but add members if we have sessions and set wasn't in Redis earlier (GetOrSetSessionsAsync only returns existing/filled).
            var sessions = chatObjectSessions.GetValueOrDefault(ownerId);
            if (sessions.IsNullOrEmpty()) continue;

            // Add members (idempotent)
            foreach (var sid in sessions)
            {
                _ = writeBatch.SetAddAsync(ownerKey, sid.ToString());
            }
            Expire(writeBatch, ownerKey);
        }

        // chatObject -> connection hash (owner conn mapping)
        foreach (var ownerId in ownerIds)
        {
            var ownerDeviceKey = OwnerDeviceKey(ownerId);
            var deviceValue = $"{connectionPool.DeviceType}:{connectionPool.DeviceId}";
            _ = writeBatch.HashSetAsync(ownerDeviceKey, connectionId, deviceValue);
            Expire(writeBatch, ownerDeviceKey);
        }

        // session -> connection hash (session -> connId : owners joined)
        foreach (var kv in sessionChatObjects)
        {
            var sessionId = kv.Key;
            var owners = kv.Value;
            var sessionConnKey = SessionConnKey(sessionId);
            _ = writeBatch.HashSetAsync(sessionConnKey, connectionId, owners.JoinAsString(","));
            Expire(writeBatch, sessionConnKey);
        }

        writeBatch.Execute();

        Logger.LogInformation($"[CreateAsync] writeBatch.Execute()");

        return true;
    }

    public virtual Task<ConnectionPoolCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
    {
        return MeasureAsync(nameof(RefreshExpireAsync), () => RefreshExpireAsync(connectionId, token));
    }

    protected virtual async Task<ConnectionPoolCacheItem> RefreshExpireAsync(string connectionId, CancellationToken token = default)
    {
        Logger.LogInformation($"[RefreshExpireAsync] connectionId: {connectionId}");

        var connKey = ConnKey(connectionId);
        var readBatch = Db.CreateBatch();

        // Schedule reading ChatObjectIdList and Host
        var chatObjectIdListTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.ChatObjectIdList));
        var hostTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPool.Host));
        var userTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPool.UserId));

        readBatch.Execute();

        var chatObjectIdListValue = await chatObjectIdListTask;
        var hostValue = await hostTask;
        var userValue = await userTask;

        var chatObjectIdList = chatObjectIdListValue.IsNull ? new List<long>() : chatObjectIdListValue.ToString().Split(",").Select(x => long.Parse(x)).ToList();

        Logger.LogInformation($"[RefreshExpireAsync] chatObjectIdList: {chatObjectIdList.JoinAsString(",")}");

        // get or set sessions for owners (batch reads inside)
        var chatObjectSessions = await GetOrSetSessionsAsync(chatObjectIdList, token);

        var sessionChatObjects = BuildSessionChatObjects(chatObjectSessions);

        var writeBatch = Db.CreateBatch();

        // Refresh expire on owner session sets, owner conn hashes, session conn hashes
        foreach (var ownerId in chatObjectIdList)
        {
            var ownerSessionKey = OwnerSessionKey(ownerId);
            Expire(writeBatch, ownerSessionKey);

            var ownerDeviceKey = OwnerDeviceKey(ownerId);
            Expire(writeBatch, ownerDeviceKey);
        }

        foreach (var kv in sessionChatObjects)
        {
            var sessionConnKey = SessionConnKey(kv.Key);
            Expire(writeBatch, sessionConnKey);
        }

        // Host: update timestamp in sorted set
        if (!hostValue.IsNullOrEmpty)
        {
            var hostConnKey = HostConnKey(hostValue.ToString());
            _ = writeBatch.SortedSetAddAsync(hostConnKey, connectionId, Clock.Now.Ticks);
            Expire(writeBatch, hostConnKey);
        }

        // User: update 
        if (!userValue.IsNullOrEmpty && Guid.TryParse(userValue.ToString(), out var userId))
        {
            var userConnKey = UserConnKey(userId);
            Expire(writeBatch, userConnKey);
        }

        // expire conn hash itself
        Expire(writeBatch, connKey);

        writeBatch.Execute();

        Logger.LogInformation($"[RefreshExpireAsync] writeBatch.Execute()");
        // Original method returned default; to be safe keep behavior unchanged.
        return default;
    }

    public async Task<bool> DisconnectedAsync(string connectionId, CancellationToken token = default)
    {
        return await MeasureAsync(nameof(DisconnectedAsync), async () =>
        {
            var batch = Db.CreateBatch();
            await DeleteConnctionAsync(batch, connectionId, token);
            batch.Execute();
            return true;
        });
    }

    protected virtual async Task DeleteConnctionAsync(IBatch batch, string connectionId, CancellationToken token = default)
    {
        var connKey = ConnKey(connectionId);

        // Read ChatObjectIdList and Host in a read batch
        var readBatch = Db.CreateBatch();
        var chatObjectIdListTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.ChatObjectIdList));
        var hostTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.Host));
        var userTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.UserId));
        readBatch.Execute();

        var chatObjectIdListValue = await chatObjectIdListTask;
        var hostValue = await hostTask;
        var userValue = await userTask;

        var chatObjectIdList = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToString().Split(",").Select(x => long.Parse(x)).ToList();

        // Get sessions per owner (batched inside)
        var chatObjectSessions = await GetOrSetSessionsAsync(chatObjectIdList, token);

        var sessionChatObjects = BuildSessionChatObjects(chatObjectSessions);

        // Remove connection from owner -> conn hash, session -> conn hash, host sorted set
        // Use provided batch (caller may be a batch)
        foreach (var ownerId in chatObjectIdList)
        {
            var ownerDeviceKey = OwnerDeviceKey(ownerId);
            _ = batch.HashDeleteAsync(ownerDeviceKey, connectionId);
            Expire(batch, ownerDeviceKey);
        }

        foreach (var kv in sessionChatObjects)
        {
            var sessionConnKey = SessionConnKey(kv.Key);
            _ = batch.HashDeleteAsync(sessionConnKey, connectionId);
            Expire(batch, sessionConnKey);
        }

        // Host
        if (!hostValue.IsNullOrEmpty)
        {
            var hostConnKey = HostConnKey(hostValue.ToString());
            _ = batch.SortedSetRemoveAsync(hostConnKey, connectionId);
            Expire(batch, hostConnKey);
        }

        //User
        if (!userValue.IsNullOrEmpty && Guid.TryParse(userValue.ToString(), out var userId))
        {
            var userConnKey = UserConnKey(userId);
            _ = batch.HashDeleteAsync(userConnKey, connectionId);
            Expire(batch, userConnKey);
        }

        // Delete conn hash
        _ = batch.KeyDeleteAsync(connKey);

        // Note: we do not execute batch here because caller may pass batch and execute later.
    }


    public async Task DeleteByHostNameAsync(string hostHame)
    {
        var hostConnKey = HostConnKey(hostHame.ToString());

        // get connection ids from sorted set (members only)
        var members = await Db.SortedSetRangeByRankAsync(hostConnKey, 0, -1, Order.Ascending);

        if (members == null || members.Length == 0)
        {
            return;
        }

        var batch = Db.CreateBatch();

        foreach (var member in members)
        {
            var connectionId = member.ToString();
            // DeleteConnctionAsync will schedule deletes on batch
            await DeleteConnctionAsync(batch, connectionId);
        }

        // remove host in allHosts
        _ = batch.SortedSetRemoveAsync(AllHostKey(), CurrentHosted.Name);

        batch.Execute();
    }

    // These methods implement IHostedService (if you want them enabled, uncomment interface)
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await MeasureAsync(nameof(StartAsync), async () =>
        {
            Logger.LogWarning($"[App Start] DeleteByHostNameAsync,HostName:{CurrentHosted.Name}");

            await DeleteByHostNameAsync(CurrentHosted.Name);

            var batch = Db.CreateBatch();
            _ = batch.SortedSetAddAsync(AllHostKey(), CurrentHosted.Name, Clock.Now.Ticks);
            _ = batch.KeyExpireAsync(AllHostKey(), TimeSpan.FromDays(7));
            batch.Execute();

            return true;
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await MeasureAsync(nameof(StopAsync), async () =>
        {
            Logger.LogWarning($"[App Stop] DeleteByHostNameAsync,HostName:{CurrentHosted.Name}");

            await DeleteByHostNameAsync(CurrentHosted.Name);

            await Db.SortedSetRemoveAsync(AllHostKey(), CurrentHosted.Name);

            return true;
        });

    }
}
