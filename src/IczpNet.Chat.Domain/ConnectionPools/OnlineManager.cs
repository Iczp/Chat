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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Json;

namespace IczpNet.Chat.ConnectionPools;

public class OnlineManager : RedisService, IOnlineManager//, IHostedService
{
    public IOptions<ConnectionOptions> ConnectionOptions => LazyServiceProvider.LazyGetRequiredService<IOptions<ConnectionOptions>>();
    public ConnectionOptions Config => ConnectionOptions.Value;
    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();
    public IConnectionStatManager ConnectionStatManager => LazyServiceProvider.LazyGetRequiredService<IConnectionStatManager>();
    protected override TimeSpan? CacheExpire => TimeSpan.FromSeconds(ConnectionOptions.Value.ConnectionCacheExpirationSeconds);
    protected virtual string Prefix => $"{Options.Value.KeyPrefix}{ConnectionOptions.Value.AllConnectionsCacheKey}:";

    /// <summary>
    /// connKey
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    private RedisKey ConnHashKey(string connectionId) => $"{Prefix}Conns:{connectionId}";

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
    private RedisKey SessionConnHashKey(Guid sessionId) => $"{Prefix}Sessions:{sessionId}";

    /// <summary>
    /// 设备
    /// key: connectionId,
    /// value: deviceType:eviceId,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey DeviceHashKey(long ownerId) => $"{Prefix}Devices:{ownerId}";

    /// <summary>
    /// Parse ownerId from DeviceHashKey
    /// </summary>
    /// <param name="deviceHashKey"></param>
    /// <returns>ownerId</returns>
    private static long? ParseDeviceHashKey(string deviceHashKey) => long.TryParse(deviceHashKey.Split(":").Last(), out var ownerId) ? ownerId : null;

    /// <summary>
    /// 最后连接时间
    /// element: deviceType:deviceId,
    /// score: unixTime,
    /// </summary>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private RedisKey LastOnlineZsetKey(long ownerId) => $"{Prefix}Lasts:{ownerId}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="firendOwnerId"></param>
    /// <returns></returns>
    private RedisKey FriendsConnsHashKey(long firendOwnerId) => $"{Prefix}Friends:{firendOwnerId}";

    /// <summary>
    /// 用户连接
    /// key: connectionId,
    /// value: ownerId[],
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private RedisKey UserConnKey(Guid userId) => $"{Prefix}Users:{userId}";

    /// <summary>
    /// App连接
    /// key: connectionId,
    /// value: appId,
    /// </summary>
    /// <param name="appId"></param>
    /// <returns></returns>
    private RedisKey AppSetKey(string appId) => $"{Prefix}Apps:{appId}";

    /// <summary>
    /// Client连接
    /// key: connectionId,
    /// value: clientId,
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    private RedisKey ClientSetKey(string clientId) => $"{Prefix}Clients:{clientId}";

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

    private void SortedSetAppConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
        => SortedSetIf(!string.IsNullOrWhiteSpace(connectionPool.AppId), () => AppSetKey(connectionPool.AppId), connectionPool.ConnectionId, Clock.Now.ToUnixTimeMilliseconds(), batch: batch);

    private void SortedSetClientConn(IBatch batch, ConnectionPoolCacheItem connectionPool)
        => SortedSetIf(!string.IsNullOrWhiteSpace(connectionPool.ClientId), () => ClientSetKey(connectionPool.ClientId), connectionPool.ConnectionId, Clock.Now.ToUnixTimeMilliseconds(), batch: batch);

    private void HashSetDevice(IBatch batch, ConnectionPoolCacheItem connectionPool)
    {
        var ownerIds = connectionPool.ChatObjectIdList ?? [];
        foreach (var ownerId in ownerIds)
        {
            HashSetIf(true, () => DeviceHashKey(ownerId), connectionPool.ConnectionId, $"{connectionPool.DeviceType}:{connectionPool.DeviceId}", batch: batch);
        }

    }

    private static void HashSetFriendsAction(List<long> ownerIds, Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap, Action<long, SessionUnitElement> eachAction)
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

