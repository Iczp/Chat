using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionUnitManager
    {
        Task<SessionUnit> GetAsync(Guid sessionUnitId);

        Task<SessionUnit> FindAsync(Expression<Func<SessionUnit, bool>> predicate);

        Task<SessionUnit> FindAsync(long ownerId, long destinactionId);

        Task<SessionUnit> FindBySessionIdAsync(Guid sessionId, long ownerId);

        Task<Guid?> FindIdAsync(long ownerId, long destinactionId);

        Task<Guid?> FindIdAsync(Expression<Func<SessionUnit, bool>> predicate);

        Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping);

        Task<SessionUnit> SetReadedAsync(SessionUnit entity, long messageId, bool isForce = false);

        Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed);

        Task<SessionUnit> RemoveAsync(SessionUnit entity);

        Task<SessionUnit> KillAsync(SessionUnit entity);

        Task<SessionUnit> ClearMessageAsync(SessionUnit entity);

        Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId);

        Task<int> GetBadgeAsync(long ownerId, bool? isImmersed = null);

        Task<int> GetCountAsync(Guid sessionId);

        Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null);

        Task<List<SessionUnitCacheItem>> GetCacheListAsync(string cacheKey);

        Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(Guid sessionId);

        Task SetCacheListBySessionIdAsync(Guid sessionId);

        Task SetCacheListAsync(string cacheKey, List<SessionUnitCacheItem> sessionUnitList, DistributedCacheEntryOptions options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

        Task<List<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId);

        Task RemoveCacheListBySessionIdAsync(Guid sessionId);

    }
}
