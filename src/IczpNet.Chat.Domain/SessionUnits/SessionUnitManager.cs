using AutoMapper.Execution;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.TextTemplates;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitManager(
    IChatObjectManager chatObjectManager,
    ISessionUnitRepository repository,
    IReadOnlyRepository<SessionUnit, Guid> sessionUnitReadOnlyRepository,
    IReadOnlyRepository<Message, long> messageReadOnlyRepository,
    IDistributedCache<List<SessionUnitCacheItem>, string> sessionUnitListCache,
    IDistributedCache<SessionUnitCacheItem, Guid> sessionUnitItemCache,
    IDistributedCache<List<SessionUnitCacheItem>, long> friendsCache,
    IDistributedCache<List<SessionUnitCacheItem>, Guid> userFriendsCache,
    IDistributedCache<string, Guid> sessionUnitCountCache,
    IChatObjectRepository chatObjectRepository,
    IMessageSender messageSender,
    IObjectMapper objectMapper,
    IUnitOfWorkManager unitOfWorkManager,
    ISessionGenerator sessionGenerator,
    ISessionManager sessionManager,
    ISessionUnitIdGenerator idGenerator) : DomainService, ISessionUnitManager
{
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected ISessionUnitRepository Repository { get; } = repository;
    /// <summary>
    /// SessionUnit 更新频繁，使用 ReadOnlyRepository 防止意外更新到数据库，引起并发冲突 --2025.03.19
    /// </summary>
    public IReadOnlyRepository<SessionUnit, Guid> SessionUnitReadOnlyRepository { get; } = sessionUnitReadOnlyRepository;
    protected IReadOnlyRepository<Message, long> MessageReadOnlyRepository { get; } = messageReadOnlyRepository;
    protected IDistributedCache<List<SessionUnitCacheItem>, string> SessionUnitListCache { get; } = sessionUnitListCache;
    public IDistributedCache<SessionUnitCacheItem, Guid> SessionUnitItemCache { get; } = sessionUnitItemCache;
    public IDistributedCache<List<SessionUnitCacheItem>, long> FriendsCache { get; } = friendsCache;
    public IDistributedCache<List<SessionUnitCacheItem>, Guid> UserFriendsCache { get; } = userFriendsCache;
    protected IDistributedCache<string, Guid> SessionUnitCountCache { get; } = sessionUnitCountCache;
    protected IFollowManager FollowManager => LazyServiceProvider.LazyGetRequiredService<IFollowManager>();
    protected IChatObjectRepository ChatObjectRepository { get; } = chatObjectRepository;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected IObjectMapper ObjectMapper { get; } = objectMapper;
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public ISessionGenerator SessionGenerator { get; } = sessionGenerator;
    public ISessionManager SessionManager { get; } = sessionManager;
    protected ISessionUnitIdGenerator IdGenerator { get; } = idGenerator;

    protected static DistributedCacheEntryOptions DistributedCacheEntryOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
    };

    protected virtual async Task<SessionUnit> SetEntityAsync(SessionUnit entity, Action<SessionUnit> action = null, bool autoSave = false)
    {
        action?.Invoke(entity);

        return await Repository.UpdateAsync(entity, autoSave: autoSave);
    }

    /// <inheritdoc />
    public virtual async Task<Guid?> FindIdAsync(Expression<Func<SessionUnit, bool>> predicate)
    {
        return (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(predicate)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefault();
        ;
    }

    /// <inheritdoc />
    public virtual Task<Guid?> FindIdAsync(long ownerId, long destinactionId)
    {
        return FindIdAsync(x => x.OwnerId == ownerId && x.DestinationId == destinactionId);
    }

    /// <inheritdoc />
    public virtual async Task<bool> IsAnyAsync(long ownerId, long destinactionId)
    {
        return await SessionUnitReadOnlyRepository.AnyAsync(x => x.OwnerId == ownerId && x.DestinationId == destinactionId);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> FindAsync(long ownerId, long destinactionId)
    {
        return FindAsync(x => x.OwnerId == ownerId && x.DestinationId == destinactionId);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> FindAsync(Expression<Func<SessionUnit, bool>> predicate)
    {
        return Repository.FindAsync(predicate);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> FindBySessionIdAsync(Guid sessionId, long ownerId)
    {
        return FindAsync(x => x.SessionId == sessionId && x.OwnerId == ownerId);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> GetAsync(Guid id, bool isReadOnly = true)
    {
        return isReadOnly ? SessionUnitReadOnlyRepository.GetAsync(id) : Repository.GetAsync(id);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitCacheItem> GetByCacheAsync(Guid id)
    {
        return SessionUnitItemCache.GetOrAddAsync(id, async () =>
        {
            var sessionUnit = await GetAsync(id);
            return ObjectMapper.Map<SessionUnit, SessionUnitCacheItem>(sessionUnit);
        }, () => DistributedCacheEntryOptions);
    }

    /// <inheritdoc />
    public virtual async Task<List<SessionUnit>> GetManyAsync(List<Guid> idList)
    {
        var result = new List<SessionUnit>();

        foreach (var id in idList)
        {
            result.Add(await GetAsync(id));
        }
        return result;
    }

    /// <inheritdoc />
    public SessionUnit Create(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> action)
    {
        return session.AddSessionUnit(Generate(
                    //idGenerator: IdGenerator,
                    session: session,
                    owner: owner,
                    destination: destination,
                    action));
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnit> CreateAsync(SessionUnit sessionUnit)
    {
        return await Repository.InsertAsync(sessionUnit, autoSave: true);
    }

    /// <inheritdoc />
    public SessionUnit Generate(Session session, ChatObject owner, ChatObject destination, Action<SessionUnitSetting> action)
    {
        return new SessionUnit(
                    idGenerator: IdGenerator,
                    session: session,
                    owner: owner,
                    destination: destination,
                    action);
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnit> CreateIfNotContainsAsync(SessionUnit sessionUnit)
    {
        var entity = await FindAsync(sessionUnit.OwnerId, sessionUnit.DestinationId.Value);

        entity ??= await Repository.InsertAsync(sessionUnit, autoSave: true);

        return entity;
    }



    /// <inheritdoc />
    public virtual Task<SessionUnit> CreateIfNotContainsAsync(
            [NotNull]
            Session session,
            [NotNull]
            ChatObject owner,
            [NotNull]
            ChatObject destination,
            Action<SessionUnitSetting> setting)
    {
        return CreateIfNotContainsAsync(Generate(session, owner, destination, setting));
    }

    /// <inheritdoc />
    public Task<SessionUnit> SetMemberNameAsync(SessionUnit entity, string memberName)
    {
        return SetEntityAsync(entity, x => x.Setting.SetMemberName(memberName));
    }

    /// <inheritdoc />
    public Task<SessionUnit> SetRenameAsync(SessionUnit entity, string rename)
    {
        return SetEntityAsync(entity, x => x.Setting.SetRename(rename));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> SetToppingAsync(SessionUnit entity, bool isTopping)
    {
        return SetEntityAsync(entity, x => x.SetTopping(isTopping));
    }
    /// <inheritdoc />
    public virtual async Task<SessionUnit> SetReadedMessageIdAsync(Guid sessionUnitId, bool isForce = false, long? messageId = null)
    {
        var entity = await GetAsync(sessionUnitId);
        return await SetReadedMessageIdAsync(entity, isForce, messageId);
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnit> SetReadedMessageIdAsync(SessionUnit entity, bool isForce = false, long? messageId = null)
    {
        var isNullOrZero = messageId == null || messageId == 0;

        var lastMessageId = isNullOrZero ? entity.LastMessageId.Value : messageId.Value;

        if (!isNullOrZero)
        {
            var message = await MessageReadOnlyRepository.GetAsync(lastMessageId);

            Assert.If(entity.SessionId != message.SessionId, $"Not in same session,messageId:{messageId}");
        }

        var counter = new SessionUnitCounterInfo()
        {
            Id = entity.Id,
            ReadedMessageId = lastMessageId,
            PublicBadge = 0,
            PrivateBadge = 0,
            RemindAllCount = 0,
            RemindMeCount = 0,
            FollowingCount = 0,
        };

        if (isForce)
        {
            counter = await GetCounterAsync(entity.Id, lastMessageId);
        }

        //await SetEntityAsync(entity, x => x.UpdateCounter(counter));
        // 更新记数器
        return await Repository.UpdateCountersync(counter);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> SetImmersedAsync(SessionUnit entity, bool isImmersed)
    {
        Assert.If(!entity.Session.IsEnableSetImmersed, "Session is disable to set immersed.");

        return SetEntityAsync(entity, x => x.Setting.SetImmersed(isImmersed));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> SetIsContactsAsync(SessionUnit entity, bool isContacts)
    {
        return SetEntityAsync(entity, x => x.Setting.SetIsContacts(isContacts));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> SetIsShowMemberNameAsync(SessionUnit entity, bool isShowMemberName)
    {
        return SetEntityAsync(entity, x => x.Setting.SetIsShowMemberName(isShowMemberName));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> RemoveAsync(SessionUnit entity)
    {
        return SetEntityAsync(entity, x => x.Setting.Remove(Clock.Now));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> KillAsync(SessionUnit entity)
    {
        return SetEntityAsync(entity, x => x.Setting.Kill(Clock.Now));
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnit> ClearMessageAsync(SessionUnit entity)
    {
        await SetReadedMessageIdAsync(entity, false);

        return await SetEntityAsync(entity, x => x.Setting.ClearMessage(Clock.Now));
    }

    /// <inheritdoc />
    public virtual Task<SessionUnit> DeleteMessageAsync(SessionUnit entity, long messageId)
    {
        throw new NotImplementedException();
    }

    protected virtual async Task<int> GetBadgeAsync(Func<IQueryable<SessionUnit>, IQueryable<SessionUnit>> queryAction)
    {
        var query = queryAction.Invoke(await SessionUnitReadOnlyRepository.GetQueryableAsync());

        var badge = query.Select(x => new
        {
            PublicBadge = x.Session.MessageList.Count(d =>
                !d.IsPrivate &&
                //!x.IsRollbacked &&
                d.SenderId != x.OwnerId &&
                (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime)),
            PrivateBadge = x.Session.MessageList.Count(d =>
                d.IsPrivate && d.ReceiverId == x.OwnerId &&
                //!x.IsRollbacked &&
                d.SenderId != x.OwnerId &&
                (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime)),
        })
        .Select(x => new
        {
            Badge = x.PublicBadge + x.PrivateBadge
        })
        .Where(x => x.Badge > 0)
        .ToList()
        .Sum(x => x.Badge);

        return badge;
    }

    /// <inheritdoc />
    public virtual async Task<Dictionary<ChatObjectTypeEnums, int>> GetTypeBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null)
    {
        var ret = (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == ownerId)
            .Where(x => x.DestinationObjectType.HasValue)
            .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed)
            .GroupBy(x => x.DestinationObjectType, (c, f) => new
            {
                DestinationObjectType = c.Value,
                Badge = f.Sum(x => x.PublicBadge),
            })
            .ToDictionary(x => x.DestinationObjectType, x => x.Badge);

        return ret;
    }

    /// <inheritdoc />
    public virtual async Task<int> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null)
    {
        return (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == ownerId)
            .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed)
            .Sum(x => x.PublicBadge);
        ;
        //---------------------- 1

        //var list = (await Repository.GetQueryableAsync()).Where(x => x.SessionUnitId == ownerId).Select(x => x.Id).ToList();

        //var count = 0;

        //foreach (var id in list)
        //{
        //    count += await GetBadgeByIdAsync(id);
        //}
        //return count;

        //---------------------- 2

        //return (await Repository.GetQueryableAsync())
        //        .Where(x => x.SessionUnitId == ownerId)
        //        .Join(await MessageReadOnlyRepository.GetQueryableAsync(), x => x.SessionId, x => x.SessionId, (unit, message) => new
        //        {
        //            unit.Setting,
        //            Message = message,
        //            // Badge = messages.Where(x => x.Id > unit.Setting.ReadedMessageId)
        //            //.Where(x => unit.Setting.HistoryFristTime == null || x.CreationTime >= unit.Setting.HistoryFristTime)
        //            //.Where(x => unit.Setting.HistoryLastTime == null || x.CreationTime < unit.Setting.HistoryLastTime)
        //            //.Where(x => unit.Setting.ClearTime == null || x.CreationTime > unit.Setting.ClearTime)
        //            //.Count()
        //        })
        //        .Where(x => x.Setting.ReadedMessageId == null || x.Message.Id > x.Setting.ReadedMessageId)
        //        .Where(x => x.Setting.HistoryFristTime == null || x.Message.CreationTime >= x.Setting.HistoryFristTime)
        //        .Where(x => x.Setting.HistoryLastTime == null || x.Message.CreationTime >= x.Setting.HistoryLastTime)
        //        .Where(x => x.Setting.ClearTime == null || x.Message.CreationTime >= x.Setting.ClearTime)
        //        .Count()
        //        ;

        //---------------------- 3

        //return await GetBadgeAsync(q =>
        //    q.Where(x => x.SessionUnitId == ownerId)
        //    .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed));
    }

    /// <inheritdoc />
    public virtual async Task<int> GetBadgeByIdAsync(Guid sessionUnitId, bool? isImmersed = null)
    {

        var entity = await SessionUnitReadOnlyRepository.GetAsync(sessionUnitId);

        var setting = entity.Setting;

        var query = (await MessageReadOnlyRepository.GetQueryableAsync())
        .Where(x => x.SessionId == entity.SessionId)
        .WhereIf(setting.ReadedMessageId.HasValue, x => x.Id > setting.ReadedMessageId)
        .WhereIf(setting.HistoryFristTime.HasValue, x => x.CreationTime >= setting.HistoryFristTime)
        .WhereIf(setting.HistoryLastTime.HasValue, x => x.CreationTime < setting.HistoryLastTime)
        .WhereIf(setting.ClearTime.HasValue, x => x.CreationTime > setting.ClearTime)
        ;
        return query.Count();

        //return GetBadgeAsync(q =>
        //    q.Where(x => x.Id == sessionUnitId)
        //    .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed));
    }

    /// <inheritdoc />
    public virtual async Task<Dictionary<Guid, int>> GetBadgeByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        var badges = (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed)
            .Select(x => new
            {
                x.Id,
                x.OwnerId,
                Messages = x.Session.MessageList.Where(d =>
                    d.Id > minMessageId &&
                    d.SenderId != x.OwnerId &&
                    (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                    (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                    (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                    (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime))
            })
            .Select(x => new
            {
                x.Id,
                PublicBadge = x.Messages.Count(d => !d.IsPrivate),
                PrivateBadge = x.Messages.Count(d => d.IsPrivate && d.ReceiverId == x.OwnerId),
            })
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(x => x.PublicBadge + x.PrivateBadge))
            .ToDictionary(x => x.Id, x => x.PrivateBadge + x.PublicBadge)
            ;

        return badges;
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnitCounterInfo> GetCounterAsync(Guid sessionUnitId, long minMessageId = 0, bool? isImmersed = null)
    {
        var entity = await SessionUnitReadOnlyRepository.GetAsync(sessionUnitId);

        var setting = entity.Setting;

        var readedMessageId = minMessageId == 0 ? setting.ReadedMessageId : minMessageId;

        var query = (await MessageReadOnlyRepository.GetQueryableAsync())
        .Where(x => x.SessionId == entity.SessionId)
        .Where(x => x.Id > readedMessageId.GetValueOrDefault())
        .WhereIf(setting.HistoryFristTime.HasValue, x => x.CreationTime >= setting.HistoryFristTime)
        .WhereIf(setting.HistoryLastTime.HasValue, x => x.CreationTime < setting.HistoryFristTime)
        .WhereIf(setting.ClearTime.HasValue, x => x.CreationTime > setting.ClearTime)
        ;

        var followingIdList = await FollowManager.GetFollowingIdListAsync(sessionUnitId);

        return new SessionUnitCounterInfo()
        {
            Id = entity.Id,
            ReadedMessageId = readedMessageId,
            PublicBadge = query.Count(),
            PrivateBadge = query.Where(x => x.IsPrivate).Count(),
            FollowingCount = query.Where(x => followingIdList.Contains(x.SenderSessionUnitId.Value)).Count(),
            RemindAllCount = query.Where(x => x.IsRemindAll && !x.IsRollbacked).Count(),
            RemindMeCount = query.Where(x => x.MessageReminderList.Any(g => g.SessionUnitId == entity.Id)).Count(),
        };
    }

    /// <inheritdoc />
    public virtual async Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        return await GetStatsByEachAsync(sessionUnitIdList, isImmersed);
    }

    /// <inheritdoc />
    protected virtual async Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsByEachAsync(List<Guid> sessionUnitIdList, bool? isImmersed = null)
    {

        var dics = new Dictionary<Guid, SessionUnitStatModel>();
        foreach (var id in sessionUnitIdList.Distinct())
        {
            var entity = await SessionUnitReadOnlyRepository.GetAsync(id);

            var setting = entity.Setting;

            var query = (await MessageReadOnlyRepository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId)
            .Where(x => x.Id > setting.ReadedMessageId)
            .WhereIf(setting.HistoryFristTime.HasValue, x => x.CreationTime >= setting.HistoryFristTime)
            .WhereIf(setting.HistoryLastTime.HasValue, x => x.CreationTime < setting.HistoryFristTime)
            .WhereIf(setting.ClearTime.HasValue, x => x.CreationTime > setting.ClearTime)
            ;

            dics.Add(entity.Id, new SessionUnitStatModel()
            {
                Id = entity.Id,
                PublicBadge = query.Count(),
                PrivateBadge = query.Where(x => x.IsPrivate).Count(),
                FollowingCount = query.Where(x => entity.FollowingList.Select(d => d.DestinationSessionUnitId).Contains(x.SenderSessionUnitId.Value)).Count(),
                RemindAllCount = query.Where(x => x.IsRemindAll && !x.IsRollbacked).Count(),
                RemindMeCount = query.Where(x => x.MessageReminderList.Any(g => g.SessionUnitId == entity.Id)).Count(),
            });

        }
        return dics;
    }

    protected virtual async Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsByLinqAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        return (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed)
            .Select(x => new
            {
                x.Id,
                x.OwnerId,
                x.FollowingList,
                Messages = x.Session.MessageList.Where(d =>
                    d.Id > minMessageId &&
                    d.SenderId != x.OwnerId &&
                    (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                    (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                    (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                    (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime))
            })
            .Select(x => new SessionUnitStatModel
            {
                Id = x.Id,
                PublicBadge = x.Messages.Count(d => !d.IsPrivate),
                PrivateBadge = x.Messages.Count(d => d.IsPrivate && d.ReceiverId == x.OwnerId),
                FollowingCount = x.Messages.Count(d => x.FollowingList.Any(d => d.DestinationSessionUnitId == x.Id)),
                RemindAllCount = x.Messages.Count(d => d.IsRemindAll && !d.IsRollbacked),
                RemindMeCount = x.Messages.Count(d => d.MessageReminderList.Any(g => g.SessionUnitId == x.Id))
            })
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(x => x.PublicBadge + x.PrivateBadge))
            .ToDictionary(x => x.Id)
            ;
    }

    /// <inheritdoc />
    public virtual async Task<Dictionary<Guid, int>> GetReminderCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        var query = (await Repository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed);
        //return ReminderList.AsQueryable().Select(x => x.Message).Where(x => !x.IsRollbacked).Count(new SessionUnitMessageSpecification(this).ToExpression());
        var reminds = query.Select(x => new
        {
            x.Id,
            RemindMeCount = x.ReminderList.Select(x => x.Message)
                .Where(d => d.Id > minMessageId)
                .Where(d =>
                    d.SenderId != x.OwnerId &&
                    (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                    (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                    (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                    (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime)
                )
                .Count(),
            RemindAllCount = x.Session.MessageList
                .Where(d => d.IsRemindAll && !d.IsRollbacked)
                .Where(d => d.Id > minMessageId)
                .Where(d =>
                    d.SenderId != x.OwnerId &&
                    (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                    (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                    (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                    (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime)
                )
                .Count(),
        })
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(x => x.RemindMeCount + x.RemindAllCount))
            .ToDictionary(x => x.Id, x => x.RemindMeCount + x.RemindAllCount)
            ;

        return reminds;
    }

    /// <inheritdoc />
    public virtual async Task<Dictionary<Guid, int>> GetFollowingCountByIdAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
    {
        var query = (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(x => sessionUnitIdList.Contains(x.Id))
            .WhereIf(isImmersed.HasValue, x => x.Setting.IsImmersed == isImmersed);

        var follows = query.Select(x => new
        {
            x.Id,
            FollowingCount = x.Session.MessageList
                .Where(d => x.FollowingList.Any(d => d.DestinationSessionUnitId == x.Id))
                .Where(d => d.Id > minMessageId)
                .Where(d =>
                    d.SenderId != x.OwnerId &&
                    (x.Setting.ReadedMessageId == null || d.Id > x.Setting.ReadedMessageId) &&
                    (!x.Setting.HistoryFristTime.HasValue || d.CreationTime > x.Setting.HistoryFristTime) &&
                    (!x.Setting.HistoryLastTime.HasValue || d.CreationTime < x.Setting.HistoryLastTime) &&
                    (!x.Setting.ClearTime.HasValue || d.CreationTime > x.Setting.ClearTime)
                )
                .Count(),
        })
            .ToDictionary(x => x.Id, x => x.FollowingCount)
            //.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.Sum(x => x.FollowingCount))
            ;

        return follows;
    }

    /// <inheritdoc />
    public virtual async Task<int> GetCountBySessionIdAsync(Guid sessionId)
    {
        var value = await SessionUnitCountCache.GetOrAddAsync(sessionId, async () =>
        {
            var count = (await SessionUnitReadOnlyRepository.GetQueryableAsync())
                .Where(x => x.SessionId == sessionId)
                .Where(SessionUnit.GetActivePredicate(Clock.Now))
                .Count();
            return count.ToString();
        });
        return int.Parse(value);
    }

    /// <inheritdoc />
    public virtual async Task<int> GetCountByOwnerIdAsync(long ownerId)
    {
        return (await SessionUnitReadOnlyRepository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                .Where(SessionUnit.GetActivePredicate(Clock.Now))
                .Count();
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetCacheListAsync(string sessionUnitCachKey)
    {
        return SessionUnitListCache.GetAsync(sessionUnitCachKey);
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetCacheListBySessionIdAsync(Guid sessionId)
    {
        return SessionUnitListCache.GetAsync($"{new SessionUnitCacheKey(sessionId)}");
    }

    /// <inheritdoc />
    public virtual async Task<SessionUnitCacheItem> GetCacheItemAsync(Guid sessionId, Guid sessionUnitId)
    {
        var items = await GetCacheListBySessionIdAsync(sessionId);

        return items?.FirstOrDefault(x => x.Id == sessionUnitId);
    }

    /// <inheritdoc />
    public virtual Task<SessionUnitCacheItem> GetCacheItemAsync(SessionUnit sessionUnit)
    {
        return GetCacheItemAsync(sessionUnit.SessionId.Value, sessionUnit.Id);
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(Guid sessionId)
    {
        return SessionUnitListCache.GetOrAddAsync($"{new SessionUnitCacheKey(sessionId)}", () => GetListBySessionIdAsync(sessionId));
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetOrAddCacheListAsync(List<SessionUnit> sessionUnitList)
    {
        return SessionUnitListCache.GetOrAddAsync($"{new SessionUnitCacheKey(sessionUnitList.Select(x => x.Id))}", () =>
        {
            return Task.FromResult(ToCacheItem(sessionUnitList.AsQueryable()));
        });
    }

    /// <inheritdoc />
    public virtual async Task SetCacheListBySessionIdAsync(Guid sessionId, List<SessionUnitCacheItem> sessionUnitList)
    {
        await SetCacheListAsync($"{new SessionUnitCacheKey(sessionId)}", sessionUnitList);
    }

    public virtual Task<string> GetCacheKeyByMessageAsync(Message message)
    {
        var cacheKey = message.IsPrivateMessage()
          ? new SessionUnitCacheKey([message.SenderSessionUnitId.Value, message.ReceiverSessionUnitId.Value])
          : new SessionUnitCacheKey(message.SessionId.Value);

        return Task.FromResult($"{cacheKey}");
    }
    /// <inheritdoc />
    public virtual async Task<List<SessionUnitCacheItem>> GetOrAddByMessageAsync(Message message)
    {
        var cacheKey = await GetCacheKeyByMessageAsync(message);

        return await SessionUnitListCache.GetOrAddAsync($"{cacheKey}", async () =>
        {
            if (message.IsPrivateMessage())
            {
                var sessionUnitList = new List<SessionUnit>() { message.SenderSessionUnit };

                if (message.ReceiverSessionUnit == null && message.ReceiverSessionUnitId.HasValue)
                {
                    sessionUnitList.Add(await SessionUnitReadOnlyRepository.GetAsync(message.ReceiverSessionUnitId.Value));
                }
                return ToCacheItem(sessionUnitList.AsQueryable());
            }
            return await GetListBySessionIdAsync(message.SessionId.Value);
        }, () => DistributedCacheEntryOptions);
    }

    /// <inheritdoc />
    public virtual async Task SetCacheListAsync(string cacheKey, List<SessionUnitCacheItem> sessionUnitList, DistributedCacheEntryOptions options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        await SessionUnitListCache.SetAsync(cacheKey, sessionUnitList, options, hideErrors, considerUow, token);
    }

    private static List<SessionUnitCacheItem> ToCacheItem(IQueryable<SessionUnit> qurey)
    {
        return [.. qurey.Select(x => new SessionUnitCacheItem()
        {
            Id = x.Id,
            //UserId = x.UserId,
            SessionId = x.SessionId,
            OwnerId = x.OwnerId,
            OwnerObjectType = x.OwnerObjectType,
            DestinationId = x.DestinationId,
            DestinationObjectType = x.DestinationObjectType,
            IsPublic = x.Setting.IsPublic,
            IsStatic = x.Setting.IsStatic,
            IsVisible = x.Setting.IsVisible,
            IsEnabled = x.Setting.IsEnabled,
            //ReadedMessageId = x.Setting.ReadedMessageId,
            //PublicBadge = x.PublicBadge,
            //PrivateBadge = x.PrivateBadge,
            //RemindAllCount = x.RemindAllCount,
            //RemindMeCount = x.RemindMeCount,
            //FollowingCount = x.FollowingCount,
            //LastMessageId = x.LastMessageId,
            //Ticks = x.Ticks,
        })];
    }
    /// <inheritdoc />
    public virtual async Task<List<SessionUnitCacheItem>> GetListBySessionIdAsync(Guid sessionId)
    {
        var list = ToCacheItem((await SessionUnitReadOnlyRepository.GetQueryableAsync())
                .Where(SessionUnit.GetActivePredicate(Clock.Now))
                .Where(x => x.SessionId == sessionId)
            );

        await SessionUnitCountCache.SetAsync(sessionId, list.Where(x => x.IsPublic).Count().ToString());

        return list;
    }
    /// <inheritdoc />
    public virtual async Task<List<SessionUnitCacheItem>> GetListByUserIdAsync(Guid userId)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(userId);
        var list = ToCacheItem((await SessionUnitReadOnlyRepository.GetQueryableAsync())
                .Where(SessionUnit.GetActivePredicate(Clock.Now))
                .Where(x => chatObjectIdList.Contains(x.OwnerId))
            );
        return list;
    }

    /// <inheritdoc />
    public virtual async Task<List<SessionUnitCacheItem>> GetListByOwnerIdAsync(long ownerId)
    {
        var list = ToCacheItem((await SessionUnitReadOnlyRepository.GetQueryableAsync())
                .Where(SessionUnit.GetActivePredicate(Clock.Now))
                .Where(x => x.OwnerId == ownerId)
            );
        return list;
    }

    /// <inheritdoc />
    public virtual async Task RemoveCacheListBySessionIdAsync(Guid sessionId)
    {
        await SessionUnitListCache.RemoveAsync($"{new SessionUnitCacheKey(sessionId)}");
    }

    /// <inheritdoc />
    private async Task<IQueryable<SessionUnit>> GetOwnerQueryableAsync(long ownerId, List<ChatObjectTypeEnums> destinationObjectTypeList = null)
    {
        return (await SessionUnitReadOnlyRepository.GetQueryableAsync())
              .Where(x => x.OwnerId.Equals(ownerId) && !x.Setting.IsKilled && x.Setting.IsEnabled)
              .WhereIf(destinationObjectTypeList.IsAny(), x => destinationObjectTypeList.Contains(x.DestinationObjectType.Value));

    }

    /// <inheritdoc />
    public virtual async Task<IQueryable<SessionUnit>> GetSameSessionQeuryableAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var targetSessionIdList = (await GetOwnerQueryableAsync(targetChatObjectId, chatObjectTypeList))
            .Select(x => x.SessionId);

        var sourceQuery = (await GetOwnerQueryableAsync(sourceChatObjectId, chatObjectTypeList))
            .Where(x => targetSessionIdList.Contains(x.SessionId))
            ;

        return sourceQuery;
    }

    /// <inheritdoc />
    public virtual async Task<int> GetSameSessionCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var query = await GetSameSessionQeuryableAsync(sourceChatObjectId, targetChatObjectId, chatObjectTypeList);

        return await AsyncExecuter.CountAsync(query);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual async Task<int> GetSameDestinationCountAsync(long sourceChatObjectId, long targetChatObjectId, List<ChatObjectTypeEnums> chatObjectTypeList = null)
    {
        var query = await GetSameDestinationQeuryableAsync(sourceChatObjectId, targetChatObjectId, chatObjectTypeList);

        return await AsyncExecuter.CountAsync(query);
    }


    /// <inheritdoc />
    public virtual async Task<int> IncrementFollowingCountAsync(SessionUnit senderSessionUnit, Message message)
    {
        var ownerSessionUnitIdList = await FollowManager.GetFollowerIdListAsync(senderSessionUnit.Id);

        if (ownerSessionUnitIdList.Count != 0)
        {
            ownerSessionUnitIdList.Remove(senderSessionUnit.Id);

            return await Repository.IncrementFollowingCountAsync(senderSessionUnit.SessionId.Value, message.CreationTime, destinationSessionUnitIdList: ownerSessionUnitIdList);
        }
        return 0;
    }

    //protected virtual async Task UpdateCacheItemsAsync(SessionUnit senderSessionUnit, Func<List<SessionUnitCacheItem>, bool> action)
    //{
    //    var stopwatch = Stopwatch.StartNew();

    //    var sessionUnitList = await GetOrAddCacheListAsync(senderSessionUnit.SessionId.Value);

    //    if (action.Invoke(sessionUnitList))
    //    {
    //        await SetCacheListBySessionIdAsync(senderSessionUnit.SessionId.Value, sessionUnitList);
    //    }

    //    stopwatch.Stop();

    //    Logger.LogInformation($"UpdateCacheItems stopwatch: {stopwatch.ElapsedMilliseconds}ms.");
    //}

    ///// <inheritdoc />
    //public virtual async Task<int> UpdateCachesAsync(SessionUnit senderSessionUnit, Message message)
    //{
    //    int count = 0;

    //    await UpdateCacheItemsAsync(senderSessionUnit, items =>
    //    {
    //        var self = items.FirstOrDefault(x => x.Id == senderSessionUnit.Id);

    //        if (self != null)
    //        {
    //            self.LastMessageId = message.Id;
    //        }

    //        var others = items.Where(x => x.Id != senderSessionUnit.Id).ToList();

    //        foreach (var item in others)
    //        {
    //            //item.RemindMeCount++;
    //            //item.FollowingCount++;
    //            item.PublicBadge++;
    //            item.RemindAllCount++;
    //            item.LastMessageId = message.Id;
    //        }
    //        count = others.Count;

    //        return true;
    //    });

    //    Logger.LogInformation($"BatchUpdateCacheAsync:{count}");

    //    return count;
    //}

    /// <inheritdoc />
    public virtual async Task<int> BatchUpdateAsync(SessionUnit senderSessionUnit, Message message)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await Repository.IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(
               sessionId: senderSessionUnit.SessionId.Value,
               lastMessageId: message.Id,
               messageCreationTime: message.CreationTime,
               senderSessionUnit.Id,
               isRemindAll: message.IsRemindAll
               );

        stopwatch.Stop();

        Logger.LogInformation($"BatchUpdateLastMessageIdAndPublicBadgeAndRemindAllCount:{result}, stopwatch: {stopwatch.ElapsedMilliseconds}ms.");

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<List<Guid>> GetIdListByNameAsync(Guid sessionId, List<string> nameList)
    {
        var chatObjectIds = (await ChatObjectRepository.GetQueryableAsync())
            .Where(x => nameList.Contains(x.Name))
            .Select(x => x.Id)
            ;

        return (await SessionUnitReadOnlyRepository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionId)
            .Where(x => nameList.Contains(x.Setting.MemberName) || chatObjectIds.Contains(x.OwnerId))
            .Where(SessionUnit.GetActivePredicate(null))
            .Select(x => x.Id)
            .ToList();
    }

    /// <inheritdoc/>
    public virtual async Task<int> IncremenetAsync(SessionUnitIncrementJobArgs args)
    {
        Logger.LogInformation($"Incremenet args:{args},starting.....................................");

        var stopwatch = Stopwatch.StartNew();

        var counter = new List<int>();

        if (args.IsPrivate)
        {
            var count = await Repository.IncrementPrivateBadgeAndUpdateLastMessageIdAsync(
                sessionId: args.SessionId,
                lastMessageId: args.LastMessageId,
                messageCreationTime: args.MessageCreationTime,
                senderSessionUnitId: args.SenderSessionUnitId,
                destinationSessionUnitIdList: args.PrivateBadgeSessionUnitIdList);

            Logger.LogInformation($"IncrementPrivateBadgeAndUpdateLastMessageId count:{count}");

            counter.Add(count);
        }
        else
        {
            var count = await Repository.IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(
                sessionId: args.SessionId,
                lastMessageId: args.LastMessageId,
                messageCreationTime: args.MessageCreationTime,
                senderSessionUnitId: args.SenderSessionUnitId,
                isRemindAll: args.IsRemindAll);

            Logger.LogInformation($"IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync count:{count}");

            counter.Add(count);
        }

        if (args.RemindSessionUnitIdList.IsAny())
        {
            var count = await Repository.IncrementRemindMeCountAsync(
                sessionId: args.SessionId,
                messageCreationTime: args.MessageCreationTime,
                destinationSessionUnitIdList: args.RemindSessionUnitIdList);

            Logger.LogInformation($"IncrementRemindMeCountAsync count:{count}");

            counter.Add(count);
        }

        if (args.FollowingSessionUnitIdList.IsAny())
        {
            var count = await Repository.IncrementFollowingCountAsync(
                sessionId: args.SessionId,
                messageCreationTime: args.MessageCreationTime,
                destinationSessionUnitIdList: args.FollowingSessionUnitIdList);

            Logger.LogInformation($"IncrementFollowingCountAsync count:{count}");

            counter.Add(count);
        }

        stopwatch.Stop();

        var totalCount = counter.Sum();

        Logger.LogInformation($"Incremenet totalCount:{totalCount}, stopwatch: {stopwatch.ElapsedMilliseconds}ms.");

        return totalCount;
    }

    /// <inheritdoc />
    [Obsolete($"Move to {nameof(ISessionUnitSettingManager.SetMuteExpireTimeAsync)}")]
    public virtual async Task<DateTime?> SetMuteExpireTimeAsync(SessionUnit muterSessionUnit, DateTime? muteExpireTime, SessionUnit setterSessionUnit, bool isSendMessage)
    {
        Assert.If(muterSessionUnit.Setting.IsCreator, $"Creator can't be mute.");

        var allowList = new List<ChatObjectTypeEnums?> { ChatObjectTypeEnums.Room, ChatObjectTypeEnums.Square, ChatObjectTypeEnums.Official, ChatObjectTypeEnums.Subscription, };

        Assert.If(!allowList.Contains(muterSessionUnit.DestinationObjectType), $"DestinationObjectType '{muterSessionUnit.DestinationObjectType}' can't be mute.");

        await SetEntityAsync(muterSessionUnit, x => x.Setting.SetMuteExpireTime(muteExpireTime));

        if (!isSendMessage)
        {
            return muteExpireTime;
        }

        if (setterSessionUnit == null)
        {
            Logger.LogWarning($"SetMuteExpireTime send message fial,SessionUnitId={muterSessionUnit.Id}");
            return muteExpireTime;
        }

        var timeSpan = muteExpireTime - Clock.Now;

        var isMuted = timeSpan.HasValue && timeSpan.Value.Milliseconds > 0;

        //sendMessage
        await MessageSender.SendCmdAsync(setterSessionUnit, new MessageInput<CmdContentInfo>()
        {
            Content = new CmdContentInfo()
            {
                Text = new TextTemplate(isMuted ? "'{muteObject}' 被禁言 {Minutes} 分钟" : "'{muteObject}' 被取消禁言")
                        .WithData("muteObject", new SessionUnitTextTemplate(muterSessionUnit))
                        .WithData("minutes", timeSpan?.Minutes)
                        .ToString(),
            }
        });

        return muteExpireTime;
    }

    /// <inheritdoc />
    [Obsolete($"Move to {nameof(ISessionUnitSettingManager.SetMuteExpireTimeAsync)}")]
    public virtual async Task<DateTime?> SetMuteExpireTimeAsync(SessionUnit muterSessionUnit, DateTime? muteExpireTime)
    {
        var setterSessionUnit = await SessionUnitReadOnlyRepository.FirstOrDefaultAsync(x => x.SessionId == muterSessionUnit.SessionId && x.IsStatic && !x.IsPublic && x.Id != muterSessionUnit.Id);

        return await SetMuteExpireTimeAsync(muterSessionUnit, muteExpireTime, setterSessionUnit, setterSessionUnit != null);
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetUserFriendsAsync(Guid userId)
    {
        return UserFriendsCache.GetOrAddAsync(userId, () => GetListByUserIdAsync(userId));
    }

    /// <inheritdoc />
    public virtual Task<List<SessionUnitCacheItem>> GetFriendsAsync(long chatObjectId)
    {
        return FriendsCache.GetOrAddAsync(chatObjectId, () => GetListByOwnerIdAsync(chatObjectId));
    }

    public virtual async Task<List<SessionUnit>> GenerateDefaultSessionByChatObjectAsync(ChatObject userChatObject)
    {
        var codeList = new List<string>() { ChatConsts.PrivateAssistant, ChatConsts.Notify, ChatConsts.News, ChatConsts.GroupAssistant };

        if (codeList.Contains(userChatObject.Code))
        {
            Logger.LogWarning($"{nameof(GenerateDefaultSessionByChatObjectAsync)}:不需要创建默认会话");
            return [];
        }
        var a = await GenerateNotifySessionAsync(userChatObject);
        var b = await GenerateNewsSessionAsync(userChatObject);
        return [.. a, .. b];
    }

    protected virtual async Task<List<SessionUnit>> GenerateNotifySessionAsync(ChatObject userChatObject)
    {
        var notifyChatObject = await ChatObjectManager.GetOrAddNotifyAsync();

        //创建会话
        var session = await SessionGenerator.MakeAsync(notifyChatObject, userChatObject);

        session.SetOwner(notifyChatObject);
        //创建通知单元
        var notifySessionUnit = await CreateIfNotContainsAsync(
               session: session,
               owner: notifyChatObject,
               destination: userChatObject,
               x =>
               {
                   x.IsPublic = true;
                   x.IsStatic = true;
                   x.SetIsCreator(true);
                   x.JoinWay = JoinWays.Creator;
                   x.IsInputEnabled = true;
               });

        var isAny = await IsAnyAsync(userChatObject.Id, notifyChatObject.Id);

        //创建用户单元
        var userSessionUnit = await CreateIfNotContainsAsync(
                session: session,
                owner: userChatObject,
                destination: notifyChatObject,
                x =>
                {
                    x.IsStatic = true;
                    x.IsPublic = true;
                    x.JoinWay = JoinWays.AutoJoin;
                    x.InviterId = notifySessionUnit?.Id;
                    x.IsInputEnabled = false;
                });

        if (!isAny)
        {
            //第一次 才发送通知
            await MessageSender.SendCmdAsync(notifySessionUnit, new MessageInput<CmdContentInfo>()
            {
                Content = new CmdContentInfo()
                {
                    Cmd = MessageKeyNames.CreatedUser,
                    Text = new TextTemplate("欢迎 {userSessionUnit} 加入")
                            .WithData("userSessionUnit", new SessionUnitTextTemplate(userSessionUnit))
                            .ToString(),
                }
            });
        }

        return [notifySessionUnit, userSessionUnit];
    }

    protected virtual async Task<List<SessionUnit>> GenerateNewsSessionAsync(ChatObject userChatObject)
    {
        var newsChatObject = await ChatObjectManager.GetOrAddNewsAsync();

        //创建新闻单元
        var newsSessionUnit = await FindAsync(newsChatObject.Id, newsChatObject.Id);

        if (newsSessionUnit == null)
        {
            //创建会话
            var session = await SessionGenerator.MakeAsync(newsChatObject, newsChatObject);
            newsSessionUnit = Generate(
                   session: session,
                   owner: newsChatObject,
                   destination: newsChatObject,
                   x =>
                   {
                       x.IsPublic = true;
                       x.IsStatic = true;
                       x.SetIsCreator(true);
                       x.JoinWay = JoinWays.Creator;
                       x.IsInputEnabled = true;
                   });
        }

        //创建用户单元
        var userSessionUnit = Generate(
                session: newsSessionUnit.Session,
                owner: userChatObject,
                destination: newsChatObject,
                x =>
                {
                    x.IsStatic = true;
                    x.IsPublic = true;
                    x.JoinWay = JoinWays.AutoJoin;
                    x.InviterId = newsSessionUnit?.Id;
                    x.IsInputEnabled = false;
                });

        return [newsSessionUnit, userSessionUnit];
    }
}
