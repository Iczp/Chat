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
using Volo.Abp.Json;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ConnectionPools;

public class ConnectionCacheManager : RedisService, IConnectionCacheManager//, IHostedService
{
    public IOptions<ConnectionOptions> ConnectionOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<ConnectionOptions>>();

    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();

    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();

    protected virtual TimeSpan? CacheExpire => TimeSpan.FromSeconds(ConnectionOptions.Value.ConnectionCacheExpirationSeconds);

    protected virtual string Prefix => $"{Options.Value.KeyPrefix}{ConnectionOptions.Value.AllConnectionsCacheKey}:";

    private static string LuaSAddIfExistsScript => @"
if redis.call('EXISTS', KEYS[1]) == 1 then
    redis.call('SADD', KEYS[1], ARGV[1])
    redis.call('EXPIRE', KEYS[1], ARGV[2])
end
return 1";

    // connKey 
    private string ConnHashKey(string connectionId)
        => $"{Prefix}Conns:ConnId-{connectionId}";
    // host -> sorted set of connection ids (score = ticks)
    private string HostConnZsetKey(string hostName)
        => $"{Prefix}Hosts:{hostName}";

    /// <summary>
    /// 会话连接
    /// key: connectionId,
    /// value: ownerId,
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private string SessionConnHashKey(Guid sessionId)
       => $"{Prefix}Sessions:SessionId-{sessionId}";

    /// <summary>
    /// 设备
    /// key: connectionId,
    /// value: deviceType:eviceId,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerDeviceHashKey(long ownerId)
        => $"{Prefix}Owners:Devices:OwnerId-{ownerId}";

    /// <summary>
    /// 最后连接时间
    /// element: deviceType:deviceId,
    /// score: unixTime,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerLatestZsetKey(long ownerId)
        => $"{Prefix}Owners:Latest:OwnerId-{ownerId}";

    private string FriendLatestZsetKey(long friendOwnerId)
        => $"{Prefix}Owners:Friends:OwnerId-{friendOwnerId}";

    /// <summary>
    /// 会话
    /// key: connectionId,
    /// value: ,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private string OwnerSessionsHashKey(long ownerId)
        => $"{Prefix}Owners:Sessions:OwnerId-{ownerId}";

    /// <summary>
    /// 用户连接
    /// key: connectionId,
    /// value: ownerId[],
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private string UserConnKey(Guid userId)
        => $"{Prefix}Users:userId-{userId}";

    private string AllHostZsetKey()
        => $"{Prefix}AllHosts";