    private void HashSetAndRefreshFriendsConns(IBatch batch, List<long> ownerIds, Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap, string connectionId)
    {
        if (!Config.IsEnableExclusiveStatFriends)
        {
            return;
        }
        HashSetFriendsAction(ownerIds, friendsMap, (ownerId, element) =>
        {
            //Friends Conns
            var friendConnsHashKey = FriendsConnsHashKey(element.DestinationId);
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                batch.HashSetAsync(friendConnsHashKey, connectionId, element);
            }
            Expire(batch, friendConnsHashKey);
        });
    }

    private void RemoveFriendsConn(IBatch batch, List<long> ownerIds, Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap, string connectionId)
    {
        if (!Config.IsEnableExclusiveStatFriends)
        {
            return;
        }
        HashSetFriendsAction(ownerIds, friendsMap, (ownerId, element) =>
        {
            //Friends Conns
            var friendConnsHashKey = FriendsConnsHashKey(element.DestinationId);
            batch.HashDeleteAsync(friendConnsHashKey, connectionId);
            Expire(batch, friendConnsHashKey);
        });
    }
    private void HashSetSessionConn(IBatch batch, ConnectionPoolCacheItem connectionPool, Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap)
    {
        var sessionOwnersMap = BuildSessionChatObjects(friendsMap);
        var connectionId = connectionPool.ConnectionId;
        foreach (var kv in sessionOwnersMap)
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
    private static Dictionary<Guid, List<long>> BuildSessionChatObjects(Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap)
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
    /// <param name="ownerIds"></param>
    /// <returns></returns>
    private Task<Dictionary<long, IEnumerable<SessionUnitElement>>> LoadFriendsMapAsync(List<long> ownerIds)
    {
        return SessionUnitManager.LoadFriendsMapAsync(ownerIds);
    }

    public async Task<KeyValuePair<string, ConnectionPoolCacheItem>[]> GetManyAsync(IEnumerable<string> ids)
    {
        if (ids == null) return [];

        var idList = ids.ToList();
        if (idList.Count == 0) return [];

        var batch = Database.CreateBatch();
        var tasks = new List<Task<HashEntry[]>>(idList.Count);

        // pipeline 全部 HashGetAll
        foreach (var unitId in idList)
        {
            tasks.Add(batch.HashGetAllAsync(ConnHashKey(unitId)));
        }

        // 一次提交（高性能关键点）
        batch.Execute();

        // 等待全部完成
        var resultEntries = await Task.WhenAll(tasks);

        // 构建结果
        var result = new KeyValuePair<string, ConnectionPoolCacheItem>[idList.Count];

        for (int i = 0; i < idList.Count; i++)
        {
            var entries = resultEntries[i];

            var item =
                (entries == null || entries.Length == 0)
                ? null
                : RedisMapper.ToObject<ConnectionPoolCacheItem>(entries);

            result[i] = new KeyValuePair<string, ConnectionPoolCacheItem>(idList[i], item);
        }

        return result;
    }

    //[UnitOfWork]
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

        var friendsMap = await LoadFriendsMapAsync(ownerIds);

        var batch = Database.CreateBatch();

        // Host: use sorted set for timestamped connections
        SortedSetHostConn(batch, connectionPool);

        //User
        HashSetUserConn(batch, connectionPool);
        // App
        SortedSetAppConn(batch, connectionPool);
        // Client
        SortedSetClientConn(batch, connectionPool);

        // connectionPool hash
        HashSetConn(batch, connectionPool);

        //Friends Conns
        HashSetAndRefreshFriendsConns(batch, ownerIds, friendsMap, connectionId);

        // chatObject -> connection hash (owner conn mapping)
        HashSetDevice(batch, connectionPool);

        // session -> connection hash (session -> connId : owners joined)
        HashSetSessionConn(batch, connectionPool, friendsMap);

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
        token.ThrowIfCancellationRequested();

        Logger.LogInformation($"[RefreshExpireAsync] connectionId={connectionId}");

        var stopwatch = Stopwatch.StartNew();

        var connectionPool = await GetAsync(connectionId, token);

        Logger.LogInformation("[RefreshExpireAsync] GetAsync connectionId={connectionId}, Elapsed: {Elapsed}ms", connectionId, stopwatch.ElapsedMilliseconds);

        if (connectionPool == null)
        {
            Logger.LogInformation("connectionPool is null,connectionId={connectionId}", connectionId);
            return null;
        }

        var ownerIds = connectionPool.ChatObjectIdList;

        var friendsMap = await LoadFriendsMapAsync(ownerIds);

        Logger.LogInformation("[RefreshExpireAsync] GetFriendsMapAsync ownerIds=[{ownerIds}], Elapsed: {Elapsed}ms", ownerIds.JoinAsString(","), stopwatch.ElapsedMilliseconds);

        var now = Clock.Now;

        var batch = Database.CreateBatch();

        var unixTime = now.ToUnixTimeMilliseconds();

        // Refresh expire on owner session sets, owner conn hashes, session conn hashes
        foreach (var ownerId in ownerIds)
        {
            //Expire(batch, OwnerSessionsHashKey(ownerId));
            Expire(batch, DeviceHashKey(ownerId));
            //latest
            _ = batch.SortedSetAddAsync(LastOnlineZsetKey(ownerId), DeviceElement.Create(connectionPool.DeviceType, connectionPool.DeviceId), unixTime);
        }

        // Host: update timestamp in sorted set
        SortedSetIf(!string.IsNullOrWhiteSpace(connectionPool.Host), () => HostConnZsetKey(connectionPool.Host), connectionId, unixTime, batch: batch);

        // User: update 
        ExpireIf(connectionPool.UserId.HasValue, () => UserConnKey(connectionPool.UserId.Value), batch: batch);

        // App
        ExpireIf(!string.IsNullOrWhiteSpace(connectionPool.AppId), () => AppSetKey(connectionPool.AppId), batch: batch);

        // Client
        ExpireIf(!string.IsNullOrWhiteSpace(connectionPool.ClientId), () => ClientSetKey(connectionPool.ClientId), batch: batch);

        // [key多:1] Friends 如果启用,这个Key较多
        HashSetAndRefreshFriendsConns(batch, ownerIds, friendsMap, null);

        // SessionConn
        RefreshSessionExpire(batch, friendsMap);

        // ActiveTime
        HashSetIf(true, () => ConnHashKey(connectionId), nameof(ConnectionPoolCacheItem.ActiveTime), now.ToRedisValue(), batch: batch);

        Expire(batch, AllHostZsetKey());

        batch.Execute();

        Logger.LogInformation("[RefreshExpireAsync]  batch.Execute, Elapsed: {Elapsed}ms", stopwatch.ElapsedMilliseconds);

        connectionPool.ActiveTime = now;

        return connectionPool;
    }

    private void RefreshSessionExpire(IBatch batch, Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap)
    {
        var sessionChatObjectMap = BuildSessionChatObjects(friendsMap);
        foreach (var kv in sessionChatObjectMap)
        {
            Expire(batch, SessionConnHashKey(kv.Key));
        }
        Logger.LogInformation("[RefreshSessionExpire] sessionChatObjectMap count: {count}", sessionChatObjectMap.Count);
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
        var connectionPool = await GetAsync(connectionId, token);

        if (connectionPool == null)
        {
            Logger.LogInformation("connectionPool is null,connectionId={connectionId}", connectionId);
            return;
        }

        var ownerIds = connectionPool.ChatObjectIdList ?? [];

        // Get sessions per owner (batched inside)
        var friendsMap = await LoadFriendsMapAsync(ownerIds);

        // OwnerDevice
        RemoveDevice(batch, connectionId, ownerIds);

        // Host
        SortedRemoveIf(!string.IsNullOrWhiteSpace(connectionPool.Host), () => HostConnZsetKey(connectionPool.Host), connectionId, refreshExpire: true, batch: batch);

        // User
        HashRemoveIf(connectionPool.UserId.HasValue, () => UserConnKey(connectionPool.UserId.Value), connectionId, refreshExpire: true, batch: batch);

        // AppId
        SortedRemoveIf(!string.IsNullOrEmpty(connectionPool.AppId), () => AppSetKey(connectionPool.AppId), connectionId, refreshExpire: true, batch: batch);

        // ClientId
        SortedRemoveIf(!string.IsNullOrEmpty(connectionPool.ClientId), () => ClientSetKey(connectionPool.ClientId), connectionId, refreshExpire: true, batch: batch);

        // Friends
        RemoveFriendsConn(batch, ownerIds, friendsMap, connectionId);

        // SessionConn
        RemoveSessionConn(batch, connectionId, friendsMap);

        // Delete conn hash
        _ = batch.KeyDeleteAsync(ConnHashKey(connectionId));
    }

    private void RemoveDevice(IBatch batch, string connectionId, List<long> ownerIds)
    {
        foreach (var ownerId in ownerIds)
        {
            HashRemoveIf(true, () => DeviceHashKey(ownerId), connectionId, refreshExpire: true, batch: batch);
        }
    }

    private void RemoveSessionConn(IBatch batch, string connectionId, Dictionary<long, IEnumerable<SessionUnitElement>> friendsMap)
    {
        var sessionChatObjects = BuildSessionChatObjects(friendsMap);
        foreach (var kv in sessionChatObjects)
        {
            HashRemoveIf(true, () => SessionConnHashKey(kv.Key), connectionId, refreshExpire: true, batch: batch);
        }
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
            SortedSetIf(true, () => AllHostZsetKey(), CurrentHosted.Name, Clock.Now.ToUnixTimeMilliseconds(), batch: batch);
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

    public async Task<ConnectionPoolCacheItem> GetAsync(string connectionId, CancellationToken token = default) => (await GetManyAsync([connectionId], token)).FirstOrDefault().Value;

    public Task<KeyValuePair<string, ConnectionPoolCacheItem>[]> GetManyAsync(IEnumerable<string> connectionIds, CancellationToken token = default) => GetManyHashSetAsync<string, ConnectionPoolCacheItem>(connectionIds, ConnHashKey, token);

    public async Task<bool> IsOnlineAsync(Guid userId, CancellationToken token = default)
    {
        var length = await Database.HashLengthAsync(UserConnKey(userId));
        return length > 0;
    }

    public async Task<bool> IsOnlineAsync(long ownertId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var length = await Database.HashLengthAsync(DeviceHashKey(ownertId));
        return length > 0;
    }
    public async Task<Dictionary<long, bool>> IsOnlineAsync(IEnumerable<long> ownerIds, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var ownerIdsList = ownerIds.Distinct().ToList();
        var keys = ownerIdsList.Select(DeviceHashKey);
        var onlineMap = await BatchKeyExistsAsync(keys);
        var onlineOwnerIds = onlineMap.Where(x => x.Value)
            .Select(x => ParseDeviceHashKey(x.Key))
            .Where(x => x.HasValue)
            .Select(x => x.Value);
        return ownerIdsList.ToDictionary(x => x, x => onlineOwnerIds.Contains(x));
    }

    public async Task<IEnumerable<LastOnline>> GetLastOnlineAsync(long ownertId, CancellationToken token = default)
    {
        var sortedSetEntries = await Database.SortedSetRangeByScoreWithScoresAsync(LastOnlineZsetKey(ownertId), order: Order.Descending);
        return sortedSetEntries.Select(x =>
        {
            var element = DeviceElement.Parse(x.Element);
            return new LastOnline()
            {
                OwnerId = ownertId,
                DeviceId = element.DeviceId,
                DeviceType = element.DeviceType,
                LastTime = x.Score.ToLocalDateTime(),
            };
        });
    }

    public async Task<IEnumerable<string>> GetDeviceTypesAsync(long ownertId, CancellationToken token = default)
    {
        var key = DeviceHashKey(ownertId);

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

    public async Task<Dictionary<long, IEnumerable<string>>> GetDeviceTypesAsync(List<long> ownerIds, CancellationToken token = default)
    {
        return (await GetDevicesAsync(ownerIds, token))
            .ToDictionary(
                x => x.Key,
                x => x.Value.Select(d => d.DeviceType)
             );
    }

    public async Task<Dictionary<long, IEnumerable<DeviceModel>>> GetDevicesAsync(List<long> ownerIds, CancellationToken token = default)
    {
        var result = new Dictionary<long, IEnumerable<DeviceModel>>(ownerIds.Count);

        if (ownerIds == null || ownerIds.Count == 0)
        {
            return result;
        }

        // 批量读取
        var batch = Database.CreateBatch();
        // ownerId -> HashGetAllAsync Task
        var taskMap = ownerIds.ToDictionary(x => x, x => batch.HashGetAllAsync(DeviceHashKey(x)));

        batch.Execute();

        await Task.WhenAll(taskMap.Values);

        // ownerId -> DeviceModel[]
        return taskMap
            .Where(x => x.Value.Result.Length > 0)
            .ToDictionary(x => x.Key, x => x.Value.Result.Select(v =>
            {
                var device = DeviceElement.Parse(v.Value);
                return new DeviceModel()
                {
                    OwnerId = x.Key,
                    ConnectionId = v.Name.ToString(),
                    DeviceType = device.DeviceType,
                    DeviceId = device.DeviceId,
                };
            }));
    }


    public async Task<Dictionary<long, IEnumerable<DeviceModel>>> GetDevicesByUserAsync(Guid userId, CancellationToken token = default)
    {
        var entries = await Database.HashGetAllAsync(UserConnKey(userId));

        var ownerIds = entries.SelectMany(x => x.Value.ToList<long>()).Distinct().ToList();

        return await GetDevicesAsync(ownerIds, token);
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

        var connMap = (await GetManyAsync(connIdList, token)).ToDictionary();

        return connIdMap.ToDictionary(x => x.Key, x => x.Value.Select(v => connMap[v]));
    }

    public async Task<IEnumerable<string>> GetConnectionIdsByUserAsync(Guid userId, CancellationToken token = default)
    {
        var values = await Database.HashKeysAsync(UserConnKey(userId));
        return values.Select(x => x.ToString());
    }

    public async Task<long> GetCountByUserAsync(Guid userId, CancellationToken token = default)
    {
        return await Database.HashLengthAsync(UserConnKey(userId));
    }

    public async Task<long> GetCountByOwnerAsync(long ownerId, CancellationToken token = default)
    {
        return await Database.HashLengthAsync(DeviceHashKey(ownerId));
    }

    public async Task<Dictionary<string, List<long>>> GetConnectionsBySessionAsync(Guid sessionId, CancellationToken token = default)
    {
        // Hash entry: field = connId, value = [1,2,3]
        var entries = await Database.HashGetAllAsync(SessionConnHashKey(sessionId));

        return entries.ToDictionary(x => x.Name.ToString(), x => x.Value.ToList<long>());
    }

    public async Task<long> GetCountBySessionAsync(Guid sessionId, CancellationToken token = default)
    {
        return await Database.HashLengthAsync(SessionConnHashKey(sessionId));
    }

    public async Task AddSessionAsync(List<(Guid SessionId, long OwnerId)> ownerSessions)
    {
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
        if (Config.IsEnableExclusiveStatFriends)
        {
            return await Database.HashLengthAsync(FriendsConnsHashKey(ownerId));
        }
        // 通过 ownerId 获取好友列表，检查每个好友的在线状态
        var friendsMap = await LoadFriendsMapAsync([ownerId]);
        var firendIds = friendsMap[ownerId].Select(x => x.DestinationId).ToHashSet();
        var keys = firendIds.Select(DeviceHashKey);
        var result = await BatchKeyExistsAsync(keys);
        return result.Where(x => x.Value).Count();
    }

    public async Task<IEnumerable<OnlineFriendInfo>> GetOnlineFriendsAsync(long ownerId)
    {
        if (Config.IsEnableExclusiveStatFriends)
        {
            var entries = await Database.HashGetAllAsync(FriendsConnsHashKey(ownerId));
            return entries.GroupBy(x => x.Value).Select(x =>
            {
                var element = SessionUnitElement.Parse(x.Key);
                return new OnlineFriendInfo()
                {
                    ConnectionId = x.Select(re => re.Name.ToString()).ToList(),
                    OwnerId = element.OwnerId,
                    DestinationId = element.DestinationId,
                    SessionId = element.SessionId,
                    SessionUnitId = element.SessionUnitId,
                };
            });
        }

        //通过 ownerId 获取好友列表，检查每个好友的在线状态
        var friendsMap = await LoadFriendsMapAsync([ownerId]);
        var friends = friendsMap[ownerId];
        var friendIds = friends.Select(x => x.DestinationId).ToList();
        var devicesMap = await GetDevicesAsync(friendIds);

        return devicesMap.Select(x =>
        {
            var element = friends.FirstOrDefault(f => f.DestinationId == x.Key);

            return new OnlineFriendInfo()
            {
                ConnectionId = devicesMap.GetValueOrDefault(x.Key)?.Select(v => v.ConnectionId).ToList() ?? [],
                OwnerId = element.OwnerId,
                DestinationId = element.DestinationId,
                SessionId = element.SessionId,
                SessionUnitId = element.SessionUnitId,
            };
        });
    }

    public async Task<Dictionary<long, IEnumerable<string>>> GetOnlineFriendsConnectionIdsAsync(List<long> ownerIds)
    {
        // ownerId -> friends
        var friendsMap = await LoadFriendsMapAsync(ownerIds);

        // 所有朋友 ownerId
        var friendIds = friendsMap
            .SelectMany(x => x.Value.Select(d => d.OwnerId))
            .Distinct()
            .ToList();

        if (friendIds.Count == 0)
        {
            return [];
        }

        return await GetConnectionIdsByOwnerAsync(friendIds);
    }
}
