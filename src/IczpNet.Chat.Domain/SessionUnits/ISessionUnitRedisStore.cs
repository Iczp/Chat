using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

public interface ISessionUnitRedisStore
{
    Task BatchIncrementBadgeAndSetLastMessageAsync(Message message, TimeSpan? expire = null);
    Task<long?> GetBadgeAsync(Guid sessionId, Guid sessionUnitId);
    Task<bool> SetBadgeAsync(Guid sessionId, Guid sessionUnitId, long badge);
    Task<long?> GetLastMessageIdAsync(Guid sessionId, Guid sessionUnitId);

    /// <summary>
    /// 批量获取 Badge + LastMessageId
    /// </summary>
    Task<IDictionary<Guid, SessionUnitStoreItem>> GetManyAsync(Guid sessionId, List<Guid> unitIds);

    Task<SessionUnitStoreItem> GetItemAsync(Guid sessionId, Guid sessionUnitId);

    Task<IDictionary<Guid, long>> GetTopSessionUnitsAsync(Guid sessionId, int count);

    Task<bool> RemoveAsync(Guid sessionId, Guid sessionUnitId);
    Task<bool> RemoveAllAsync(Guid sessionId);
}