    private void HashSetConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var connKey = ConnHashKey(connectionPool.ConnectionId);
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
        var hostConnKey = HostConnZsetKey(connectionPool.Host);
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
        var ownerIdsStr = JsonSerializer.Serialize(ownerIds);
        _ = batch.HashSetAsync(userConnKey, connectionPool.ConnectionId, ownerIdsStr);
        Expire(batch, userConnKey);
    }
    private void HashSetOwnerDevice(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            var ownerDeviceKey = OwnerDeviceHashKey(ownerId);
            var deviceValue = $"{connectionPool.DeviceType}:{connectionPool.DeviceId}";
            _ = batch.HashSetAsync(ownerDeviceKey, connectionPool.ConnectionId, deviceValue);
            Expire(batch, ownerDeviceKey);
        }

    }
    private void HashSetOwnerSessions(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<long, IEnumerable<FriendModel>> friendsMap)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            var ownerSessionKey = OwnerSessionsHashKey(ownerId);
            // if chatObjectSessions contains it, we assume set was exists or just created. To avoid extra KeyExists roundtrip, only set expire
            // but add members if we have sessions and set wasn't in Redis earlier (GetOrSetSessionsAsync only returns existing/filled).
            var sessions = friendsMap.GetValueOrDefault(ownerId).Select(x => x.SessionId);
            if (sessions == null || !sessions.Any()) continue;

            // Add members (idempotent)
            foreach (var sessionId in sessions)
            {
                _ = batch.SetAddAsync(ownerSessionKey, sessionId.ToString());
            }
            Expire(batch, ownerSessionKey);
        }
    }

    private void SortedActionOwnerFriends(List<long> ownerIds, Dictionary<long, IEnumerable<FriendModel>> friendsMap, Action<string, SessionUnitElement> action)
    {
        foreach (var ownerId in ownerIds)
        {
            //friend latest
            var friends = friendsMap.GetValueOrDefault(ownerId);
            foreach (var item in friends)
            {
                var friendLatestZsetKey = FriendLatestZsetKey(item.FriendId);
                var element = SessionUnitElement.Create(item.OwnerId, item.FriendId, item.Id, item.SessionId);
                action(friendLatestZsetKey, element);
            }
        }
    }

    private void SortedSetOwnerFriends(IBatch batch, List<long> ownerIds, Dictionary<long, IEnumerable<FriendModel>> friendsMap, double unixTime)
    {
        SortedActionOwnerFriends(ownerIds, friendsMap, (friendLatestZsetKey, element) =>
        {
            _ = batch.SortedSetAddAsync(friendLatestZsetKey, element, unixTime);
            Expire(batch, friendLatestZsetKey);
        });
    }

    private void SortedRemoveOwnerFriends(IBatch batch, List<long> ownerIds, Dictionary<long, IEnumerable<FriendModel>> friendsMap)
    {
        SortedActionOwnerFriends(ownerIds, friendsMap, (friendLatestZsetKey, element) =>
        {
            _ = batch.SortedSetRemoveAsync(friendLatestZsetKey, element);
            Expire(batch, friendLatestZsetKey);
        });
    }
    private void HashSetSessionConn(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<Guid, List<long>> sessionChatObjectsMap)
    {
        var connectionId = connectionPool.ConnectionId;
        foreach (var kv in sessionChatObjectsMap)
        {
            var sessionId = kv.Key;
            var owners = kv.Value;
            var sessionConnKey = SessionConnHashKey(sessionId);
            _ = batch.HashSetAsync(sessionConnKey, connectionId, owners.JoinAsString(","));
            Expire(batch, sessionConnKey);
        }
    }

    private static HashEntry[] MapToHashEntries(ConnectionPoolCacheItem connectionPool)
    {
        var entries = RedisMapper.ToHashEntries(connectionPool);

        var newList = entries.ToList();

        //newList.Add(new HashEntry(nameof(connectionPool.ChatObjectIdList), connectionPool.ChatObjectIdList.JoinAsString(",")));

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
    private static Dictionary<Guid, List<long>> BuildSessionChatObjects(Dictionary<long, IEnumerable<FriendModel>> friendsMap)
    {
        var dict = new Dictionary<Guid, List<long>>(friendsMap.Count * 2);
        foreach (var kv in friendsMap)
        {
            var ownerId = kv.Key;
            var sessionList = kv.Value.Select(x => x.SessionId);
            if (sessionList == null) continue;
            foreach (var friend in sessionList)
            {
                if (!dict.TryGetValue(friend, out var owners))
                {
                    owners = new List<long>(4);
                    dict[friend] = owners;
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
    private async Task<Dictionary<long, IEnumerable<FriendModel>>> GetOrSetFriendsAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        var result = new Dictionary<long, IEnumerable<FriendModel>>();
        foreach (var chatObjectId in chatObjectIdList)
        {
            await SessionUnitManager.LoadFriendsIfNotExistsAsync(chatObjectId);
            var friends = await SessionUnitCacheManager.GetFriendsAsync(chatObjectId);
            result.TryAdd(chatObjectId, friends);
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

        var friendsMap = await GetOrSetFriendsAsync(ownerIds, token);

        // Build session -> owners mapping efficiently
        var sessionOwnersMap = BuildSessionChatObjects(friendsMap);

        // Now create a write batch to set all required keys

        Logger.LogInformation($"[CreateAsync] writeBatch:Database.CreateBatch()");
        var batch = Database.CreateBatch();

        // Host: use sorted set for timestamped connections
        SortedSetHostConn(batch, connectionPool);

        //User
        HashSetUserConn(batch, connectionPool);

        // connectionPool hash
        HashSetConn(batch, connectionPool);

        // owner session sets only if not exists - to avoid race we'll set expire and add members if sessions exist in memory
        HashSetOwnerSessions(batch, connectionPool, friendsMap);

        //
        SortedSetOwnerFriends(batch, ownerIds, friendsMap, Clock.Now.ToUnixTimeMilliseconds());

        // chatObject -> connection hash (owner conn mapping)
        HashSetOwnerDevice(batch, connectionPool);

        // session -> connection hash (session -> connId : owners joined)
        HashSetSessionConn(batch, connectionPool, sessionOwnersMap);

        batch.Execute();

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

        var connKey = ConnHashKey(connectionId);
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

        var ownerIds = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToList<long>();

        Logger.LogInformation($"[RefreshExpireAsync] chatObjectIdList: {ownerIds.JoinAsString(",")}");

        // get or set sessions for owners (batch reads inside)
        var friendsMap = await GetOrSetFriendsAsync(ownerIds, token);

        var sessionChatObjects = BuildSessionChatObjects(friendsMap);

        var batch = Database.CreateBatch();
        var unixTime = Clock.Now.ToUnixTimeMilliseconds();

        // Refresh expire on owner session sets, owner conn hashes, session conn hashes
        foreach (var ownerId in ownerIds)
        {
            Expire(batch, OwnerSessionsHashKey(ownerId));
            Expire(batch, OwnerDeviceHashKey(ownerId));
            //latest
            _ = batch.SortedSetAddAsync(OwnerLatestZsetKey(ownerId), $"{deviceType}:{deviceId}", unixTime);
        }

        // 
        SortedSetOwnerFriends(batch, ownerIds, friendsMap, unixTime);

        foreach (var kv in sessionChatObjects)
        {
            Expire(batch, SessionConnHashKey(kv.Key));
        }

        // Host: update timestamp in sorted set
        if (!hostValue.IsNullOrEmpty)
        {
            var hostConnKey = HostConnZsetKey(hostValue.ToString());
            _ = batch.SortedSetAddAsync(hostConnKey, connectionId, unixTime);
            Expire(batch, hostConnKey);
        }

        // User: update 
        if (!userValue.IsNullOrEmpty && Guid.TryParse(userValue.ToString(), out var userId))
        {
            Expire(batch, UserConnKey(userId));
        }

        // ActiveTime
        _ = batch.HashSetAsync(connKey, nameof(ConnectionPool.ActiveTime), Clock.Now.ToRedisValue(), when: When.Always);
        // expire conn hash itself
        Expire(batch, connKey);

        batch.Execute();

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
        var connKey = ConnHashKey(connectionId);

        // Read ChatObjectIdList and Host in a read batch
        var readBatch = Database.CreateBatch();
        var chatObjectIdListTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.ChatObjectIdList));
        var hostTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.Host));
        var userTask = readBatch.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.UserId));
        readBatch.Execute();

        var chatObjectIdListValue = await chatObjectIdListTask;
        var hostValue = await hostTask;
        var userValue = await userTask;

        var chatObjectIdList = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToList<long>();

        // Get sessions per owner (batched inside)
        var friendsMap = await GetOrSetFriendsAsync(chatObjectIdList, token);

        var sessionChatObjects = BuildSessionChatObjects(friendsMap);

        // Remove connection from owner -> conn hash, session -> conn hash, host sorted set
        // Use provided batch (caller may be a batch)

        // OwnerDevice
        foreach (var ownerId in chatObjectIdList)
        {
            var ownerDeviceKey = OwnerDeviceHashKey(ownerId);
            _ = batch.HashDeleteAsync(ownerDeviceKey, connectionId);
            Expire(batch, ownerDeviceKey);
        }

        // SessionConn
        foreach (var kv in sessionChatObjects)
        {
            var sessionConnKey = SessionConnHashKey(kv.Key);
            _ = batch.HashDeleteAsync(sessionConnKey, connectionId);
            Expire(batch, sessionConnKey);
        }

        // Host
        if (!hostValue.IsNullOrEmpty)
        {
            var hostConnKey = HostConnZsetKey(hostValue.ToString());
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

        // Friends
        SortedRemoveOwnerFriends(batch, chatObjectIdList, friendsMap);

        // Delete conn hash
        _ = batch.KeyDeleteAsync(connKey);

        // Note: we do not execute batch here because caller may pass batch and execute later.
    }
    

    public async Task<long> DeleteByHostNameAsync(string hostHame)
    {
        // get connection ids from sorted set (members only)
        var connIds = (await GetConnectionsByHostAsync(hostHame)).ToList();

        if (connIds == null || connIds.Count == 0)
        {
            return 0;
        }

        var batch = Database.CreateBatch();

        foreach (var connectionId in connIds)
        {
            // DeleteConnctionAsync will schedule deletes on batch
            await DeleteConnctionAsync(batch, connectionId);
        }

        // remove host in allHosts
        _ = batch.SortedSetRemoveAsync(AllHostZsetKey(), CurrentHosted.Name);
        batch.Execute();

        return connIds.Count;
    }

    // These methods implement IHostedService (if you want them enabled, uncomment interface)
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await MeasureAsync(nameof(StartAsync), async () =>
        {
            Logger.LogWarning($"[App Start] DeleteByHostNameAsync,HostName:{CurrentHosted.Name}");

            await DeleteByHostNameAsync(CurrentHosted.Name);

            var batch = Database.CreateBatch();
            var allHostKey = AllHostZsetKey();
            _ = batch.SortedSetAddAsync(allHostKey, CurrentHosted.Name, Clock.Now.ToUnixTimeMilliseconds());
            _ = batch.KeyExpireAsync(allHostKey, TimeSpan.FromDays(7));
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

            await Database.SortedSetRemoveAsync(AllHostZsetKey(), CurrentHosted.Name);

            return true;
        });

    }

    public async Task<ConnectionPoolCacheItem> GetAsync(string connectionId, CancellationToken token = default)
    {
        var items = await GetManyAsync([connectionId], token);
        return items.FirstOrDefault().Value;
    }

    public async Task<Dictionary<string, ConnectionPoolCacheItem>> GetManyAsync(List<string> connectionIds, CancellationToken token = default)
    {
        var batch = Database.CreateBatch();
        var tasks = connectionIds.Distinct().ToDictionary(x => x, x => batch.HashGetAllAsync(ConnHashKey(x)));
        batch.Execute();

        await Task.WhenAll(tasks.Values);
        var result = tasks.ToDictionary(x => x.Key, x => x.Value.Result.ToObject<ConnectionPoolCacheItem>());
        return result;
    }

    public async Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default)
    {
        var length = await Database.HashLengthAsync(UserConnKey(userId));
        return length > 0;
    }

    public async Task<bool> IsOnlineAsync(long ownertId, CancellationToken token = default)
    {
        var length = await Database.HashLengthAsync(OwnerDeviceHashKey(ownertId));
        return length > 0;
    }

    public async Task<IEnumerable<OwnerLatestOnline>> GetLatestOnlineAsync(long ownertId, CancellationToken token = default)
    {
        var key = OwnerLatestZsetKey(ownertId);
        var result = await Database.SortedSetRangeByScoreWithScoresAsync(key, order: Order.Descending);
        return result.Select(x =>
        {
            var element = DeviceElement.Parse(x.Element);
            return new OwnerLatestOnline()
            {
                DeviceId = element.DeviceId,
                DeviceType = element.DeviceType,
                LatestTime = x.Score.ToLocalDateTime(),
            };
        });
    }

    public async Task<List<string>> GetDeviceTypesAsync(long ownertId, CancellationToken token = default)
    {
        var key = OwnerDeviceHashKey(ownertId);

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

    public async Task<Dictionary<long, List<string>>> GetDeviceTypesAsync(List<long> ownerIds, CancellationToken token = default)
    {
        return (await GetDevicesAsync(ownerIds, token))
            .ToDictionary(
                x => x.Key,
                x => x.Value
                    .Select(d => d.DeviceType)
                    .ToList()
             );
    }

    public async Task<Dictionary<long, List<DeviceModel>>> GetDevicesAsync(List<long> ownerIds, CancellationToken token = default)
    {
        var result = new Dictionary<long, List<DeviceModel>>(ownerIds.Count);

        if (ownerIds == null || ownerIds.Count == 0)
        {
            return result;
        }

        // 批量读取
        var batch = Database.CreateBatch();

        var taskMap = new Dictionary<long, Task<HashEntry[]>>(ownerIds.Count);

        foreach (var id in ownerIds)
        {
            var ownerDeviceKey = OwnerDeviceHashKey(id);
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
                    var element = DeviceElement.Parse(v.Value);
                    return new DeviceModel()
                    {
                        OwnerId = ownerId,
                        ConnectionId = v.Name.ToString(),
                        DeviceType = element.DeviceType,
                        DeviceId = element.DeviceId,
                    };
                })
                .Distinct()
                .ToList();

            result[ownerId] = types;
        }

        return result;
    }


    public async Task<IEnumerable<string>> GetDeviceTypesAsync(Guid userId, CancellationToken token = default)
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
                var ownerKey = OwnerDeviceHashKey(ownerId);
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

    public async Task<IEnumerable<string>> GetConnectionsByUserAsync(Guid userId, CancellationToken token = default)
    {
        var entries = await Database.HashGetAllAsync(UserConnKey(userId));
        return entries.Select(x => x.Name.ToString());
    }

    public async Task<long> GetCountByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await Database.HashLengthAsync(UserConnKey(userId));
    }

    public async Task<long> GetCountByOwnerAsync(long ownerId, CancellationToken token = default)
    {
        return await Database.HashLengthAsync(OwnerDeviceHashKey(ownerId));
    }

    public async Task<Dictionary<string, List<long>>> GetConnectionsBySessionAsync(Guid sessionId, CancellationToken token = default)
    {
        var sessionConnKey = SessionConnHashKey(sessionId);

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

        var ownerSessionKeys = ownerIds.Select(OwnerSessionsHashKey);
        var ownerSessionKeyExists = await BatchKeyExistsAsync(ownerSessionKeys);

        var batch = Database.CreateBatch();

        foreach (var (sessionId, units) in sessionDict)
        {
            var sessionConnKey = SessionConnHashKey(sessionId);
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
                var ownerSessionKey = OwnerSessionsHashKey(unit.OwnerId);
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
                var sessionConnKey = SessionConnHashKey(sessionId);
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
                var ownerSessionKey = OwnerSessionsHashKey(ownerId);

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

    public async Task<Dictionary<string, DateTime?>> GetAllHostsAsync()
    {
        var allHost = await Database.SortedSetRangeByRankWithScoresAsync(AllHostZsetKey(), order: Order.Ascending);
        return allHost.ToDictionary(x => x.Element.ToString(), x => x.Score.ToLocalDateTime());
    }

    public async Task<Dictionary<string, long>> GetCountByHostsAsync(IEnumerable<string> hosts)
    {
        var hostList = (hosts as IList<string> ?? hosts.ToList())
            .Distinct()
            .ToList();

        var batch = Database.CreateBatch();

        var tasks = hostList.ToDictionary(x => x, x => batch.SortedSetLengthAsync(HostConnZsetKey(x)));

        batch.Execute();

        await Task.WhenAll(tasks.Values);

        var result = tasks.ToDictionary(x => x.Key, x => x.Value.Result);

        return result;
    }

    public async Task<long> GetCountByHostAsync(string host, CancellationToken token = default)
    {
        return (await GetCountByHostsAsync([host])).Values.FirstOrDefault();
    }

    public async Task<IEnumerable<string>> GetConnectionsByHostAsync(string host, CancellationToken token = default)
    {
        var members = await Database.SortedSetRangeByRankAsync(HostConnZsetKey(host));
        return members.Select(x => x.ToString());
    }

    public async Task<long> GetOnlineFriendsCountAsync(long ownerId)
    {
        return await Database.SortedSetLengthAsync(FriendLatestZsetKey(ownerId));
    }

    public async Task<IEnumerable<SessionUnitElement>> GetOnlineFriendsAsync(
        long ownerId,
        double start = double.NegativeInfinity,
        double stop = double.PositiveInfinity,
        Exclude exclude = Exclude.None,
        Order order = Order.Ascending,
        long skip = 0,
        long take = -1,
        CommandFlags flags = CommandFlags.None)
    {
        var list = await Database.SortedSetRangeByScoreWithScoresAsync(FriendLatestZsetKey(ownerId), start, stop, exclude, order, skip, take, flags);

        return list.Select(x => SessionUnitElement.Parse(x.Element));

    }
}
