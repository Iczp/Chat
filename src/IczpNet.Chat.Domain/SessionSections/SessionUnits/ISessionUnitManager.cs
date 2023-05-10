using IczpNet.Chat.Enums;
using IczpNet.Chat.OpenedRecorders;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitManager
    {
        Task<SessionUnit> GetAsync(Guid sessionUnitId);

        Task<List<SessionUnit>> GetManyAsync(List<Guid> idList);

        Task<SessionUnit> FindAsync(Expression<Func<SessionUnit, bool>> predicate);

        Task<SessionUnit> FindAsync(long ownerId, long destinactionId);

        Task<SessionUnit> CreateIfNotContainsAsync(SessionUnit sessionUnit);

        Task<SessionUnit> FindBySessionIdAsync(Guid sessionId, long ownerId);

        Task<Guid?> FindIdAsync(long ownerId, long destinactionId);

        Task<Guid?> FindIdAsync(Expression<Func<SessionUnit, bool>> predicate);

        Task<SessionUnit> SetMemberNameAsync(SessionUnit entity, string memberName);

        Task<SessionUnit> SetRenameAsync(SessionUnit entity, string rename);

        Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping);

        Task<SessionUnit> SetReadedAsync(SessionUnit entity, long messageId, bool isForce = false);

        Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed);

        Task<SessionUnit> RemoveAsync(SessionUnit entity);

        Task<SessionUnit> KillAsync(SessionUnit entity);

        Task<SessionUnit> ClearMessageAsync(SessionUnit entity);

        Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId);

        Task<int> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null);

        Task<int> GetBadgeByIdAsync(Guid sessionUnitId, bool? isImmersed = null);

        Task<Dictionary<Guid, int>> GetBadgeByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<Dictionary<Guid, int>> GetReminderCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<Dictionary<Guid, int>> GetFollowingCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<int> GetCountAsync(Guid sessionId);

        Task<int> BatchUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null);

        Task<List<SessionUnitCacheItem>> GetCacheListAsync(string cacheKey);

        Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(Guid sessionId);

        Task SetCacheListBySessionIdAsync(Guid sessionId);

        Task SetCacheListAsync(string cacheKey, List<SessionUnitCacheItem> sessionUnitList, DistributedCacheEntryOptions options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

        Task<List<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId);

        Task RemoveCacheListBySessionIdAsync(Guid sessionId);

        Task<IQueryable<SessionUnit>> GetSameSessionQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<int> GetSameSessionCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<IQueryable<SessionUnit>> GetSameDestinationQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<int> GetSameDestinationCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

    }
}
