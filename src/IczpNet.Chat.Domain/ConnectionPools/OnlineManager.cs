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

public class OnlineManager : RedisService, IOnlineManager//, IHostedService
{
    public IOptions<ConnectionOptions> ConnectionOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<ConnectionOptions>>();

    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();

    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();

    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();
    public IConnectionStatManager ConnectionStatManager => LazyServiceProvider.LazyGetRequiredService<IConnectionStatManager>();

    protected override TimeSpan? CacheExpire => TimeSpan.FromSeconds(ConnectionOptions.Value.ConnectionCacheExpirationSeconds);

    protected virtual int ExpireSeconds => (int)(CacheExpire?.TotalSeconds ?? -1);

    protected virtual string Prefix => $"{Options.Value.KeyPrefix}{ConnectionOptions.Value.AllConnectionsCacheKey}:";

    private static string LuaSAddIfExistsScript => @"
if redis.call('EXISTS', KEYS[1]) == 1 then
    redis.call('SADD', KEYS[1], ARGV[1])
    redis.call('EXPIRE', KEYS[1], ARGV[2])
end
return 1";

    /// <summary>
    /// connKey
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    private RedisKey ConnHashKey(string connectionId) => $"{Prefix}Conns:ConnId-{connectionId}";

    /// <summary>
    /// host -> sorted set of connection ids (score = ticks)
    /// </summary>
    /// <param name="hostName"></param>
    /// <returns></returns>
    private RedisKey HostConnZsetKey(string hostName) => $"{Prefix}Hosts:{hostName}";

    /// <summary>
    /// 会话连接
    /// key: connectionId,
    /// value: [ownerId],
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private RedisKey SessionConnHashKey(Guid sessionId) => $"{Prefix}Sessions:SessionId-{sessionId}";

    /// <summary>
    /// 设备
    /// key: connectionId,
    /// value: deviceType:eviceId,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerDeviceHashKey(long ownerId) => $"{Prefix}Owners:Devices:OwnerId-{ownerId}";

    /// <summary>
    /// 最后连接时间
    /// element: deviceType:deviceId,
    /// score: unixTime,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerLatestOnlineDeviceZsetKey(long ownerId) => $"{Prefix}Owners:LatestOnlineDevice:OwnerId-{ownerId}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="firendOwnerId"></param>
    /// <returns></returns>
    private RedisKey FriendsConnsHashKey(long firendOwnerId) => $"{Prefix}Friends:OwnerId-{firendOwnerId}";

    /// <summary>
    /// 会话
    /// key: connectionId,
    /// value: ,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey OwnerSessionsHashKey(long ownerId) => $"{Prefix}Owners:Sessions:OwnerId-{ownerId}";

    /// <summary>
    /// 用户连接
    /// key: connectionId,
    /// value: ownerId[],
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private RedisKey UserConnKey(Guid userId) => $"{Prefix}Users:userId-{userId}";

    /// <summary>
    /// 所有主机
    /// </summary>
    /// <returns></returns>
    private RedisKey AllHostZsetKey() => $"{Prefix}AllHosts";

