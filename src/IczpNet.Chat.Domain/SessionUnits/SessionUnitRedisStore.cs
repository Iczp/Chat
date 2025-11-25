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

    private readonly TimeSpan? _cacheExpire = TimeSpan.FromDays(7);

    // key builders
    private string UnitKey(Guid sessionId, Guid unitId) => $"{Options.Value.KeyPrefix}SessionUnits:Units:SessionId-{sessionId}:UnitId-{unitId}";
    private string SessionUnitIdSetKey(Guid sessionId) => $"{Options.Value.KeyPrefix}SessionUnits:Ids:SessionId-{sessionId}";
    private string LastMessageSetKey(Guid sessionId) => $"{Options.Value.KeyPrefix}SessionUnits:LastMessages:SessionId-{sessionId}";
    private string OwnerSetKey(long ownerId) => $"{Options.Value.KeyPrefix}SessionUnits:Owners:OwnerId-{ownerId}";

    // composite score multiplier (sorting * MULT + lastMessageId)
    private const double OWNER_SCORE_MULT = 1_000_000_000_000d; // 1e12

    #region Field names
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
    private static readonly string F_Sorting = nameof(SessionUnitCacheItem.Sorting);
    #endregion

    #region Safe helpers (avoid null values)
    private static RedisValue Safe(object? value)
    {
        if (value == null)
            return RedisValue.EmptyString;

        // handle Nullable<T> (long?, int?, double?, Guid?, bool?, etc)
        var type = value.GetType();
        if (Nullable.GetUnderlyingType(type) != null)
        {
            // extract actual value in nullable
            value = Convert.ChangeType(value, Nullable.GetUnderlyingType(type)!);
            if (value == null)
                return RedisValue.EmptyString;
            type = value.GetType();
        }

        return value switch
        {
            string s => s,
            Guid g => g.ToString(),
            bool b => b ? "1" : "0",
            int i => i,
            long l => l,
            double d => d,
            float f => (double)f,
            DateTime dt => new DateTimeOffset(dt).ToUnixTimeMilliseconds(),
            DateTimeOffset dto => dto.ToUnixTimeMilliseconds(),
            _ => value.ToString() ?? RedisValue.EmptyString
        };
    }

    private static long TryGetLong(HashEntry[] entries, string field, long defaultValue)
    {
        var e = Array.Find(entries, x => (string)x.Name == field);
        if (e.Equals(default) || e.Value.IsNull) return defaultValue;
        if (long.TryParse(e.Value.ToString(), out var v)) return v;
        try { return (long)e.Value; } catch { return defaultValue; }
    }

    private static int TryGetInt(HashEntry[] entries, string field, int defaultValue)
    {
        var e = Array.Find(entries, x => (string)x.Name == field);
        if (e.Equals(default) || e.Value.IsNull) return defaultValue;
        if (int.TryParse(e.Value.ToString(), out var v)) return v;
        try { return (int)e.Value; } catch { return defaultValue; }
    }

    private static double TryGetDouble(HashEntry[] entries, string field, double defaultValue)
    {
        var e = Array.Find(entries, x => (string)x.Name == field);
        if (e.Equals(default) || e.Value.IsNull) return defaultValue;
        if (double.TryParse(e.Value.ToString(), out var v)) return v;
        try { return (double)e.Value; } catch { return defaultValue; }
    }

    private static bool TryGetBool(HashEntry[] entries, string field, bool defaultValue)
    {
        var e = Array.Find(entries, x => (string)x.Name == field);
        if (e.Equals(default) || e.Value.IsNull) return defaultValue;
        if (int.TryParse(e.Value.ToString(), out var iv)) return iv == 1;
        if (bool.TryParse(e.Value.ToString(), out var bv)) return bv;
        try { return (int)e.Value == 1; } catch { return defaultValue; }
    }

    private static Guid? TryGetGuid(HashEntry[] entries, string field)
    {
        var e = Array.Find(entries, x => (string)x.Name == field);
        if (e.Equals(default) || e.Value.IsNull) return null;
        if (Guid.TryParse(e.Value.ToString(), out var g)) return g;
        return null;
    }

    #endregion

    #region Initialize / Ensure helpers

    private static HashEntry[] MapToHashEntries(SessionUnitCacheItem unit)
    {
        var entries = new HashEntry[]
            {
                new HashEntry(F_Id, Safe(unit.Id.ToString())),
                new HashEntry(F_SessionId, Safe(unit.SessionId)),
                new HashEntry(F_OwnerId, Safe(unit.OwnerId)),
                new HashEntry(F_OwnerObjectType, Safe(unit.OwnerObjectType?.ToString())),
                new HashEntry(F_DestinationId, Safe(unit.DestinationId)),
                new HashEntry(F_DestinationObjectType, Safe(unit.DestinationObjectType?.ToString())),
                new HashEntry(F_IsStatic, Safe(unit.IsStatic ? 1 : 0)),
                new HashEntry(F_IsPublic, Safe(unit.IsPublic ? 1 : 0)),
                new HashEntry(F_IsVisible, Safe(unit.IsVisible ? 1 : 0)),
                new HashEntry(F_IsEnabled, Safe(unit.IsEnabled ? 1 : 0)),
                new HashEntry(F_ReadedMessageId, Safe(unit.ReadedMessageId)),
                new HashEntry(F_LastMessageId, Safe(unit.LastMessageId)),
                new HashEntry(F_PublicBadge, Safe(unit.PublicBadge)),
                new HashEntry(F_PrivateBadge, Safe(unit.PrivateBadge)),
                new HashEntry(F_RemindAllCount, Safe(unit.RemindAllCount)),
                new HashEntry(F_RemindMeCount, Safe(unit.RemindMeCount)),
                new HashEntry(F_FollowingCount, Safe(unit.FollowingCount)),
                new HashEntry(F_Ticks, Safe(unit.Ticks)),
                new HashEntry(F_Sorting, Safe(unit.Sorting)),
            };
        return entries;
    }
    public async Task InitializeSessionCacheAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        var unitList = units.ToList();
        if (unitList.Count == 0) return;

        var sessionUnitIdSetKey = SessionUnitIdSetKey(sessionId);
        var lastMsgKey = LastMessageSetKey(sessionId);

        var batch = Database.CreateBatch();

        // clear existing set to avoid stale members
        _ = batch.KeyDeleteAsync(sessionUnitIdSetKey);

        var setAddTasks = new List<Task<bool>>();
        var hashTasks = new List<Task>();
        var zsetAddTasks = new List<Task<bool>>();

        foreach (var unit in unitList)
        {
            var idStr = unit.Id.ToString();
            setAddTasks.Add(batch.SetAddAsync(sessionUnitIdSetKey, idStr));

            var hashKey = UnitKey(unit.SessionId ?? Guid.Empty, unit.Id);

            var entries = MapToHashEntries(unit);

            hashTasks.Add(batch.HashSetAsync(hashKey, entries));

            if (unit.LastMessageId.HasValue)
            {
                zsetAddTasks.Add(batch.SortedSetAddAsync(lastMsgKey, idStr, unit.LastMessageId.Value));
            }

            _ = batch.KeyExpireAsync(hashKey, _cacheExpire);
        }

        _ = batch.KeyExpireAsync(sessionUnitIdSetKey, _cacheExpire);
        _ = batch.KeyExpireAsync(lastMsgKey, _cacheExpire);

        batch.Execute();

        if (setAddTasks.Count > 0) await Task.WhenAll(setAddTasks);
        if (hashTasks.Count > 0) await Task.WhenAll(hashTasks);
        if (zsetAddTasks.Count > 0) await Task.WhenAll(zsetAddTasks);
    }

    public async Task InitializeSessionCacheAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchFromDb)
    {
        ArgumentNullException.ThrowIfNull(fetchFromDb);
        var units = (await fetchFromDb(sessionId))?.ToList() ?? new List<SessionUnitCacheItem>();
        await InitializeSessionCacheAsync(sessionId, units);
    }

    public async Task EnsureSessionInitializedAsync(Guid sessionId, Message message)
    {
        var sessionUnitIdSetKey = SessionUnitIdSetKey(sessionId);
        if (!await Database.KeyExistsAsync(sessionUnitIdSetKey))
        {
            await InitializeSessionCacheAsync(sessionId, async (sid) => await SessionUnitManager.GetCacheListAsync(message));
        }
    }

    #endregion

    #region GetListByOwnerIdAsync (DB initial values + Redis merge)

    /// <summary>
    /// Get list of SessionUnitCacheItem by ownerId. Units argument is the DB-supplied initial list (may be from DB).
    /// Redis values take precedence when present. After merging, Redis is backfilled with merged values and owner zset updated.
    /// </summary>
    public async Task<List<SessionUnitCacheItem>> GetListByOwnerIdAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units)
    {
        var unitList = units?.ToList() ?? new List<SessionUnitCacheItem>();
        if (unitList.Count == 0) return new List<SessionUnitCacheItem>();

        string ownerKey = OwnerSetKey(ownerId);

        // read owner zset (may be empty)
        var redisZset = await Database.SortedSetRangeByScoreWithScoresAsync(ownerKey);
        var redisMap = redisZset.ToDictionary(x => Guid.Parse(x.Element), x => x.Score);

        // batch read unit hashes
        var batch = Database.CreateBatch();
        var hashTasks = new Dictionary<Guid, Task<HashEntry[]>>();

        foreach (var unit in unitList)
        {
            // ensure we have a valid session id in the DB item; if missing skip
            if (!unit.SessionId.HasValue) continue;
            hashTasks[unit.Id] = batch.HashGetAllAsync(UnitKey(unit.SessionId.Value, unit.Id));
        }

        batch.Execute();
        await Task.WhenAll(hashTasks.Values);

        var result = new List<SessionUnitCacheItem>(unitList.Count);
        var ownerZsetUpdateTasks = new List<Task<bool>>();
        var hashSetBackfillTasks = new List<Task>();

        foreach (var unit in unitList)
        {
            if (!unit.SessionId.HasValue) continue;
            var id = unit.Id;
            var entries = hashTasks.ContainsKey(id) ? hashTasks[id].Result : Array.Empty<HashEntry>();

            // defaults from DB
            double sorting = unit.Sorting;
            long lastMessageId = unit.LastMessageId ?? 0;

            // override from redis hash if present
            if (entries != null && entries.Length > 0)
            {
                sorting = TryGetDouble(entries, F_Sorting, sorting);
                lastMessageId = TryGetLong(entries, F_LastMessageId, lastMessageId);
            }

            // override from owner zset score if present
            if (redisMap.TryGetValue(id, out var score))
            {
                // score = sorting * MULT + lastMessageId
                var sortingFromScore = Math.Floor(score / OWNER_SCORE_MULT);
                var lastFromScore = (long)(score % OWNER_SCORE_MULT);
                if (sortingFromScore > 0) sorting = sortingFromScore;
                if (lastFromScore > 0) lastMessageId = lastFromScore;
            }

            var item = new SessionUnitCacheItem
            {
                Id = id,
                SessionId = unit.SessionId,
                OwnerId = ownerId,
                OwnerObjectType = unit.OwnerObjectType,
                DestinationId = unit.DestinationId,
                DestinationObjectType = unit.DestinationObjectType,
                IsStatic = unit.IsStatic,
                IsPublic = unit.IsPublic,
                IsVisible = unit.IsVisible,
                IsEnabled = unit.IsEnabled,
                ReadedMessageId = unit.ReadedMessageId,
                PublicBadge = unit.PublicBadge,
                PrivateBadge = unit.PrivateBadge,
                RemindAllCount = unit.RemindAllCount,
                RemindMeCount = unit.RemindMeCount,
                FollowingCount = unit.FollowingCount,
                Ticks = unit.Ticks,
                Sorting = sorting,
                LastMessageId = lastMessageId,
            };

            result.Add(item);

            // backfill merged values to Redis (hash + owner zset)
            var unitKey = UnitKey(item.SessionId!.Value, id);
            var hashEntries = MapToHashEntries(unit);

            hashSetBackfillTasks.Add(Database.HashSetAsync(unitKey, hashEntries));

            double composite = item.Sorting * OWNER_SCORE_MULT + item.LastMessageId ?? 0;
            ownerZsetUpdateTasks.Add(Database.SortedSetAddAsync(ownerKey, id.ToString(), composite));

            // ensure expire
            _ = Database.KeyExpireAsync(unitKey, _cacheExpire);
        }

        if (hashSetBackfillTasks.Count > 0) await Task.WhenAll(hashSetBackfillTasks);
        if (ownerZsetUpdateTasks.Count > 0) await Task.WhenAll(ownerZsetUpdateTasks);

        // final sort for return
        result = result.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId).ToList();

        if (_cacheExpire.HasValue)
        {
            await Database.KeyExpireAsync(ownerKey, _cacheExpire);
        }

        return result;
    }

    #endregion

    #region Map / GetMany

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
                if (Guid.TryParse(e.Value.ToString(), out var gid)) item.Id = gid;
                else item.Id = Guid.Empty;
            }
            else if (name == F_SessionId && e.Value.HasValue) item.SessionId = Guid.TryParse(e.Value.ToString(), out var ss) ? ss : (Guid?)null;
            else if (name == F_OwnerId && e.Value.HasValue) item.OwnerId = TryGetLong(entries, F_OwnerId, 0);
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
            else if (name == F_ReadedMessageId && e.Value.HasValue) item.ReadedMessageId = TryGetLong(entries, F_ReadedMessageId, 0);
            else if (name == F_LastMessageId && e.Value.HasValue) item.LastMessageId = TryGetLong(entries, F_LastMessageId, 0);
            else if (name == F_PublicBadge && e.Value.HasValue) item.PublicBadge = TryGetInt(entries, F_PublicBadge, 0);
            else if (name == F_PrivateBadge && e.Value.HasValue) item.PrivateBadge = TryGetInt(entries, F_PrivateBadge, 0);
            else if (name == F_RemindAllCount && e.Value.HasValue) item.RemindAllCount = TryGetInt(entries, F_RemindAllCount, 0);
            else if (name == F_RemindMeCount && e.Value.HasValue) item.RemindMeCount = TryGetInt(entries, F_RemindMeCount, 0);
            else if (name == F_FollowingCount && e.Value.HasValue) item.FollowingCount = TryGetInt(entries, F_FollowingCount, 0);
            else if (name == F_Ticks && e.Value.HasValue) item.Ticks = TryGetDouble(entries, F_Ticks, 0);
            else if (name == F_Sorting && e.Value.HasValue) item.Sorting = TryGetDouble(entries, F_Sorting, 0);
        }

        return item;
    }

    #endregion

    #region BatchIncrementBadgeAndSetLastMessageAsync (updates owner zset score)

    public async Task BatchIncrementBadgeAndSetLastMessageAsync(
        Message message,
        TimeSpan? expire = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var sessionId = message.SessionId!.Value;
        var lastMessageId = message.Id;
        var lastMsgKey = LastMessageSetKey(sessionId);
        var isPrivate = message.IsPrivateMessage();
        var isRemindAll = message.IsRemindAll;

        var sessionUnitList = await SessionUnitManager.GetCacheListAsync(message);
        if (sessionUnitList == null || sessionUnitList.Count == 0) return;

        var batch = Database.CreateBatch();

        var hashSetTasks = new List<Task<bool>>();
        var hashIncTasks = new List<Task<long>>();
        var zsetGlobalTasks = new List<Task<bool>>();
        var zsetOwnerTasks = new List<Task<bool>>();
        var expireTasks = new List<Task<bool>>();

        foreach (var unit in sessionUnitList)
        {
            var id = unit.Id;
            var key = UnitKey(sessionId, id);
            var idStr = id.ToString();
            var isSender = id == message.SenderSessionUnitId;

            if (!isSender)
            {
                if (isPrivate) hashIncTasks.Add(batch.HashIncrementAsync(key, F_PrivateBadge, 1));
                else hashIncTasks.Add(batch.HashIncrementAsync(key, F_PublicBadge, 1));

                if (isRemindAll) hashIncTasks.Add(batch.HashIncrementAsync(key, F_RemindAllCount, 1));
            }

            // update lastMessageId in hash
            hashSetTasks.Add(batch.HashSetAsync(key, F_LastMessageId, lastMessageId));
            // update ticks
            hashSetTasks.Add(batch.HashSetAsync(key, F_Ticks, (double)DateTime.UtcNow.Ticks));
            // update global last-message sorted set
            zsetGlobalTasks.Add(batch.SortedSetAddAsync(lastMsgKey, idStr, lastMessageId));

            // owner-specific zset update (compute composite score from Sorting & lastMessageId)
            var ownerKey = OwnerSetKey(unit.OwnerId);

            // read current sorting from hash (deferred read not allowed inside pipeline easily) - we will read from unit.Sorting (cached snapshot) and redis hash will be handled by GetListByOwnerId flow
            double sorting = unit.Sorting;

            var score = sorting * OWNER_SCORE_MULT + lastMessageId;
            zsetOwnerTasks.Add(batch.SortedSetAddAsync(ownerKey, idStr, score));

            // expire tasks
            expireTasks.Add(batch.KeyExpireAsync(key, expire ?? _cacheExpire));
            // ensure owner zset expire and global zset expire
            expireTasks.Add(batch.KeyExpireAsync(ownerKey, expire ?? _cacheExpire));
            expireTasks.Add(batch.KeyExpireAsync(lastMsgKey, expire ?? _cacheExpire));
        }

        batch.Execute();

        if (hashSetTasks.Count > 0) await Task.WhenAll(hashSetTasks);
        if (hashIncTasks.Count > 0) await Task.WhenAll(hashIncTasks);
        if (zsetGlobalTasks.Count > 0) await Task.WhenAll(zsetGlobalTasks);
        if (zsetOwnerTasks.Count > 0) await Task.WhenAll(zsetOwnerTasks);
        if (expireTasks.Count > 0) await Task.WhenAll(expireTasks);
    }

    #endregion

    #region Remove / Set / Misc

    public async Task AddUnitIdToSessionAsync(Guid sessionId, Guid unitId)
    {
        await Database.SetAddAsync(SessionUnitIdSetKey(sessionId), unitId.ToString());
        await Database.KeyExpireAsync(SessionUnitIdSetKey(sessionId), _cacheExpire);
    }

    public async Task RemoveUnitIdFromSessionAsync(Guid sessionId, Guid unitId)
    {
        await Database.SetRemoveAsync(SessionUnitIdSetKey(sessionId), unitId.ToString());
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

        var r1 = await Database.KeyDeleteAsync(setKey);
        var r2 = await Database.KeyDeleteAsync(lastKey);
        return r1 && r2;
    }

    public async Task UpdateReadedMessageIdAsync(Guid sessionId, Guid unitId, long readedMessageId, TimeSpan? expire = null)
    {
        var key = UnitKey(sessionId, unitId);
        await Database.HashSetAsync(key, F_ReadedMessageId, readedMessageId);
        if (expire.HasValue) await Database.KeyExpireAsync(key, expire.Value);
    }

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
            tasks.Add(Database.KeyExpireAsync(SessionUnitIdSetKey(sessionId), e));
            tasks.Add(Database.KeyExpireAsync(LastMessageSetKey(sessionId), e));
        }

        var results = await Task.WhenAll(tasks);
        return results.All(x => x);
    }

    #endregion
}
