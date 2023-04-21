using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using IczpNet.AbpCommons.Extensions;

namespace IczpNet.Chat.SessionSections.Sessions;

public class SessionUnitManager : DomainService, ISessionUnitManager
{
    protected ISessionUnitRepository Repository { get; }
    protected IRepository<ReadedRecorder, Guid> ReadedRecorderRepository { get; }
    protected IMessageRepository MessageRepository { get; }
    protected IDistributedCache<List<SessionUnitCacheItem>, string> UnitListCache { get; }
    protected IDistributedCache<string, Guid> UnitCountCache { get; }

    public SessionUnitManager(
        ISessionUnitRepository repository,
        IRepository<ReadedRecorder, Guid> readedRecorderRepository,
        IMessageRepository messageRepository,
        IDistributedCache<List<SessionUnitCacheItem>, string> unitListCache,
        IDistributedCache<string, Guid> unitCountCache)
    {
        Repository = repository;
        ReadedRecorderRepository = readedRecorderRepository;
        MessageRepository = messageRepository;
        UnitListCache = unitListCache;
        UnitCountCache = unitCountCache;
    }

    protected virtual async Task<SessionUnit> SetEntityAsync(SessionUnit entity, Action<SessionUnit> action = null)
    {
        action?.Invoke(entity);

        return await Repository.UpdateAsync(entity);
    }

    public virtual async Task<Guid?> FindIdAsync(Expression<Func<SessionUnit, bool>> predicate)
    {
        return (await Repository.GetQueryableAsync())
            .Where(predicate)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefault();
        ;
    }
    public virtual Task<Guid?> FindIdAsync(long ownerId, long destinactionId)
    {
        return FindIdAsync(x => x.OwnerId == ownerId && x.DestinationId == destinactionId);
    }

    public virtual Task<SessionUnit> FindAsync(long ownerId, long destinactionId)
    {
        return FindAsync(x => x.OwnerId == ownerId && x.DestinationId == destinactionId);
    }

    public virtual Task<SessionUnit> FindAsync(Expression<Func<SessionUnit, bool>> predicate)
    {
        return Repository.FindAsync(predicate);
    }

    public virtual Task<SessionUnit> FindBySessionIdAsync(Guid sessionId, long ownerId)
    {
        return FindAsync(x => x.SessionId == sessionId && x.OwnerId == ownerId);
    }

    public virtual Task<SessionUnit> GetAsync(Guid id)
    {
        return Repository.GetAsync(id);
    }

    public Task<SessionUnit> SetMemberNameAsync(SessionUnit entity, string memberName)
    {
        return SetEntityAsync(entity, x => x.SetMemberName(memberName));
    }

    public Task<SessionUnit> SetRenameAsync(SessionUnit entity, string rename)
    {
        return SetEntityAsync(entity, x => x.SetRename(rename));
    }

