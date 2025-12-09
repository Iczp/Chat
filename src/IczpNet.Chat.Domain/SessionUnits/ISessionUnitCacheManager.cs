using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitCacheManager
{
    Task<IEnumerable<SessionUnitCacheItem>> SetListBySessionAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units);

    Task<IEnumerable<SessionUnitCacheItem>> SetListBySessionAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> SetListBySessionIfNotExistsAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> GetOrSetListBySessionAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IDictionary<Guid, long>> GetDictBySessionAsync(Guid sessionId);

    Task<IDictionary<long, Guid>> GetUnitsBySessionAsync(Guid sessionId, List<long> ownerIds);

    Task<IEnumerable<SessionUnitCacheItem>> GetListBySessionAsync(Guid sessionId);

    Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units);

    Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerIfNotExistsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> GetOrSetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> GetListByOwnerAsync(
        long ownerId, 
        double minScore = double.NegativeInfinity, 
        double maxScore = double.PositiveInfinity, 
        long skip = 0, 
        long take = -1, 
        bool isDescending = true);

    Task<KeyValuePair<Guid, double>[]> GetSortedSetByOwnerAsync(
        long ownerId, 
        double minScore = double.NegativeInfinity, 
        double maxScore = double.PositiveInfinity, 
        long skip = 0, 
        long take = -1, 
        bool isDescending = true);

    Task<KeyValuePair<Guid, double>[]> GetHistoryByOwnerAsync(
        long ownerId,
        double minScore = double.NegativeInfinity,
        double maxScore = double.PositiveInfinity,
        long skip = 0,
        long take = -1,
        bool isDescending = true);

    Task<long> GetTotalCountByOwnerAsync(long ownerId);

    Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetManyAsync(IEnumerable<Guid> unitIds);

    Task<KeyValuePair<Guid, SessionUnitCacheItem>[]> GetOrSetManyAsync(IEnumerable<Guid> unitIds, Func<List<Guid>, Task<KeyValuePair<Guid, SessionUnitCacheItem>[]>> func);

    Task BatchIncrementAsync(Message message, TimeSpan? expire = null);

    Task UpdateCountersync(SessionUnitCounterInfo counter, Func<Guid, Task<SessionUnitCacheItem>> fetchTask);

    Task<bool> SetTotalBadgeAsync(long ownerId, long badge);

    Task<long?> GetTotalBadgeAsync(long ownerId);

    Task<bool> RemoveTotalBadgeAsync(long ownerId);

    Task<SessionUnitCacheItem> UnitTestAsync();



    //Task<long?> GetBadgeAsync(Guid sessionId, Guid sessionUnitId);
    //Task<bool> SetBadgeAsync(Guid sessionId, Guid sessionUnitId, long badge);
    //Task<long?> GetLastMessageIdAsync(Guid sessionId, Guid sessionUnitId);

    ///// <summary>
    ///// 批量获取 Badge + LastMessageId
    ///// </summary>
    //Task<IDictionary<Guid, SessionUnitStoreItem>> GetManyAsync(Guid sessionId, List<Guid> unitIds);

    //Task<SessionUnitStoreItem> GetItemAsync(Guid sessionId, Guid sessionUnitId);

    //Task<IDictionary<Guid, long>> GetTopSessionUnitsAsync(Guid sessionId, int count);

    //Task<bool> RemoveAsync(Guid sessionId, Guid sessionUnitId);
    //Task<bool> RemoveAllAsync(Guid sessionId);
}