using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionUnitManager
    {
        Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping);

        Task<SessionUnit> SetReadedAsync(SessionUnit entity, long messageId, bool isForce = false);

        Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed);

        Task<SessionUnit> RemoveAsync(SessionUnit entity);

        Task<SessionUnit> KillAsync(SessionUnit entity);

        Task<SessionUnit> ClearMessageAsync(SessionUnit entity);

        Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId);

        Task<int> GetBadgeAsync(long ownerId, bool? isImmersed = null);

        Task<int> GetCountAsync(Guid sessionId);

        Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId);

        Task<List<SessionUnitInfo>> GetCacheListBySessionIdAsync(Guid sessionId);

        Task<List<SessionUnitInfo>> GetOrAddCacheListBySessionIdAsync(Guid sessionId);

        Task SetCacheListBySessionIdAsync(Guid sessionId);

        Task<List<SessionUnitInfo>> GetListBySessionIdAsync(Guid sessionId);

        Task RemoveCacheListBySessionIdAsync(Guid sessionId);

    }
}