    public virtual Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping)
    {
        return SetEntityAsync(entity, x => x.SetTopping(isTopping));
    }

    public virtual async Task<SessionUnit> SetReadedAsync(SessionUnit entity, long messageId, bool isForce = false)
    {
        var message = await MessageRepository.GetAsync(messageId);

        // add readedRecorder
        /// ...
        return await SetEntityAsync(entity, x => x.SetReaded(message.Id, isForce = false));
    }

    public virtual Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed)
    {
        return SetEntityAsync(entity, x => x.SetImmersed(isImmersed));
    }

    public virtual Task<SessionUnit> RemoveAsync(SessionUnit entity)
    {
        return SetEntityAsync(entity, x => x.Remove(Clock.Now));
    }

    public virtual Task<SessionUnit> KillAsync(SessionUnit entity)
    {
        return SetEntityAsync(entity, x => x.Kill(Clock.Now));
    }

    public virtual Task<SessionUnit> ClearMessageAsync(SessionUnit entity)
    {
        return SetEntityAsync(entity, x => x.ClearMessage(Clock.Now));
    }

    public virtual Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<int> GetBadgeAsync(long ownerId, bool? isImmersed = null)
    {
        var badge = (await Repository.GetQueryableAsync())
            .Where(x => x.OwnerId == ownerId)
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed)
            .Select(x => new
            {
                PublicBadge = x.Session.MessageList.Count(d =>
                    //!x.IsRollbacked &&
                    d.SenderId != x.OwnerId &&
                    !d.IsPrivate &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)),
                PrivateBadge = x.Session.MessageList.Count(d =>
                    //!x.IsRollbacked &&
                    d.SenderId != x.OwnerId &&
                    (d.IsPrivate && d.ReceiverId == x.OwnerId) &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)),
            })
            .Select(x => x.PublicBadge + x.PrivateBadge)
            .Where(x => x > 0)
            .ToList()
        .Sum();

        return badge;
    }


    public virtual async Task<int> GetCountAsync(Guid sessionId)
    {
        var value = await UnitCountCache.GetOrAddAsync(sessionId, async () =>
        {
            var count = await Repository.CountAsync(x => x.SessionId == sessionId && x.IsPublic && !x.IsKilled && x.IsEnabled);
            return count.ToString();
        });
        return int.Parse(value);
    }

    public virtual Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
    {
        return Repository.BatchUpdateAsync(sessionId, lastMessageId, sessionUnitIdList);
    }

    public virtual Task<List<SessionUnitCacheItem>> GetCacheListAsync(string sessionUnitCachKey)
    {
        return UnitListCache.GetAsync(sessionUnitCachKey);
    }

    public virtual Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(Guid sessionId)
    {
        return UnitListCache.GetOrAddAsync($"{new SessionUnitCacheKey(sessionId)}", () => GetListBySessionIdAsync(sessionId));
    }

    public virtual async Task SetCacheListBySessionIdAsync(Guid sessionId)
    {
        var sessionUnitInfoList = await GetListBySessionIdAsync(sessionId);

        await SetCacheListAsync($"{new SessionUnitCacheKey(sessionId)}", sessionUnitInfoList);
    }

    public virtual async Task SetCacheListAsync(string cacheKey, List<SessionUnitCacheItem> sessionUnitList, DistributedCacheEntryOptions options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        await UnitListCache.SetAsync(cacheKey, sessionUnitList, options, hideErrors, considerUow, token);
    }

    public virtual async Task<List<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId)
    {
        var list = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && !x.IsKilled && x.IsEnabled)
            .Select(x => new SessionUnitCacheItem()
            {
                Id = x.Id,
                SessionId = x.SessionId,
                DestinationId = x.DestinationId,
                OwnerId = x.OwnerId,
                DestinationObjectType = x.DestinationObjectType,
                IsPublic = x.IsPublic,
                ServiceStatus = x.ServiceStatus,
            })
            .ToList();

        await UnitCountCache.SetAsync(sessionId, list.Where(x => x.IsPublic).Count().ToString());

        return list;
    }

    public virtual async Task RemoveCacheListBySessionIdAsync(Guid sessionId)
    {
        await UnitListCache.RemoveAsync($"{new SessionUnitCacheKey(sessionId)}");
    }

    private async Task<IQueryable<SessionUnit>> GetOwnerQueryableAsync(long ownerId, List<ChatObjectTypeEnums> destinationObjectTypeList = null)
    {
        return (await Repository.GetQueryableAsync())
              .Where(x => x.OwnerId.Equals(ownerId) && !x.IsKilled && x.IsEnabled)
              .WhereIf(destinationObjectTypeList.IsAny(), x => destinationObjectTypeList.Contains(x.DestinationObjectType.Value));

    }

    public virtual async Task<IQueryable<SessionUnit>> GetSameSessionQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var targetSessionIdList = (await GetOwnerQueryableAsync(targetChatObjectId, chatObjectTypeList))
            .Select(x => x.SessionId);

        var sourceQuery = (await GetOwnerQueryableAsync(sourceChatObjectId, chatObjectTypeList))
            .Where(x => targetSessionIdList.Contains(x.SessionId))
            ;

        return sourceQuery;
    }

    public virtual async Task<int> GetSameSessionCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var query = await GetSameSessionQeuryableAsync(sourceChatObjectId, targetChatObjectId, chatObjectTypeList);

        return await AsyncExecuter.CountAsync(query);
    }

    public virtual async Task<IQueryable<SessionUnit>> GetSameDestinationQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var destinationIdList = (await GetOwnerQueryableAsync(targetChatObjectId, chatObjectTypeList))
            .Where(x => x.DestinationId.HasValue)
            .Select(x => x.DestinationId.Value);

        var sourceQuery = (await GetOwnerQueryableAsync(sourceChatObjectId, chatObjectTypeList))
            .Where(x => x.DestinationId.HasValue)
            .Where(x => destinationIdList.Contains(x.DestinationId.Value))
            ;

        return sourceQuery;
    }

    public virtual async Task<int> GetSameDestinationCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var query = await GetSameDestinationQeuryableAsync(sourceChatObjectId, targetChatObjectId, chatObjectTypeList);

        return await AsyncExecuter.CountAsync(query);
    }

}
