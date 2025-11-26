using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitCacheManager
{
    Task SetListBySessionAsync(Guid sessionId, IEnumerable<SessionUnitCacheItem> units);

    Task SetListBySessionAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task SetListBySessionIfNotExistsAsync(Guid sessionId, Func<Guid, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units);

    Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> SetListByOwnerIfNotExistsAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> GetOrSetListByOwnerAsync(long ownerId, Func<long, Task<IEnumerable<SessionUnitCacheItem>>> fetchTask);

    Task<IEnumerable<SessionUnitCacheItem>> GetListByOwnerIdAsync(long ownerId);

    Task<List<SessionUnitCacheItem>> GetListByOwnerIdAsync(long ownerId, IEnumerable<SessionUnitCacheItem> units);

    Task BatchIncrementBadgeAndSetLastMessageAsync(Message message, TimeSpan? expire = null);
    

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