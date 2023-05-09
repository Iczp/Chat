using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
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
using IczpNet.AbpCommons;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public class SessionUnitManager : DomainService, ISessionUnitManager
{
    protected ISessionUnitRepository Repository { get; }
    protected IMessageRepository MessageRepository { get; }
    protected IDistributedCache<List<SessionUnitCacheItem>, string> UnitListCache { get; }
    protected IDistributedCache<string, Guid> UnitCountCache { get; }

    public SessionUnitManager(
        ISessionUnitRepository repository,
        IMessageRepository messageRepository,
        IDistributedCache<List<SessionUnitCacheItem>, string> unitListCache,
        IDistributedCache<string, Guid> unitCountCache)
    {
        Repository = repository;
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

    public virtual async Task<List<SessionUnit>> GetManyAsync(List<Guid> idList)
    {
        var result = new List<SessionUnit>();

        foreach (var id in idList)
        {
            result.Add(await GetAsync(id));
        }
        return result;
    }

    public virtual async Task<SessionUnit> CreateIfNotContainsAsync(SessionUnit sessionUnit)
    {
        var entity = await FindAsync(sessionUnit.OwnerId, sessionUnit.DestinationId.Value);

        entity ??= await Repository.InsertAsync(sessionUnit, autoSave: true);

        return entity;
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

        Assert.If(entity.SessionId != message.SessionId, $"Not in same session,messageId:{messageId}");

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

    protected virtual async Task<int> GetBadgeAsync(Func<IQueryable<SessionUnit>, IQueryable<SessionUnit>> queryAction)
    {
        var query = queryAction.Invoke(await Repository.GetQueryableAsync());

        var badge = query.Select(x => new
        {
            PublicBadge = x.Session.MessageList.Count(d =>
                !d.IsPrivate &&
                //!x.IsRollbacked &&
                d.SenderId != x.OwnerId &&
                (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)),
            PrivateBadge = x.Session.MessageList.Count(d =>
                d.IsPrivate && d.ReceiverId == x.OwnerId &&
                //!x.IsRollbacked &&
                d.SenderId != x.OwnerId &&
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

    public virtual Task<int> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null)
    {
        return GetBadgeAsync(q =>
            q.Where(x => x.OwnerId == ownerId)
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed));
    }

    public virtual Task<int> GetBadgeByIdAsync(Guid sessionUnitId, bool? isImmersed = null)
    {
        return GetBadgeAsync(q =>
            q.Where(x => x.Id == sessionUnitId)
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed));
    }

    public virtual async Task<Dictionary<Guid, int>> GetBadgeByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        var badges = (await Repository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed)
            .Select(x => new
            {
                x.Id,
                x.OwnerId,
                Messages = x.Session.MessageList.Where(d =>
                    d.Id > minMessageId &&
                    d.SenderId != x.OwnerId &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime))
            })
            .Select(x => new
            {
                x.Id,
                PublicBadge = x.Messages.Count(d => !d.IsPrivate),
                PrivateBadge = x.Messages.Count(d => d.IsPrivate && d.ReceiverId == x.OwnerId),
            })
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(d => d.PublicBadge + d.PrivateBadge))
            .ToDictionary(x => x.Id, x => x.PrivateBadge + x.PublicBadge)
            ;

        return badges;
    }

    public virtual async Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        return (await Repository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed)
            .Select(x => new
            {
                x.Id,
                x.OwnerId,
                x.OwnerFollowList,
                Messages = x.Session.MessageList.Where(d =>
                    d.Id > minMessageId &&
                    d.SenderId != x.OwnerId &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime))
            })
            .Select(x => new SessionUnitStatModel
            {
                Id = x.Id,
                PublicBadge = x.Messages.Count(d => !d.IsPrivate),
                PrivateBadge = x.Messages.Count(d => d.IsPrivate && d.ReceiverId == x.OwnerId),
                FollowingCount = x.Messages.Count(d => x.OwnerFollowList.Any(d => d.DestinationId == x.Id)),
                RemindAllCount = x.Messages.Count(d => d.IsRemindAll && !d.IsRollbacked),
                RemindMeCount = x.Messages.Count(d => d.MessageReminderList.Any(g => g.SessionUnitId == x.Id))
            })
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(d => d.PublicBadge + d.PrivateBadge))
            .ToDictionary(x => x.Id)
            ;
    }

    public virtual async Task<Dictionary<Guid, int>> GetReminderCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed);
        //return ReminderList.AsQueryable().Select(x => x.Message).Where(x => !x.IsRollbacked).Count(new SessionUnitMessageSpecification(this).ToExpression());
        var reminds = query.Select(x => new
        {
            x.Id,
            RemindMeCount = x.ReminderList.Select(x => x.Message)
                .Where(d => d.Id > minMessageId)
                .Where(d =>
                    d.SenderId != x.OwnerId &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                )
                .Count(),
            RemindAllCount = x.Session.MessageList
                .Where(d => d.IsRemindAll && !d.IsRollbacked)
                .Where(d => d.Id > minMessageId)
                .Where(d =>
                    d.SenderId != x.OwnerId &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                )
                .Count(),
        })
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(d => d.RemindMeCount + d.RemindAllCount))
            .ToDictionary(x => x.Id, x => x.RemindMeCount + x.RemindAllCount)
            ;

        return reminds;
    }

    public virtual async Task<Dictionary<Guid, int>> GetFollowingCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed);

        var follows = query.Select(x => new
        {
            x.Id,
            FollowingCount = x.Session.MessageList
                .Where(d => x.OwnerFollowList.Any(d => d.DestinationId == x.Id))
                .Where(d => d.Id > minMessageId)
                .Where(d =>
                    d.SenderId != x.OwnerId &&
                    (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                )
                .Count(),
        })
            .ToDictionary(x => x.Id, x => x.FollowingCount)
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(d => d.FollowingCount))
            ;

        return follows;
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
