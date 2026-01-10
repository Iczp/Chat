using IczpNet.Chat.Clocks;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.RedisServices;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionCacheManager : RedisService, IConnectionCacheManager//, IHostedService
{
    public IOptions<ConnectionOptions> ConnectionOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<ConnectionOptions>>();

    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();

    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();


    protected virtual TimeSpan? CacheExpire => TimeSpan.FromSeconds(ConnectionOptions.Value.ConnectionCacheExpirationSeconds);

    protected virtual string Prefix => $"{Options.Value.KeyPrefix}{ConnectionOptions.Value.AllConnectionsCacheKey}:";

    private static string LuaSAddIfExistsScript => @"
if redis.call('EXISTS', KEYS[1]) == 1 then
    redis.call('SADD', KEYS[1], ARGV[1])
    redis.call('EXPIRE', KEYS[1], ARGV[2])
end
return 1";

    // connKey 
    private string ConnKey(string connectionId)
        => $"{Prefix}Conns:ConnId-{connectionId}";
    // host -> sorted set of connection ids (score = ticks)
    private string HostConnKey(string hostName)
        => $"{Prefix}Hosts:{hostName}";

    /// <summary>
    /// 会话连接
    /// key: connectionId
    /// value: ownerId
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private string SessionConnKey(Guid sessionId)
       => $"{Prefix}Sessions:SessionId-{sessionId}";

    /// <summary>
    /// 设备
    /// key: connectionId
    /// value: deviceType:eviceId
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    private string OwnerDeviceKey(long chatObjectId)
        => $"{Prefix}Owners:Devices:OwnerId-{chatObjectId}";

    /// <summary>
    /// 最后连接时间
    /// element: deviceType:deviceId
    /// score: unixTime
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    private string OwnerLatestZsetKey(long chatObjectId)
        => $"{Prefix}Owners:Latest:OwnerId-{chatObjectId}";

    /// <summary>
    /// 会话
    /// key: connectionId
    /// value: 
    /// </summary>
    /// <param name="chatObjectId"></param>
    /// <returns></returns>
    private string OwnerSessionKey(long chatObjectId)
        => $"{Prefix}Owners:Sessions:OwnerId-{chatObjectId}";

    /// <summary>
    /// 用户连接
    /// key: connectionId
    /// value: ownerId[]
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private string UserConnKey(Guid userId)
        => $"{Prefix}Users:userId-{userId}";

    private string AllHostKey()
        => $"{Prefix}AllHosts";

    private void HashSetConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var connKey = ConnKey(connectionPool.ConnectionId);
        var hashEntries = MapToHashEntries(connectionPool);
        _ = batch.HashSetAsync(connKey, hashEntries);
        Expire(batch, connKey);
    }
    private void SortedSetHostConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        if (string.IsNullOrWhiteSpace(connectionPool.Host))
        {
            return;
        }
        var hostConnKey = HostConnKey(connectionPool.Host);
        var connectionId = connectionPool.ConnectionId;
        _ = batch.SortedSetAddAsync(hostConnKey, connectionId, Clock.Now.ToUnixTimeMilliseconds());
        Expire(batch, hostConnKey);
    }
    private void HashSetUserConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        if (!connectionPool.UserId.HasValue)
        {
            return;
        }
        var userConnKey = UserConnKey(connectionPool.UserId.Value);
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        var ownerIdsStr = ownerIds.JoinAsString(",");
        _ = batch.HashSetAsync(userConnKey, connectionPool.ConnectionId, ownerIdsStr);
        Expire(batch, userConnKey);
    }
    private void HashSetOwnerDevice(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            var ownerDeviceKey = OwnerDeviceKey(ownerId);
            var deviceValue = $"{connectionPool.DeviceType}:{connectionPool.DeviceId}";
            _ = batch.HashSetAsync(ownerDeviceKey, connectionPool.ConnectionId, deviceValue);
            Expire(batch, ownerDeviceKey);
        }

    }
    private void HashSetOwnerSession(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<long, List<Guid>> chatObjectSessionMap)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            var ownerSessionKey = OwnerSessionKey(ownerId);
            // if chatObjectSessions contains it, we assume set was exists or just created. To avoid extra KeyExists roundtrip, only set expire
            // but add members if we have sessions and set wasn't in Redis earlier (GetOrSetSessionsAsync only returns existing/filled).
            var sessions = chatObjectSessionMap.GetValueOrDefault(ownerId);
            if (sessions.IsNullOrEmpty()) continue;

            // Add members (idempotent)
            foreach (var sessionId in sessions)
            {
                _ = batch.SetAddAsync(ownerSessionKey, sessionId.ToString());
            }
            Expire(batch, ownerSessionKey);
        }
    }
    private void HashSetSessionConn(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<Guid, List<long>> sessionChatObjectsMap)
    {
        var connectionId = connectionPool.ConnectionId;
        foreach (var kv in sessionChatObjectsMap)
        {
            var sessionId = kv.Key;
            var owners = kv.Value;
            var sessionConnKey = SessionConnKey(sessionId);
            _ = batch.HashSetAsync(sessionConnKey, connectionId, owners.JoinAsString(","));
            Expire(batch, sessionConnKey);
        }
    }

    private static HashEntry[] MapToHashEntries(ConnectionPoolCacheItem connectionPool)
    {
        var entries = RedisMapper.ToHashEntries(connectionPool);

        var newList = entries.ToList();

        newList.Add(new HashEntry(nameof(connectionPool.ChatObjectIdList), connectionPool.ChatObjectIdList.JoinAsString(",")));

        return newList.ToArray();
    }


    private int ExpireSeconds => (int)(CacheExpire?.TotalSeconds ?? -1);
    private void Expire(IBatch batch, string key, ExpireWhen when = ExpireWhen.Always, CommandFlags flags = CommandFlags.None)
    {
        if (CacheExpire.HasValue)
        {
            _ = batch.KeyExpireAsync(key, CacheExpire, when, flags);
        }
    }

    /// <summary>
    /// Build dictionary: sessionId -> ownerSessions of ownerIds
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
    /// 获取好友会话
    /// key: ownerId
    /// value: sessionId[]
    /// </summary>
    /// <param name="chatObjectIdList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<Dictionary<long, List<Guid>>> GetOrSetOwnerSessionsAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        var result = new Dictionary<long, List<Guid>>();
        foreach (var chatObjectId in chatObjectIdList)
        {
            await SessionUnitManager.LoadFriendsIfNotExistsAsync(chatObjectId);
            var units = await SessionUnitCacheManager.GetFriendsAsync(chatObjectId);
            var sessionIds = units.Select(x => x.SessionId).ToList();
            result.TryAdd(chatObjectId, sessionIds);
        }
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

        var chatObjectSessionMap = await GetOrSetOwnerSessionsAsync(ownerIds, token);

        // Build session -> owners mapping efficiently
        var sessionChatObjectsMap = BuildSessionChatObjects(chatObjectSessionMap);

        // Now create a write batch to set all required keys

        Logger.LogInformation($"[CreateAsync] writeBatch:Database.CreateBatch()");
        var writeBatch = Database.CreateBatch();

        // Host: use sorted set for timestamped connections
        SortedSetHostConn(writeBatch, connectionPool);

        //User
        HashSetUserConn(writeBatch, connectionPool);

        // connectionPool hash
        HashSetConn(writeBatch, connectionPool);

        // owner session sets only if not exists - to avoid race we'll set expire and add members if sessions exist in memory
        HashSetOwnerSession(writeBatch, connectionPool, chatObjectSessionMap);

        // chatObject -> connection hash (owner conn mapping)
        HashSetOwnerDevice(writeBatch, connectionPool);

        // session -> connection hash (session -> connId : owners joined)
        HashSetSessionConn(writeBatch, connectionPool, sessionChatObjectsMap);

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
        var readBatch = Database.CreateBatch();

        var readTask = readBatch.HashGetAsync(connKey, [
            nameof(ConnectionPoolCacheItem.ChatObjectIdList),
            nameof(ConnectionPool.Host),
            nameof(ConnectionPool.UserId),
            nameof(ConnectionPool.DeviceType),
            nameof(ConnectionPool.DeviceId),
            ]);

        readBatch.Execute();

        var redisValues = await readTask;

        var chatObjectIdListValue = redisValues[0];
        var hostValue = redisValues[1];
        var userValue = redisValues[2];
        var deviceType = redisValues[3];
        var deviceId = redisValues[4];

        var chatObjectIdList = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToString().Split(",").Select(x => long.Parse(x)).ToList();

        Logger.LogInformation($"[RefreshExpireAsync] chatObjectIdList: {chatObjectIdList.JoinAsString(",")}");

        // get or set sessions for owners (batch reads inside)
        var chatObjectSessions = await GetOrSetOwnerSessionsAsync(chatObjectIdList, token);

        var sessionChatObjects = BuildSessionChatObjects(chatObjectSessions);

        var writeBatch = Database.CreateBatch();
        var unixTime = Clock.Now.ToUnixTimeMilliseconds();

        // Refresh expire on owner session sets, owner conn hashes, session conn hashes
        foreach (var ownerId in chatObjectIdList)
        {
            Expire(writeBatch, OwnerSessionKey(ownerId));
            Expire(writeBatch, OwnerDeviceKey(ownerId));
            //latest
            _ = writeBatch.SortedSetAddAsync(OwnerLatestZsetKey(ownerId), $"{deviceType}:{deviceId}", unixTime);
        }

        foreach (var kv in sessionChatObjects)
        {
            Expire(writeBatch, SessionConnKey(kv.Key));
        }

        // Host: update timestamp in sorted set
        if (!hostValue.IsNullOrEmpty)
        {
            var hostConnKey = HostConnKey(hostValue.ToString());
            _ = writeBatch.SortedSetAddAsync(hostConnKey, connectionId, unixTime);
            Expire(writeBatch, hostConnKey);
        }

        // User: update 
        if (!userValue.IsNullOrEmpty && Guid.TryParse(userValue.ToString(), out var userId))
        {
            Expire(writeBatch, UserConnKey(userId));
        }

        // ActiveTime
        _ = writeBatch.HashSetAsync(connKey, nameof(ConnectionPool.ActiveTime), Clock.Now.ToRedisValue(), when: When.Always);
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
            var batch = Database.CreateBatch();
            await DeleteConnctionAsync(batch, connectionId, token);
            batch.Execute();
            return true;
        });
    }

    protected virtual async Task DeleteConnctionAsync(IBatch batch, string connectionId, CancellationToken token = default)
    {
        var connKey = ConnKey(connectionId);

        // Read ChatObjectIdList and Host in a read batch
        var readBatch = Database.CreateBatch();
        var chatObjectIdListTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.ChatObjectIdList));
        var hostTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.Host));
        var userTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.UserId));
        readBatch.Execute();

        var chatObjectIdListValue = await chatObjectIdListTask;
        var hostValue = await hostTask;
        var userValue = await userTask;

        var chatObjectIdList = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToString().Split(",").Select(x => long.Parse(x)).ToList();

        // Get sessions per owner (batched inside)
        var chatObjectSessions = await GetOrSetOwnerSessionsAsync(chatObjectIdList, token);

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
        var members = await Database.SortedSetRangeByRankAsync(hostConnKey, 0, -1, Order.Ascending);

        if (members == null || members.Length == 0)
        {
            return;
        }

        var batch = Database.CreateBatch();

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

            var batch = Database.CreateBatch();
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

            await Database.SortedSetRemoveAsync(AllHostKey(), CurrentHosted.Name);

            return true;
        });

    }

    public async Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default)
    {
        var key = UserConnKey(userId);
        var length = await Database.HashLengthAsync(key);
        return length > 0;
    }

    public async Task<bool> IsOnlineAsync(long chatObjectId, CancellationToken token = default)
    {
        var key = OwnerDeviceKey(chatObjectId);
        var length = await Database.HashLengthAsync(key);
        return length > 0;
    }

    public async Task<List<string>> GetDeviceTypesAsync(long chatObjectId, CancellationToken token = default)
    {
        var key = OwnerDeviceKey(chatObjectId);

        var values = await Database.HashValuesAsync(key);
        if (values == null || values.Length == 0)
        {
            return [];
        }

        // DeviceType:DeviceId
        return values
            .Select(v => v.ToString().Split(':')[0])
            .Distinct()
            .ToList();
    }

    public async Task<Dictionary<long, List<string>>> GetDeviceTypesAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        return (await GetDevicesAsync(chatObjectIdList, token))
            .ToDictionary(
                x => x.Key,
                x => x.Value
                    .Select(d => d.DeviceType)
                    .ToList()
             );
    }

    public async Task<Dictionary<long, List<(string ConnectionId, string DeviceId, string DeviceType)>>> GetDevicesAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        var result = new Dictionary<long, List<(string ConnectionId, string DeviceId, string DeviceType)>>(chatObjectIdList.Count);

        if (chatObjectIdList == null || chatObjectIdList.Count == 0)
        {
            return result;
        }

        // 批量读取
        var batch = Database.CreateBatch();

        var taskMap = new Dictionary<long, Task<HashEntry[]>>(chatObjectIdList.Count);

        foreach (var id in chatObjectIdList)
        {
            var ownerDeviceKey = OwnerDeviceKey(id);
            taskMap[id] = batch.HashGetAllAsync(ownerDeviceKey);
        }

        batch.Execute();

        // 解析结果
        foreach (var kv in taskMap)
        {
            var ownerId = kv.Key;
            var entries = await kv.Value;

            if (entries == null || entries.Length == 0)
            {
                result[ownerId] = [];
                continue;
            }

            var types = entries
                .Where(v => !string.IsNullOrWhiteSpace(v.Value.ToString()))
                .Select(v =>
                {
                    // DeviceType:DeviceId → 取 DeviceType
                    var arr = v.Value.ToString().Split(':');

                    return (v.Name.ToString(), arr[0], arr[1]);

                })
                .Distinct()
                .ToList();

            result[ownerId] = types;
        }

        return result;
    }


    public async Task<List<string>> GetDeviceTypesAsync(Guid userId, CancellationToken token = default)
    {
        var userKey = UserConnKey(userId);
        var hash = await Database.HashGetAllAsync(userKey);

        if (hash == null || hash.Length == 0)
        {
            return [];
        }

        var deviceTypes = new HashSet<string>();

        // Hash: connectionId -> ownerIdList "12,45,66"
        foreach (var entry in hash)
        {
            var ownerIdsStr = entry.Value.ToString();
            if (string.IsNullOrWhiteSpace(ownerIdsStr))
            {
                continue;
            }

            foreach (var ownerId in ownerIdsStr.Split(',').Select(long.Parse))
            {
                var ownerKey = OwnerDeviceKey(ownerId);
                var ownerValues = await Database.HashValuesAsync(ownerKey);

                foreach (var v in ownerValues)
                {
                    var deviceType = v.ToString().Split(':')[0];
                    deviceTypes.Add(deviceType);
                }
            }
        }

        return deviceTypes.ToList();
    }

    public async Task<int> GetCountByUserAsync(Guid userId, CancellationToken token = default)
    {
        var userConnKey = UserConnKey(userId);
        var length = await Database.HashLengthAsync(userConnKey);
        return (int)length;
    }

    public async Task<int> GetCountByChatObjectAsync(long chatObjectId, CancellationToken token = default)
    {
        var ownerDeviceKey = OwnerDeviceKey(chatObjectId);
        var length = await Database.HashLengthAsync(ownerDeviceKey);
        return (int)length;
    }

    public async Task<Dictionary<string, List<long>>> GetConnectionsBySessionAsync(Guid sessionId, CancellationToken token = default)
    {
        var sessionConnKey = SessionConnKey(sessionId);

        // Hash entry: field = connId, value = "1,2,3"
        var entries = await Database.HashGetAllAsync(sessionConnKey);

        var result = new Dictionary<string, List<long>>(entries?.Length ?? 0);

        if (entries == null || entries.Length == 0)
        {
            return result;
        }

        foreach (var entry in entries)
        {
            var connId = entry.Name.ToString();
            var ownerList = entry.Value.ToString().Split(",").Select(long.Parse).ToList();  // chatObjectIdList 已经是逗号分隔

            result[connId] = ownerList;
        }

        return result;
    }

    public async Task AddSessionAsync(List<(Guid SessionId, long OwnerId)> ownerSessions)
    {
        var preparedScript = LuaScript.Prepare(LuaSAddIfExistsScript);

        Logger.LogInformation("AddUnitsAsync");

        foreach (var unit in ownerSessions)
        {
            Logger.LogInformation($"(Guid SessionId, long OwnerId): ({unit})");
        }

        var ownerIds = ownerSessions.Select(x => x.OwnerId).Distinct().ToList();

        var deviceDict = await GetDevicesAsync(ownerIds);

        // 全局：connectionId -> ownerId 列表
        var connOwnersMap = deviceDict
            .SelectMany(x => x.Value.Select(d => new { d.ConnectionId, OwnerId = x.Key }))
            .GroupBy(x => x.ConnectionId, x => x.OwnerId)
            .ToDictionary(x => x.Key, x => x.Distinct().ToList());

        // sessionId -> [{OwnerId}]
        var sessionDict = ownerSessions
            .GroupBy(x => x.SessionId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(u => new { u.OwnerId, }).ToList()
            );

        var ownerSessionKeys = ownerIds.Select(OwnerSessionKey);
        var ownerSessionKeyExists = await BatchKeyExistsAsync(ownerSessionKeys);

        var batch = Database.CreateBatch();

        foreach (var (sessionId, units) in sessionDict)
        {
            var sessionConnKey = SessionConnKey(sessionId);
            Expire(batch, sessionConnKey);

            // 1. 当前 session 关联的 ownerIds
            var sessionOwnerIds = units.Select(x => x.OwnerId).Distinct().ToHashSet();

            // 2. 找到这些 ownerIds 所对应的 connectionId 列表
            foreach (var (connectionId, ownerIdList) in connOwnersMap)
            {
                // 得到当前 connectionId 中，属于本 session 的 ownerId 列表
                var relatedOwnerIds = ownerIdList
                    .Where(oid => sessionOwnerIds.Contains(oid))
                    .ToList();

                if (relatedOwnerIds.Count == 0)
                    continue;

                // 3. 写入到 Redis
                var ownerIdStr = string.Join(",", relatedOwnerIds);
                _ = batch.HashSetAsync(sessionConnKey, connectionId, ownerIdStr);

                Logger.LogInformation($"HashSetAsync: sessionConnKey={sessionConnKey}, connectionId={connectionId}, ownerIdStr={ownerIdStr}");
            }

            // 4. ownerId -> sessionId 反向索引
            foreach (var unit in units)
            {
                var ownerSessionKey = OwnerSessionKey(unit.OwnerId);
                //var isExists = await Database.KeyExistsAsync(ownerSessionKey);
                // 缓存Key存在才执行
                if (ownerSessionKeyExists.TryGetValue(ownerSessionKey, out var isExists) && isExists)
                {
                    _ = batch.SetAddAsync(ownerSessionKey, sessionId.ToString());
                    Expire(batch, ownerSessionKey);
                    Logger.LogInformation($"SetAddAsync: ownerSessionKey={ownerSessionKey}, sessionId={sessionId}");
                }
                //_ = preparedScript.EvaluateAsync(batch, new
                //{
                //    KEYS = new[] { ownerSessionKeys },
                //    ARGV = new[] { sessionId.ToString(), ExpireSeconds.ToString() }
                //});
                //Logger.LogInformation($"Lua-SAdd-IfExists: ownerSessionKeys={ownerSessionKeys}, sessionId={sessionId}");
            }
        }
        batch.Execute();
    }


    public async Task AddSession1Async(List<(Guid SessionId, long OwnerId)> ownerSessions)
    {
        var preparedScript = LuaScript.Prepare(LuaSAddIfExistsScript);

        Logger.LogInformation("AddSessionAsync");

        var ownerIds = ownerSessions.Select(x => x.OwnerId).Distinct().ToList();

        // 在线用户 (ownerId -> devices)
        var deviceDict = await GetDevicesAsync(ownerIds);

        // sessionId -> ownerId list
        var sessionDict = ownerSessions
            .GroupBy(x => x.SessionId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(u => u.OwnerId).Distinct().ToList()
            );

        // 新增一个映射：ownerId -> sessionId list
        var ownerSessionMap = ownerSessions
            .GroupBy(x => x.OwnerId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(u => u.SessionId).Distinct().ToList()
            );

        var batch = Database.CreateBatch();

        // 最外层：deviceDict（最小字典）
        foreach (var (ownerId, devices) in deviceDict)
        {
            // 找到该 ownerId 属于哪些会话（session）
            if (!ownerSessionMap.TryGetValue(ownerId, out var ownerSessionIds))
                continue; // 在线但没有 session，跳过

            foreach (var sessionId in ownerSessionIds)
            {
                var sessionConnKey = SessionConnKey(sessionId);
                Expire(batch, sessionConnKey);

                // 当前 ownerId 的所有在线 connection 写入此 session
                foreach (var device in devices)
                {
                    var connectionId = device.ConnectionId;

                    _ = batch.HashSetAsync(sessionConnKey, connectionId, ownerId.ToString());

                    Logger.LogInformation(
                        $"HashSetAsync: sessionId={sessionId}, connId={connectionId}, ownerId={ownerId}"
                    );
                }

                // owner → session 索引（Lua：只有 Key 存在才写入）
                var ownerSessionKey = OwnerSessionKey(ownerId);

                _ = preparedScript.EvaluateAsync(batch, new
                {
                    KEYS = new[] { ownerSessionKey },
                    ARGV = new[] { sessionId.ToString(), ExpireSeconds.ToString() }
                });

                Logger.LogInformation(
                    $"Lua-SAdd-IfExists: ownerSessionKey={ownerSessionKey}, sessionId={sessionId}"
                );
            }
        }

        batch.Execute();
    }

}
