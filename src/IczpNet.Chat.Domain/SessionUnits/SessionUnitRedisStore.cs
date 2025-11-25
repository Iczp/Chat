
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitRedisStore(
    ISessionUnitManager sessionUnitManager,
    IOptions<AbpDistributedCacheOptions> options,
    IConnectionMultiplexer connection) : DomainService, ISessionUnitRedisStore
{
    protected readonly IDatabase Database = connection.GetDatabase();

    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public IOptions<AbpDistributedCacheOptions> Options { get; } = options;

    private readonly TimeSpan _cacheExpire = TimeSpan.FromDays(7);




    private string UnitKey(Guid sessionId, Guid unitId) => $"{Options.Value.KeyPrefix}Session:{sessionId}:Unit:{unitId}";

    private string SessionUnitIdSetKey(Guid sessionId) => $"{Options.Value.KeyPrefix}Session:UnitIds:{sessionId}";

    private string LastMessageSetKey(Guid sessionId) => $"{Options.Value.KeyPrefix}Session:{sessionId}:LastMessageId";



    #region Helpers for field names

    private static readonly string F_Id = nameof(SessionUnitCacheItem.Id);
    private static readonly string F_SessionId = nameof(SessionUnitCacheItem.SessionId);
    private static readonly string F_OwnerId = nameof(SessionUnitCacheItem.OwnerId);
    private static readonly string F_OwnerObjectType = nameof(SessionUnitCacheItem.OwnerObjectType);
    private static readonly string F_DestinationId = nameof(SessionUnitCacheItem.DestinationId);
    private static readonly string F_DestinationObjectType = nameof(SessionUnitCacheItem.DestinationObjectType);
    private static readonly string F_IsStatic = nameof(SessionUnitCacheItem.IsStatic);
    private static readonly string F_IsPublic = nameof(SessionUnitCacheItem.IsPublic);
    private static readonly string F_IsVisible = nameof(SessionUnitCacheItem.IsVisible);
    private static readonly string F_IsEnabled = nameof(SessionUnitCacheItem.IsEnabled);
    private static readonly string F_ReadedMessageId = nameof(SessionUnitCacheItem.ReadedMessageId);
    private static readonly string F_LastMessageId = nameof(SessionUnitCacheItem.LastMessageId);
    private static readonly string F_PublicBadge = nameof(SessionUnitCacheItem.PublicBadge);
    private static readonly string F_PrivateBadge = nameof(SessionUnitCacheItem.PrivateBadge);
    private static readonly string F_RemindAllCount = nameof(SessionUnitCacheItem.RemindAllCount);
    private static readonly string F_RemindMeCount = nameof(SessionUnitCacheItem.RemindMeCount);
    private static readonly string F_FollowingCount = nameof(SessionUnitCacheItem.FollowingCount);
    private static readonly string F_Ticks = nameof(SessionUnitCacheItem.Ticks);

    #endregion

    #region Initialization (from DB -> Redis)

    /// <summary>
    /// Initialize a session's Redis cache from the provided sessionUnit list (from DB).
    /// This method will populate:
    ///  - Session:UnitIds:{sessionId} (SET of unit ids)
    ///  - Session:Unit:{sessionId}:{unitId} (HASH per unit)
    ///  - Session:LastMessageId:{sessionId} (ZSET members for lastMessageId)
    /// TTL will be applied from configuration (default 7 days).
    /// </summary>
    public async Task InitializeSessionCacheAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);

        var unitList = units.ToList();
        if (unitList.Count == 0) return;

        var sessionUnitIdSetKey = SessionUnitIdSetKey(sessionId);
        var lastMsgKey = LastMessageSetKey(sessionId);

        var batch = Database.CreateBatch();

        // delete existing set to avoid stale members
        _ = batch.KeyDeleteAsync(sessionUnitIdSetKey);

        var hashTasks = new List<Task>();
        var setAddTasks = new List<Task<bool>>();
        var zsetAddTasks = new List<Task<bool>>();

        foreach (var u in unitList)
        {
            var idStr = u.Id.ToString();
            setAddTasks.Add(batch.SetAddAsync(sessionUnitIdSetKey, idStr));

            var hashKey = UnitKey(sessionId, u.Id);

            var entries = new HashEntry[]
            {
                new HashEntry(F_Id, u.Id.ToString()),
                new HashEntry(F_SessionId, u.SessionId.HasValue ? u.SessionId.ToString() : RedisValue.Null),
                new HashEntry(F_OwnerId, u.OwnerId),
                new HashEntry(F_OwnerObjectType, u.OwnerObjectType?.ToString() ?? RedisValue.Null),
                new HashEntry(F_DestinationId, u.DestinationId.HasValue ? u.DestinationId.ToString() : RedisValue.Null),
                new HashEntry(F_DestinationObjectType, u.DestinationObjectType?.ToString() ?? RedisValue.Null),
                new HashEntry(F_IsStatic, u.IsStatic ? 1 : 0),
                new HashEntry(F_IsPublic, u.IsPublic ? 1 : 0),
                new HashEntry(F_IsVisible, u.IsVisible ? 1 : 0),
                new HashEntry(F_IsEnabled, u.IsEnabled ? 1 : 0),
                new HashEntry(F_ReadedMessageId, u.ReadedMessageId ?? RedisValue.Null),
                new HashEntry(F_LastMessageId, u.LastMessageId ?? RedisValue.Null),
                new HashEntry(F_PublicBadge, u.PublicBadge),
                new HashEntry(F_PrivateBadge, u.PrivateBadge),
                new HashEntry(F_RemindAllCount, u.RemindAllCount),
                new HashEntry(F_RemindMeCount, u.RemindMeCount),
                new HashEntry(F_FollowingCount, u.FollowingCount),
                new HashEntry(F_Ticks, u.Ticks),
            };

            // 去除 Null 值，避免 Redis 存储 Null 导致的问题
            var safeEntries = entries.Select(x => new HashEntry(x.Name, x.Value.IsNull ? RedisValue.EmptyString : x.Value)).ToArray();

            hashTasks.Add(batch.HashSetAsync(hashKey, safeEntries));

            // zset for last message ordering; if LastMessageId null, skip adding
            if (u.LastMessageId.HasValue)
            {
                zsetAddTasks.Add(batch.SortedSetAddAsync(lastMsgKey, idStr, u.LastMessageId.Value));
            }

            // set expire on each unit hash
            _ = batch.KeyExpireAsync(hashKey, _cacheExpire);
        }

        // set expire on set and sorted set
        _ = batch.KeyExpireAsync(sessionUnitIdSetKey, _cacheExpire);
        _ = batch.KeyExpireAsync(lastMsgKey, _cacheExpire);

        batch.Execute();

        if (setAddTasks.Count > 0) await Task.WhenAll(setAddTasks);
        if (hashTasks.Count > 0) await Task.WhenAll(hashTasks);
        if (zsetAddTasks.Count > 0) await Task.WhenAll(zsetAddTasks);
    }

    /// <summary>
    /// Convenience overload: fetch units from a supplier function (e.g. DB call) and initialize
    /// </summary>
    public async Task InitializeSessionCacheAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchFromDb)
    {

        ArgumentNullException.ThrowIfNull(fetchFromDb);
        var units = (await fetchFromDb(sessionId))?.ToList() ?? [];
        await InitializeSessionCacheAsync(sessionId, units);

    }

    #endregion

    #region Ensure initialization helpers

    /// <summary>
    /// Ensure the session unit set exists; if not, call the provided initializer to load from DB.
    /// </summary>
    public async Task EnsureSessionInitializedAsync(Guid sessionId, Message message)
    {
        var sessionUnitIdSetKey = SessionUnitIdSetKey(sessionId);

        if (!await Database.KeyExistsAsync(sessionUnitIdSetKey))
        {
            await InitializeSessionCacheAsync(sessionId, async (sessionId) => await SessionUnitManager.GetCacheListAsync(message));
        }
    }

    #endregion

    #region Get / GetMany / GetListBySessionId

    /// <summary>
    /// Batch read many unit hashes and map to SessionUnitCacheItem
    /// </summary>
    public async Task<IDictionary<Guid, SessionUnitCacheItem>> GetManyAsync(Guid sessionId, List<Guid> unitIds)
    {
        var batch = Database.CreateBatch();
        var tasks = new Task<HashEntry[]>[unitIds.Count];

        for (int i = 0; i < unitIds.Count; i++)
        {
            tasks[i] = batch.HashGetAllAsync(UnitKey(sessionId, unitIds[i]));
        }

        batch.Execute();
        await Task.WhenAll(tasks);

        var dict = new Dictionary<Guid, SessionUnitCacheItem>(unitIds.Count);
        for (int i = 0; i < unitIds.Count; i++)
        {
            var entries = tasks[i].Result;
            dict[unitIds[i]] = MapHashEntriesToCacheItem(entries);
        }

        return dict;
    }

    /// <summary>
    /// Get list of all session units under a sessionId (reads the Session:UnitIds:{sessionId} set)
    /// and returns full mapped SessionUnitCacheItem objects.
    /// </summary>
    public async Task<IList<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId)
    {
        var setKey = SessionUnitIdSetKey(sessionId);
        var members = await Database.SetMembersAsync(setKey);
        var idList = members.Select(x => Guid.Parse(x.ToString())).ToList();
        if (idList.Count == 0) return [];

        var dict = await GetManyAsync(sessionId, idList);
        return dict.Values.ToList();
    }

    private static SessionUnitCacheItem MapHashEntriesToCacheItem(HashEntry[] entries)
    {
        var item = new SessionUnitCacheItem();

        if (entries == null || entries.Length == 0) return item;

        foreach (var e in entries)
        {
            var name = (string)e.Name;
            if (string.IsNullOrEmpty(name)) continue;

            if (name == F_Id && e.Value.HasValue)
            {
                if (Guid.TryParse(e.Value.ToString(), out var gid))
                {
                    item.Id = gid;
                }
                else
                {
                    item.Id = Guid.Empty;
                }
            }
            else if (name == F_SessionId && e.Value.HasValue) item.SessionId = Guid.TryParse(e.Value.ToString(), out var ss) ? ss : (Guid?)null;
            else if (name == F_OwnerId && e.Value.HasValue) item.OwnerId = (long)e.Value;
            else if (name == F_OwnerObjectType && e.Value.HasValue)
            {
                if (Enum.TryParse(typeof(ChatObjectTypeEnums), e.Value.ToString(), out var o)) item.OwnerObjectType = (ChatObjectTypeEnums?)o;
            }
            else if (name == F_DestinationId && e.Value.HasValue) item.DestinationId = long.TryParse(e.Value.ToString(), out var dd) ? (long?)dd : (long?)null;
            else if (name == F_DestinationObjectType && e.Value.HasValue)
            {
                if (Enum.TryParse(typeof(ChatObjectTypeEnums), e.Value.ToString(), out var d)) item.DestinationObjectType = (ChatObjectTypeEnums?)d;
            }
            else if (name == F_IsStatic && e.Value.HasValue) item.IsStatic = (int)e.Value == 1;
            else if (name == F_IsPublic && e.Value.HasValue) item.IsPublic = (int)e.Value == 1;
            else if (name == F_IsVisible && e.Value.HasValue) item.IsVisible = (int)e.Value == 1;
            else if (name == F_IsEnabled && e.Value.HasValue) item.IsEnabled = (int)e.Value == 1;
            else if (name == F_ReadedMessageId && e.Value.HasValue) item.ReadedMessageId = (long)e.Value;
            else if (name == F_LastMessageId && e.Value.HasValue) item.LastMessageId = (long)e.Value;
            else if (name == F_PublicBadge && e.Value.HasValue) item.PublicBadge = (int)e.Value;
            else if (name == F_PrivateBadge && e.Value.HasValue) item.PrivateBadge = (int)e.Value;
            else if (name == F_RemindAllCount && e.Value.HasValue) item.RemindAllCount = (int)e.Value;
            else if (name == F_RemindMeCount && e.Value.HasValue) item.RemindMeCount = (int)e.Value;
            else if (name == F_FollowingCount && e.Value.HasValue) item.FollowingCount = (int)e.Value;
            else if (name == F_Ticks && e.Value.HasValue) item.Ticks = (double)e.Value;
        }

        return item;
    }

    #endregion

    #region BatchIncrementBadgeAndSetLastMessageAsync

    /// <summary>
    /// Update badges / counts and last message id for all session units related to a message.
    /// Rules:
    /// - If message.IsPrivateMessage() => PrivateBadge += 1 for non-senders
    /// - Else => PublicBadge += 1 for non-senders
    /// - If message.IsRemindAll() => RemindAllCount += 1 for non-senders
    /// - For remember lists (remindMeList, followingIncrementList) increase respective counters
    /// - Always update LastMessageId in hash + sorted set
    /// - Update Ticks to current UTC ticks
    /// - Do NOT modify ReadedMessageId here
    /// - Optionally set TTL for keys
    /// </summary>
    public async Task BatchIncrementBadgeAndSetLastMessageAsync(
        Message message,
        //IEnumerable<Guid>? followingIncrementList = null,
        //IEnumerable<Guid>? remindMeList = null,
        TimeSpan? expire = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var followingIncrementList = new List<Guid>();
        var remindMeList = new List<Guid>();

        var sessionId = message.SessionId!.Value;
        var lastMessageId = message.Id;
        var lastMsgKey = LastMessageSetKey(sessionId);
        var isPrivate = message.IsPrivateMessage();
        var isRemindAll = message.IsRemindAll;

        // load session units - use manager to get cached snapshot if possible
        var sessionUnitList = await SessionUnitManager.GetCacheListAsync(message);
        if (sessionUnitList == null || sessionUnitList.Count == 0) return;

        var followingSet = followingIncrementList != null ? new HashSet<Guid>(followingIncrementList) : null;
        var remindMeSet = remindMeList != null ? new HashSet<Guid>(remindMeList) : null;

        var batch = Database.CreateBatch();

        var hashSetTasks = new List<Task<bool>>();
        var hashIncTasks = new List<Task<long>>();
        var zsetTasks = new List<Task<bool>>();
        var expireTasks = new List<Task<bool>>();

        foreach (var unit in sessionUnitList)
        {
            var id = unit.Id;
            var key = UnitKey(sessionId, id);
            var idStr = id.ToString();
            var isSender = id == message.SenderSessionUnitId;

            // increase badges according to rules (only non-senders)
            if (!isSender)
            {
                if (isPrivate)
                {
                    hashIncTasks.Add(batch.HashIncrementAsync(key, F_PrivateBadge, 1));
                }
                else
                {
                    hashIncTasks.Add(batch.HashIncrementAsync(key, F_PublicBadge, 1));
                }

                if (isRemindAll)
                {
                    hashIncTasks.Add(batch.HashIncrementAsync(key, F_RemindAllCount, 1));
                }

                if (remindMeSet != null && remindMeSet.Contains(id))
                {
                    hashIncTasks.Add(batch.HashIncrementAsync(key, F_RemindMeCount, 1));
                }
            }

            // Following count (can include sender too depending on business logic)
            if (followingSet != null && followingSet.Contains(id))
            {
                hashIncTasks.Add(batch.HashIncrementAsync(key, F_FollowingCount, 1));
            }

            // update LastMessageId field in hash
            hashSetTasks.Add(batch.HashSetAsync(key, F_LastMessageId, lastMessageId));

            // update SortedSet score (last message id)
            zsetTasks.Add(batch.SortedSetAddAsync(lastMsgKey, idStr, lastMessageId));

            // update ticks
            hashSetTasks.Add(batch.HashSetAsync(key, F_Ticks, (double)DateTime.UtcNow.Ticks));

            // schedule expire per unit
            expireTasks.Add(batch.KeyExpireAsync(key, expire ?? _cacheExpire));
        }

        // ensure zset has expire set too
        expireTasks.Add(batch.KeyExpireAsync(lastMsgKey, expire ?? _cacheExpire));

        batch.Execute();

        if (hashSetTasks.Count > 0) await Task.WhenAll(hashSetTasks);
        if (hashIncTasks.Count > 0) await Task.WhenAll(hashIncTasks);
        if (zsetTasks.Count > 0) await Task.WhenAll(zsetTasks);
        if (expireTasks.Count > 0) await Task.WhenAll(expireTasks);
    }

    #endregion

    #region Remove / Set / Misc operations

    public async Task AddUnitIdToSessionAsync(Guid sessionId, Guid unitId)
    {
        await Database.SetAddAsync(SessionUnitIdSetKey(sessionId), unitId.ToString());
        // set expire on id set
        await Database.KeyExpireAsync(SessionUnitIdSetKey(sessionId), _cacheExpire);
    }

    public async Task RemoveUnitIdFromSessionAsync(Guid sessionId, Guid unitId)
    {
        await Database.SetRemoveAsync(SessionUnitIdSetKey(sessionId), unitId.ToString());
        // optionally delete the unit hash as well
        await Database.KeyDeleteAsync(UnitKey(sessionId, unitId));
    }

    public async Task<bool> RemoveManyAsync(Guid sessionId, IEnumerable<Guid> unitIds)
    {
        var batch = Database.CreateBatch();
        var zkey = LastMessageSetKey(sessionId);

        var delTasks = new List<Task<bool>>();
        var zremoveTasks = new List<Task<bool>>();

        foreach (var id in unitIds)
        {
            delTasks.Add(batch.KeyDeleteAsync(UnitKey(sessionId, id)));
            zremoveTasks.Add(batch.SortedSetRemoveAsync(zkey, id.ToString()));
            // also remove from set
            _ = batch.SetRemoveAsync(SessionUnitIdSetKey(sessionId), id.ToString());
        }

        batch.Execute();

        if (delTasks.Count > 0) await Task.WhenAll(delTasks);
        if (zremoveTasks.Count > 0) await Task.WhenAll(zremoveTasks);

        return true;
    }

    public async Task<bool> RemoveAsync(Guid sessionId, Guid unitId)
    {
        return await RemoveManyAsync(sessionId, new[] { unitId });
    }

    public async Task<bool> RemoveAllAsync(Guid sessionId)
    {
        var setKey = SessionUnitIdSetKey(sessionId);
        var lastKey = LastMessageSetKey(sessionId);

        // delete zset and set; unit hashes should be deleted by caller if needed (or can be iterated)
        var r1 = await Database.KeyDeleteAsync(setKey);
        var r2 = await Database.KeyDeleteAsync(lastKey);
        return r1 && r2;
    }

    /// <summary>
    /// Update ReadedMessageId for a single unit (explicitly called by business logic)
    /// </summary>
    public async Task UpdateReadedMessageIdAsync(Guid sessionId, Guid unitId, long readedMessageId, TimeSpan? expire = null)
    {
        var key = UnitKey(sessionId, unitId);
        await Database.HashSetAsync(key, F_ReadedMessageId, readedMessageId);
        if (expire.HasValue) await Database.KeyExpireAsync(key, expire.Value);
    }

    /// <summary>
    /// Set TTL for a specific unit (and optionally the session sorted set)
    /// </summary>
    public async Task<bool> SetExpireAsync(Guid sessionId, Guid? unitId = null, TimeSpan? expire = null)
    {
        var e = expire ?? _cacheExpire;
        var tasks = new List<Task<bool>>();
        if (unitId.HasValue)
        {
            tasks.Add(Database.KeyExpireAsync(UnitKey(sessionId, unitId.Value), e));
        }
        else
        {
            // set expire for unit id set
            tasks.Add(Database.KeyExpireAsync(SessionUnitIdSetKey(sessionId), e));
            // set expire for sorted set
            tasks.Add(Database.KeyExpireAsync(LastMessageSetKey(sessionId), e));
        }

        var results = await Task.WhenAll(tasks);
        return results.All(x => x);
    }

    #endregion
}

//// Minimal ISessionUnit interface placeholder in case it is not in the compilation unit.
//// If your project already defines ISessionUnit elsewhere, remove or adjust this definition.
//public interface ISessionUnit
//{
//    Guid Id { get; set; }
//}

//// Placeholder enum in case ChatObjectTypeEnums is not defined in the compilation unit.
//// Remove or adjust as necessary if already defined in your project.
//public enum ChatObjectTypeEnums
//{
//    Unknown = 0
//}