    private void HashSetConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var connKey = ConnHashKey(connectionPool.ConnectionId);
        var hashEntries = MapToHashEntries(connectionPool);
        _ = batch.HashSetAsync(connKey, hashEntries);
        Expire(batch, connKey);
    }

    private void SortedSetHostConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
         => SortedSetIf(!string.IsNullOrWhiteSpace(connectionPool.Host), () => HostConnZsetKey(connectionPool.Host), connectionPool.ConnectionId, Clock.Now.ToUnixTimeMilliseconds(), batch: batch);

    private void HashSetUserConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
        => HashSetIf(connectionPool.UserId.HasValue, () => UserConnKey(connectionPool.UserId.Value), connectionPool.ConnectionId, JsonSerializer.Serialize(connectionPool.ChatObjectIdList ?? []), batch: batch);

    private void HashSetOwnerDevice(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            HashSetIf(true, () => OwnerDeviceHashKey(ownerId), connectionPool.ConnectionId, $"{connectionPool.DeviceType}:{connectionPool.DeviceId}", batch: batch);
        }

    }
    private void HashSetOwnerSessions(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>> friendsMap)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            var ownerSessionKey = OwnerSessionsHashKey(ownerId);
            // if chatObjectSessions contains it, we assume set was exists or just created. To avoid extra KeyExists roundtrip, only set expire
            // but add members if we have sessions and set wasn't in Redis earlier (GetOrSetSessionsAsync only returns existing/filled).
            var sessions = friendsMap.GetValueOrDefault(ownerId).Select(x => x.Key.SessionId);
            if (sessions == null || !sessions.Any()) continue;

            // Add members (idempotent)
            foreach (var sessionId in sessions)
            {
                _ = batch.SetAddAsync(ownerSessionKey, sessionId.ToString());
            }
            Expire(batch, ownerSessionKey);
        }
    }

    private static void SortedActionOwnerFriends(List<long> ownerIds, Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>> friendsMap, Action<long, KeyValuePair<SessionUnitElement, FriendScore>> eachAction)
    {
        foreach (var ownerId in ownerIds)
        {
            //friend latest
            var friends = friendsMap.GetValueOrDefault(ownerId);
            foreach (var item in friends)
            {
                eachAction(ownerId, item);
            }
        }
    }

    private void SortedSetOwnerFriendsConns(IBatch batch, List<long> ownerIds, Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>> friendsMap, string connectionId)
    {
        SortedActionOwnerFriends(ownerIds, friendsMap, (ownerId, friend) =>
        {
            var element = friend.Key;
            //Friends Conns
            var friendConnsHashKey = FriendsConnsHashKey(element.DestinationId);
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                batch.HashSetAsync(friendConnsHashKey, element, connectionId);
            }
            Expire(batch, friendConnsHashKey);
        });
    }

    private void SortedRemoveOwnerFriends(IBatch batch, List<long> ownerIds, Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>> friendsMap)
    {
        SortedActionOwnerFriends(ownerIds, friendsMap, (ownerId, friend) =>
        {
            var element = friend.Key;
            //Friends Conns
            var friendConnsHashKey = FriendsConnsHashKey(element.DestinationId);
            batch.HashDeleteAsync(friendConnsHashKey, element);
            Expire(batch, friendConnsHashKey);
        });
    }
    private void HashSetSessionConn(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<Guid, List<long>> sessionChatObjectsMap)
    {
        var connectionId = connectionPool.ConnectionId;
        foreach (var kv in sessionChatObjectsMap)
        {
            var sessionId = kv.Key;
            var ownerIds = kv.Value;
            var sessionConnKey = SessionConnHashKey(sessionId);
            var ownerIdsStr = ToJson(ownerIds);
            _ = batch.HashSetAsync(sessionConnKey, connectionId, ownerIdsStr);
            Expire(batch, sessionConnKey);
        }
    }

    private string ToJson(List<long> ownerIds)
    {
        return JsonSerializer.Serialize(ownerIds ?? []);
    }

    private static HashEntry[] MapToHashEntries(ConnectionPoolCacheItem connectionPool)
    {
        return RedisMapper.ToHashEntries(connectionPool);
    }

    /// <summary>
    /// Build dictionary: sessionId -> ownerSessions of ownerIds
    /// More efficient than repeated LINQ GroupBy
    /// </summary>
    private static Dictionary<Guid, List<long>> BuildSessionChatObjects(Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>> friendsMap)
    {
        var dict = new Dictionary<Guid, List<long>>(friendsMap.Count * 2);
        foreach (var kv in friendsMap)
        {
            var ownerId = kv.Key;
            var sessionList = kv.Value.Select(x => x.Key.SessionId);
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
    private async Task<Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>>> GetOrSetFriendsAsync(List<long> chatObjectIdList, CancellationToken token = default)
    {
        var result = new Dictionary<long, IEnumerable<KeyValuePair<SessionUnitElement, FriendScore>>>();
        foreach (var chatObjectId in chatObjectIdList)
        {
            await SessionUnitManager.LoadFriendsIfNotExistsAsync(chatObjectId);
            var friends = await SessionUnitCacheManager.GetRawFriendsAsync(chatObjectId);
            result.TryAdd(chatObjectId, friends);
        }
        return result;
    }

    [UnitOfWork]
    public async Task<bool> ConnectedAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        await MeasureAsync(nameof(CreateAsync), () => CreateAsync(connectionPool, token));

        await ConnectionStatManager.RecordConnectionAsync();

        var currentOnlineCount = await GetTotalCountAsync();

        await ConnectionStatManager.RecordOnlinePeakAsync(currentOnlineCount, "OnConnected");

        return true;
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

        //Friends Conns
        SortedSetOwnerFriendsConns(batch, ownerIds, friendsMap, connectionId);

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

        var redisValues = await Database.HashGetAsync(connKey, [
            nameof(ConnectionPoolCacheItem.ChatObjectIdList),
            nameof(ConnectionPool.Host),
            nameof(ConnectionPool.UserId),
            nameof(ConnectionPool.DeviceType),
            nameof(ConnectionPool.DeviceId),
            ]);
        var chatObjectIdListValue = redisValues[0];
        var hostValue = redisValues[1];
        var userValue = redisValues[2];
        var deviceType = redisValues[3];
        var deviceId = redisValues[4];

        var ownerIds = chatObjectIdListValue.ToList<long>();

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
            _ = batch.SortedSetAddAsync(OwnerLatestOnlineDeviceZsetKey(ownerId), DeviceElement.Create(deviceType, deviceId), unixTime);
        }

        // 
        SortedSetOwnerFriendsConns(batch, ownerIds, friendsMap, null);

        foreach (var kv in sessionChatObjects)
        {
            Expire(batch, SessionConnHashKey(kv.Key));
        }

        // Host: update timestamp in sorted set
        SortedSetIf(!hostValue.IsNullOrEmpty, () => HostConnZsetKey(hostValue.ToString()), connectionId, unixTime, batch: batch);

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

        var redisValues = await Database.HashGetAsync(connKey, [
            nameof(ConnectionPoolCacheItem.ChatObjectIdList),
            nameof(ConnectionPool.Host),
            nameof(ConnectionPool.UserId),
            ]);

        var chatObjectIdListValue = redisValues[0];
        var hostValue = redisValues[1];
        var userValue = redisValues[2];

        var chatObjectIdList = chatObjectIdListValue.ToList<long>();

        // Get sessions per owner (batched inside)
        var friendsMap = await GetOrSetFriendsAsync(chatObjectIdList, token);

        var sessionChatObjects = BuildSessionChatObjects(friendsMap);

        // Remove connection from owner -> conn hash, session -> conn hash, host sorted set
        // Use provided batch (caller may be a batch)

        // OwnerDevice
        foreach (var ownerId in chatObjectIdList)
        {
            HashRemoveIf(true, () => OwnerDeviceHashKey(ownerId), connectionId, refreshExpire: true, batch: batch);
        }

        // SessionConn
        foreach (var kv in sessionChatObjects)
        {
            HashRemoveIf(true, () => SessionConnHashKey(kv.Key), connectionId, refreshExpire: true, batch: batch);
        }

        // Host
        SortedRemoveIf(!hostValue.IsNullOrEmpty, () => HostConnZsetKey(hostValue.ToString()), connectionId, refreshExpire: true, batch: batch);

        //User
        
        if (!userValue.IsNullOrEmpty && Guid.TryParse(userValue.ToString(), out var userId))
        {
            HashRemoveIf(true, () => UserConnKey(userId), connectionId, refreshExpire: true, batch: batch);
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
        var connIds = (await GetConnectionIdsByHostAsync(hostHame)).ToList();

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
            HashSetIf(true, () => AllHostZsetKey(), CurrentHosted.Name, Clock.Now.ToUnixTimeMilliseconds(), expire: TimeSpan.FromDays(7), batch: batch);
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

    public async Task<Dictionary<string, ConnectionPoolCacheItem>> GetManyAsync(IEnumerable<string> connectionIds, CancellationToken token = default)
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
        var key = OwnerLatestOnlineDeviceZsetKey(ownertId);
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

    public async Task<IEnumerable<string>> GetDeviceTypesAsync(long ownertId, CancellationToken token = default)
    {
        var key = OwnerDeviceHashKey(ownertId);

        var values = await Database.HashValuesAsync(key);
        if (values == null || values.Length == 0)
        {
            return [];
        }

        // DeviceType:DeviceId
        return values
            .Select(DeviceElement.Parse)
            .Select(x => x.DeviceType)
            .Distinct();
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
                    var value = DeviceElement.Parse(v.Value);
                    return new DeviceModel()
                    {
                        OwnerId = ownerId,
                        ConnectionId = v.Name.ToString(),
                        DeviceType = value.DeviceType,
                        DeviceId = value.DeviceId,
                    };
                })
                .Distinct()
                .ToList();

            result[ownerId] = types;
        }

        return result;
    }


    public async Task<IEnumerable<DeviceElement>> GetDeviceTypesAsync(Guid userId, CancellationToken token = default)
    {
        var userKey = UserConnKey(userId);
        var hash = await Database.HashGetAllAsync(userKey);

        if (hash == null || hash.Length == 0)
        {
            return [];
        }

        var deviceTypes = new HashSet<DeviceElement>();

        // Hash: connectionId -> ownerIdList "12,45,66"
        foreach (var entry in hash)
        {
            foreach (var ownerId in entry.Value.ToList<long>())
            {
                var ownerValues = await Database.HashValuesAsync(OwnerDeviceHashKey(ownerId));
                foreach (var v in ownerValues)
                {
                    deviceTypes.Add(DeviceElement.Parse(v));
                }
            }
        }

        return deviceTypes;
    }

    public async Task<Dictionary<long, IEnumerable<string>>> GetConnectionIdsByOwnerAsync(List<long> ownerIds, CancellationToken token = default)
    {
        return (await GetDevicesAsync(ownerIds, token))
           .ToDictionary(
               x => x.Key,
               x => x.Value
                   .Select(d => d.ConnectionId)
            );
    }

    public async Task<Dictionary<long, IEnumerable<ConnectionPoolCacheItem>>> GetConnectionsByOwnerAsync(List<long> ownerIds, CancellationToken token = default)
    {
        var connIdMap = await GetConnectionIdsByOwnerAsync(ownerIds, token);

        var connIdList = connIdMap.SelectMany(x => x.Value).Distinct().ToList();

        var connMap = await GetManyAsync(connIdList, token);

        return connIdMap.ToDictionary(x => x.Key, x => x.Value.Select(v => connMap[v]));
    }

    public async Task<IEnumerable<string>> GetConnectionIdsByUserAsync(Guid userId, CancellationToken token = default)
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
            var ownerList = entry.Value.ToList<long>();  // chatObjectIdList 已经是逗号分隔
            result[connId] = ownerList;
        }

        return result;
    }

    public async Task<long> GetCountBySessionAsync(Guid sessionId, CancellationToken token = default)
    {
        return await Database.HashLengthAsync(SessionConnHashKey(sessionId));
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
                var ownerIdStr = ToJson(relatedOwnerIds);
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

                    _ = batch.HashSetAsync(sessionConnKey, connectionId, ToJson([ownerId]));

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

    public async Task<long> GetTotalCountAsync()
    {
        return (await GetCountByHostsAsync()).Sum(x => x.Value);
    }

    public async Task<Dictionary<string, long>> GetCountByHostsAsync(IEnumerable<string> hosts = null)
    {
        var hostList = hosts?.Distinct() ?? (await GetAllHostsAsync()).Select(x => x.Key);

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

    public async Task<IEnumerable<string>> GetConnectionIdsByHostAsync(string host, CancellationToken token = default)
    {
        var members = await Database.SortedSetRangeByRankAsync(HostConnZsetKey(host));
        return members.Select(x => x.ToString());
    }

    public async Task<long> GetOnlineFriendsCountAsync(long ownerId)
    {
        //var friendsMap = await GetOrSetFriendsAsync([ownerId]);
        //var firendIds = friendsMap[ownerId].Select(x => x.Key.DestinationId).ToHashSet();
        //var keys = firendIds.Select(OwnerDeviceHashKey);
        //var result = await BatchKeyExistsAsync(keys);
        //return result.Where(x => x.Value).Count();

        //return await Database.SortedSetLengthAsync(FriendsLatestOnlineZsetKey(ownerId));
        return await Database.HashLengthAsync(FriendsConnsHashKey(ownerId));
    }

    public async Task<IEnumerable<OnlineFriendInfo>> GetOnlineFriendsAsync(long ownerId)
    {
        var entries = await Database.HashGetAllAsync(FriendsConnsHashKey(ownerId));
        return entries.Select(x =>
        {
            var element = SessionUnitElement.Parse(x.Name);
            return new OnlineFriendInfo()
            {
                ConnectionId = x.Value.ToString(),
                OwnerId = element.OwnerId,
                DestinationId = element.DestinationId,
                SessionId = element.SessionId,
                SessionUnitId = element.SessionUnitId,
            };
        });
    }

    public async Task<IEnumerable<string>> GetOnlineFriendsConnectionIdsAsync(long ownerId)
    {
        var friendsConnsHashKey = FriendsConnsHashKey(ownerId);

        var values = await Database.HashValuesAsync(friendsConnsHashKey);

        return values.Select(x => x.ToString()).Distinct();
    }
}
