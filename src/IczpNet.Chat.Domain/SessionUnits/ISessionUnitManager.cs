﻿using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits
{
    public interface ISessionUnitManager
    {
        Task<SessionUnit> GetAsync(Guid sessionUnitId);

        Task<List<SessionUnit>> GetManyAsync(List<Guid> idList);

        Task<SessionUnit> FindAsync(Expression<Func<SessionUnit, bool>> predicate);

        Task<SessionUnit> FindAsync(long ownerId, long destinactionId);

        SessionUnit Create(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> action);

        SessionUnit Generate(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> action);

        Task<SessionUnit> CreateIfNotContainsAsync(SessionUnit sessionUnit);

        Task<SessionUnit> CreateIfNotContainsAsync(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> setting);

        Task<SessionUnit> FindBySessionIdAsync(Guid sessionId, long ownerId);

        Task<Guid?> FindIdAsync(long ownerId, long destinactionId);

        Task<bool> IsAnyAsync(long ownerId, long destinactionId);

        Task<Guid?> FindIdAsync(Expression<Func<SessionUnit, bool>> predicate);

        Task<SessionUnit> SetMemberNameAsync(SessionUnit entity, string memberName);

        Task<SessionUnit> SetRenameAsync(SessionUnit entity, string rename);

        Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping);

        Task<SessionUnit> SetReadedMessageIdAsync(SessionUnit entity, bool isForce = false, long? messageId = null);

        Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed);

        Task<SessionUnit> SetIsContactsAsync(SessionUnit entity, bool isContacts);

        Task<SessionUnit> SetIsShowMemberNameAsync(SessionUnit entity, bool isShowMemberName);

        Task<SessionUnit> RemoveAsync(SessionUnit entity);

        Task<SessionUnit> KillAsync(SessionUnit entity);

        Task<SessionUnit> ClearMessageAsync(SessionUnit entity);

        Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId);

        Task<Dictionary<ChatObjectTypeEnums, int>> GetTypeBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null);

        Task<int> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null);

        Task<int> GetBadgeByIdAsync(Guid sessionUnitId, bool? isImmersed = null);

        Task<Dictionary<Guid, int>> GetBadgeByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<SessionUnitCounterInfo> GetCounterAsync(Guid sessionUnitId, long minMessageId = 0, bool? isImmersed = null);

        Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<Dictionary<Guid, int>> GetReminderCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<Dictionary<Guid, int>> GetFollowingCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null);

        Task<int> GetCountBySessionIdAsync(Guid sessionId);

        Task<int> GetCountByOwnerIdAsync(long ownerId);

        Task<List<SessionUnitCacheItem>> GetCacheListAsync(string cacheKey);

        Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(Guid sessionId);

        Task<List<SessionUnitCacheItem>> GetCacheListBySessionIdAsync(Guid sessionId);

        Task<SessionUnitCacheItem> GetCacheItemAsync(Guid sessionId, Guid sessionUnitId);

        Task<SessionUnitCacheItem> GetCacheItemAsync(SessionUnit sessionUnit);

        Task SetCacheListBySessionIdAsync(Guid sessionId, List<SessionUnitCacheItem> sessionUnitList);

        Task SetCacheListAsync(string cacheKey, List<SessionUnitCacheItem> sessionUnitList, DistributedCacheEntryOptions options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default);

        Task<List<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId);

        Task RemoveCacheListBySessionIdAsync(Guid sessionId);

        Task<IQueryable<SessionUnit>> GetSameSessionQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<int> GetSameSessionCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<IQueryable<SessionUnit>> GetSameDestinationQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<int> GetSameDestinationCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null);

        Task<int> IncrementFollowingCountAsync(SessionUnit senderSessionUnit, Message message);

        Task<int> UpdateCachesAsync(SessionUnit senderSessionUnit, Message message);

        Task<int> BatchUpdateAsync(SessionUnit senderSessionUnit, Message message);

        Task<List<Guid>> GetIdListByNameAsync(Guid sessionId, List<string> nameList);

        Task<int> IncremenetAsync(SessionUnitIncrementArgs args);

        Task<DateTime?> SetMuteExpireTimeAsync(SessionUnit muterSessionUnit, DateTime? muteExpireTime, SessionUnit setterSessionUnit, bool isSendMessage);

        Task<DateTime?> SetMuteExpireTimeAsync(SessionUnit muterSessionUnit, DateTime? muteExpireTime);

    }
}
