using IczpNet.Chat.Connections;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.RedisMapping;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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
        => $"{Prefix}Items:ConnId-{connectionId}";
    // connKey 
    private string HostConnKey(string hostName)
        => $"{Prefix}Hosts:{hostName}";
    private string SessionConnKey(Guid sessionId)
       => $"{Prefix}Sessions:Conns:SessionId-{sessionId}";

    private string OwnerConnKey(long chatObjectId)
        => $"{Prefix}Owners:Conns:OwnerId-{chatObjectId}";
    private string OwnerSessionKey(long chatObjectId)
        => $"{Prefix}Owners:Sessions:OwnerId-{chatObjectId}";


    private static HashEntry[] MapToHashEntries(ConnectionPoolCacheItem connectionPool)
    {
        var entries = RedisMapper.ToHashEntries(connectionPool);

        var newList = entries.ToList();

        newList.Add(new HashEntry(nameof(connectionPool.ChatObjectIdList), connectionPool.ChatObjectIdList.JoinAsString(",")));

        return newList.ToArray();
    }


    private async Task<Dictionary<long, List<Guid>>> SetSessionsIfNotExistsAsync(IBatch batch, List<long> chatObjectIdList)
    {
        var unstoreIds = new List<long>();
        var storedIds = new List<long>();
        foreach (var chatObjectId in chatObjectIdList)
        {
            var ownerSessionKey = OwnerSessionKey(chatObjectId);
            var isExists = await Db.KeyExistsAsync(ownerSessionKey);
            if (!isExists)
            {
                unstoreIds.Add(chatObjectId);
            }
            else
            {
                storedIds.Add(chatObjectId);
            }
        }

        // Î´»º´æµÄ
        var sessionDict = await SessionUnitManager.GetSessionsByChatObjectAsync(unstoreIds);
        foreach (var chatObjectId in unstoreIds)
        {
            var ownerSessionKey = OwnerSessionKey(chatObjectId);
            var sessions = sessionDict.GetValueOrDefault(chatObjectId, []);
            if (sessions.IsNullOrEmpty())
            {
                continue;
            }
            foreach (var sessionId in sessions)
            {
                _ = batch.SetAddAsync(ownerSessionKey, sessionId.ToString());
            }
        }
        return sessionDict;
    }

    private async Task<Dictionary<long, List<Guid>>> GetSessionsAsync(List<long> chatObjectIdList)
    {

        var sessionDict = new Dictionary<long, List<Guid>>();
        foreach (var chatObjectId in chatObjectIdList)
        {
            var ownerSessionKey = OwnerSessionKey(chatObjectId);
            var sessionValues = await Db.SetMembersAsync(ownerSessionKey);
            var sessionIds = (sessionValues == null || sessionValues.Length == 0) ? [] : sessionValues.Select(x => Guid.Parse(x)).ToList();
            sessionDict[chatObjectId] = sessionIds;
        }
        return sessionDict;
    }

    private async Task<Dictionary<long, List<Guid>>> GetOrSetSessionsAsync(IBatch batch, List<long> chatObjectIdList)
    {
        var setDict = await SetSessionsIfNotExistsAsync(batch, chatObjectIdList);

        var keys = chatObjectIdList.Except(setDict.Keys).ToList();

        var getDict = await GetSessionsAsync(keys);

        var result = setDict.Concat(getDict).ToDictionary(k => k.Key, v => v.Value);

        return result;
    }

    [UnitOfWork]
    public async Task<bool> ConnectedAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        return await CreateAsync(connectionPool, token);
    }

    public async Task<bool> CreateAsync(ConnectionPoolCacheItem connectionPool, CancellationToken token = default)
    {
        var batch = Db.CreateBatch();

        var chatObjectIdList = connectionPool.ChatObjectIdList;

        var connectionId = connectionPool.ConnectionId;

        var chatObjectSessions = await GetOrSetSessionsAsync(batch, chatObjectIdList);

        var sessionChatObjects = chatObjectSessions
            .SelectMany(x => x.Value.Select(d => new { sessionId = d, ownerId = x.Key }))
            .GroupBy(x => x.sessionId, x => x.ownerId)
            .ToDictionary(g => g.Key, g => g.ToList())
            ;

        // Host
        var hostConnKey = HostConnKey(connectionPool.Host);

        _ = batch.HashSetAsync(hostConnKey, connectionId, Clock.Now.Ticks);

        // connectionPool
        var hashEntries = MapToHashEntries(connectionPool);
        _ = batch.HashSetAsync(ConnKey(connectionId), hashEntries);
        _ = batch.KeyExpireAsync(ConnKey(connectionId), CacheExpire);

        //owner session
        foreach (var chatObjectId in chatObjectIdList)
        {
            var ownerSessionKey = OwnerSessionKey(chatObjectId);
            var isExist = await Db.KeyExistsAsync(ownerSessionKey);
            if (!isExist)
            {
                var sessions = chatObjectSessions.GetValueOrDefault(chatObjectId, []);
                if (sessions.IsNullOrEmpty())
                {
                    continue;
                }
                foreach (var sessionId in sessions)
                {
                    _ = batch.SetAddAsync(ownerSessionKey, sessionId.ToString());
                }
                _ = batch.KeyExpireAsync(ownerSessionKey, CacheExpire);
            }
        }

        //chatObject
        foreach (var chatObjectId in chatObjectIdList)
        {
            var chatObjectConnKey = OwnerConnKey(chatObjectId);
            _ = batch.HashSetAsync(chatObjectConnKey, connectionId, connectionPool.DeviceId);
            _ = batch.KeyExpireAsync(chatObjectConnKey, CacheExpire);
        }

        //var connAllSessionsKey = ConnAllSessionsKey(connectionId);
        //_ = batch.KeyExpireAsync(connAllSessionsKey, CacheExpire);

        // sessionValues
        foreach (var item in sessionChatObjects)
        {
            var sessionId = item.Key;
            var chatObjectIds = item.Value;
            var sessionConnKey = SessionConnKey(sessionId);
            _ = batch.HashSetAsync(sessionConnKey, connectionId, chatObjectIds.JoinAsString(","));
            _ = batch.KeyExpireAsync(sessionConnKey, CacheExpire);
            //_ = batch.SetAddAsync(connAllSessionsKey, sessionId.ToString());
        }

        batch.Execute();

        return true;
    }

    public async Task<ConnectionPoolCacheItem> UpdateActiveTimeAsync(string connectionId, CancellationToken token = default)
    {
        var batch = Db.CreateBatch();

        var connKey = ConnKey(connectionId);

        var chatObjectIdListValue = await Db.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.ChatObjectIdList));

        var chatObjectIdList = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToString().Split(",").Select(x => long.Parse(x)).ToList();

        var chatObjectSessions = await GetOrSetSessionsAsync(batch, chatObjectIdList);

        var sessionChatObjects = chatObjectSessions
            .SelectMany(x => x.Value.Select(d => new { sessionId = d, ownerId = x.Key }))
            .GroupBy(x => x.sessionId, x => x.ownerId)
            .ToDictionary(g => g.Key, g => g.ToList())
            ;

        foreach (var chatObjectId in chatObjectIdList)
        {
            var ownerSessionKey = OwnerSessionKey(chatObjectId);
            var isExist = await Db.KeyExistsAsync(ownerSessionKey);
            if (isExist)
            {
                _ = batch.KeyExpireAsync(ownerSessionKey, CacheExpire);
            }
        }

        //chatObject
        foreach (var chatObjectId in chatObjectIdList)
        {
            var chatObjectConnKey = OwnerConnKey(chatObjectId);
            _ = batch.KeyExpireAsync(chatObjectConnKey, CacheExpire);
        }

        //var connAllSessionsKey = ConnAllSessionsKey(connectionId);
        //_ = batch.KeyExpireAsync(connAllSessionsKey, CacheExpire);
        // sessionValues
        foreach (var item in sessionChatObjects)
        {
            var sessionId = item.Key;
            var sessionConnKey = SessionConnKey(sessionId);
            _ = batch.KeyExpireAsync(sessionConnKey, CacheExpire);
        }

        // Host
        var hostHame = await Db.HashGetAsync(connKey, nameof(ConnectionPool.Host));
        var hostConnKey = HostConnKey(hostHame.ToString());
        _ = batch.HashSetAsync(hostConnKey, connectionId, Clock.Now.Ticks);

        // Delete ConnKey
        _ = batch.KeyExpireAsync(connKey, CacheExpire);

        batch.Execute();

        return default;
    }

    public async Task<bool> DisconnectedAsync(string connectionId, CancellationToken token = default)
    {
        var batch = Db.CreateBatch();
        await DeleteConnctionAsync(batch, connectionId, token);
        batch.Execute();
        return true;
    }

    public async Task DeleteConnctionAsync(IBatch batch, string connectionId, CancellationToken token = default)
    {
        var connKey = ConnKey(connectionId);

        //var entities = await Db.HashGetAllAsync(connKey);

        //var connectionPool = RedisMapper.ToObject<ConnectionPoolCacheItem>(entities);

        var chatObjectIdListValue = await Db.HashGetAsync(connKey, nameof(ConnectionPoolCacheItem.ChatObjectIdList));

        var chatObjectIdList = chatObjectIdListValue.IsNull ? [] : chatObjectIdListValue.ToString().Split(",").Select(x => long.Parse(x)).ToList();

        var chatObjectSessions = await GetOrSetSessionsAsync(batch, chatObjectIdList);

        var sessionChatObjects = chatObjectSessions
            .SelectMany(x => x.Value.Select(d => new { sessionId = d, ownerId = x.Key }))
            .GroupBy(x => x.sessionId, x => x.ownerId)
            .ToDictionary(g => g.Key, g => g.ToList())
            ;

        foreach (var chatObjectId in chatObjectIdList)
        {
            var ownerSessionKey = OwnerSessionKey(chatObjectId);
            var isExist = await Db.KeyExistsAsync(ownerSessionKey);
            if (!isExist)
            {
                var sessions = chatObjectSessions.GetValueOrDefault(chatObjectId, []);
                if (sessions.IsNullOrEmpty())
                {
                    continue;
                }
                //var batch1 = Db.CreateBatch();
                foreach (var sessionId in sessions)
                {
                    _ = batch.HashDeleteAsync(ownerSessionKey, sessionId.ToString());
                }
                _ = batch.KeyExpireAsync(ownerSessionKey, CacheExpire);
            }
        }

        //var hashEntries = MapToHashEntries(connectionPool);
        //_ = batch.HashSetAsync(ConnKey(connectionId), hashEntries);
        //_ = batch.KeyExpireAsync(ConnKey(connectionPool.ConnectionId), CacheExpire);
        //chatObject
        foreach (var chatObjectId in chatObjectIdList)
        {
            var chatObjectConnKey = OwnerConnKey(chatObjectId);
            _ = batch.HashDeleteAsync(chatObjectConnKey, connectionId);
            //_ = batch.KeyExpireAsync(chatObjectConnKey, CacheExpire);
        }

        //var connAllSessionsKey = ConnAllSessionsKey(connectionId);
        //_ = batch.KeyExpireAsync(connAllSessionsKey, CacheExpire);
        // sessionValues
        foreach (var item in sessionChatObjects)
        {
            var sessionId = item.Key;
            var chatObjectIds = item.Value;
            var sessionConnKey = SessionConnKey(sessionId);
            _ = batch.HashDeleteAsync(sessionConnKey, connectionId);
            //_ = batch.KeyExpireAsync(sessionConnKey, CacheExpire);
            //_ = batch.HashDeleteAsync(connAllSessionsKey, sessionId.ToString());
        }

        // Host
        var hostHame = await Db.HashGetAsync(connKey, nameof(ConnectionPool.Host));
        var hostConnKey = HostConnKey(hostHame.ToString());
        _ = batch.HashDeleteAsync(hostConnKey, connectionId);

        // Delete ConnKey
        _ = batch.KeyDeleteAsync(connKey);

        //batch.Execute();
    }


    public async Task DeleteByHostNameAsync(string hostHame)
    {
        var hostConnKey = HostConnKey(hostHame.ToString());

        var members = await Db.HashGetAllAsync(hostConnKey);

        var batch = Db.CreateBatch();

        foreach (var member in members)
        {
            var connectionId = member.Name;
            await DeleteConnctionAsync(batch, connectionId);
        }

        batch.Execute();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"[App Stop] DeleteByHostNameAsync,HostName:{CurrentHosted.Name}");
        await DeleteByHostNameAsync(CurrentHosted.Name);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"[App Start] DeleteByHostNameAsync,HostName:{CurrentHosted.Name}");
        await DeleteByHostNameAsync(CurrentHosted.Name);
    }
}
