using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionServices;

public class SessionUnitAppService : ChatAppService, ISessionUnitAppService
{
    protected override string GetListPolicyName { get; set; }
    protected override string GetPolicyName { get; set; }
    protected virtual string SetRenamePolicyName { get; set; }
    protected virtual string SetMemberNamePolicyName { get; set; }
    protected virtual string GetDetailPolicyName { get; set; }
    protected virtual string SetReadedPolicyName { get; set; }
    protected virtual string SetReadedManyPolicyName { get; set; }
    protected virtual string SetImmersedPolicyName { get; set; }
    protected virtual string RemoveSessionPolicyName { get; set; }
    protected virtual string ClearMessagePolicyName { get; set; }
    protected virtual string DeleteMessagePolicyName { get; set; }

    protected IRepository<Session, Guid> SessionRepository { get; }
    protected ISessionUnitRepository Repository { get; }
    protected IMessageRepository MessageRepository { get; }
    protected ISessionManager SessionManager { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionGenerator SessionGenerator { get; }
    protected IChatObjectManager ChatObjectManager { get; }
    protected IReadedRecorderManager ReadedRecorderManager { get; }
    protected IOpenedRecorderManager OpenedRecorderManager { get; }
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; }
    protected IFollowManager FollowManager { get; }
    protected IRepository<SessionUnitCounter> SessionUnitCounterRepository { get; }
    protected IRepository<EntryName, Guid> EntryNameRepository { get; }
    protected IRepository<EntryValue, Guid> EntryValueRepository { get; }
    public SessionUnitAppService(
        ISessionManager sessionManager,
        ISessionGenerator sessionGenerator,
        IRepository<Session, Guid> sessionRepository,
        IMessageRepository messageRepository,
        ISessionUnitRepository repository,
        ISessionUnitManager sessionUnitManager,
        IChatObjectManager chatObjectManager,
        IReadedRecorderManager readedRecorderManager,
        IOpenedRecorderManager openedRecorderManager,
        IFavoritedRecorderManager favoriteManager,
        IFollowManager followManager,
        IRepository<SessionUnitCounter> sessionUnitCounterRepository,
        IRepository<EntryName, Guid> entryNameRepository,
        IRepository<EntryValue, Guid> entryValueRepository)
    {
        SessionManager = sessionManager;
        SessionGenerator = sessionGenerator;
        SessionRepository = sessionRepository;
        MessageRepository = messageRepository;
        Repository = repository;
        SessionUnitManager = sessionUnitManager;
        ChatObjectManager = chatObjectManager;
        ReadedRecorderManager = readedRecorderManager;
        OpenedRecorderManager = openedRecorderManager;
        FavoritedRecorderManager = favoriteManager;
        FollowManager = followManager;
        SessionUnitCounterRepository = sessionUnitCounterRepository;
        EntryNameRepository = entryNameRepository;
        EntryValueRepository = entryValueRepository;
    }

    /// <inheritdoc/>
    protected override Task CheckPolicyAsync(string policyName)
    {
        return base.CheckPolicyAsync(policyName);
    }


    /// <inheritdoc/>
    protected virtual async Task<SessionUnit> GetEntityAsync(Guid id, bool checkIsKilled = true)
    {
        var entity = await Repository.GetAsync(id);

        Assert.If(checkIsKilled && entity.Setting.IsKilled, "已经删除的会话单元!");

        return entity;
    }

    /// <inheritdoc/>
    protected virtual async Task<IQueryable<SessionUnit>> GetQueryAsync(SessionUnitGetListInput input)
    {


        //return from a in (await Repository.GetQueryableAsync())
        //       join b in await SessionUnitCounterRepository.GetQueryableAsync() on a.Id equals b.SessionUnitId
        //       select a;


        return (await Repository.GetQueryableAsync())
            //.Include(x => x.Setting)
            //.Where(x => x.Counter != null)
            //.Select(x => x.Counter.SessionUnit)
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.DestinationObjectType == input.DestinationObjectType)
            .WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            .WhereIf(input.IsCreator.HasValue, x => x.Setting.IsCreator == input.IsCreator)
            .WhereIf(input.MinMessageId.HasValue, x => x.LastMessageId > input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.LastMessageId <= input.MaxMessageId)
            .WhereIf(input.IsTopping == true, x => x.Sorting > 0)
            .WhereIf(input.IsTopping == false, x => x.Sorting == 0)
            .WhereIf(input.IsCantacts.HasValue, x => x.Setting.IsCantacts == input.IsCantacts)
            .WhereIf(input.IsImmersed.HasValue, x => x.Setting.IsImmersed == input.IsImmersed)
            .WhereIf(input.IsBadge.HasValue, x => x.PublicBadge > 0 || x.PrivateBadge > 0)
            .WhereIf(input.IsRemind.HasValue, x => x.RemindAllCount > 0 || x.RemindMeCount > 0)
            .WhereIf(input.IsFollowing.HasValue, x => x.FollowingCount > 0)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;
    }

    /// <inheritdoc/>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadCommitted)]
    public virtual async Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyAsync(GetListPolicyName);

        var query = await GetQueryAsync(input);

        return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
            query,
            input,
            x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId),
            async entities =>
            {
                if (input.IsRealStat == true)
                {
                    var minMessageId = input.MinMessageId.GetValueOrDefault();

                    var idList = entities.Select(x => x.Id).ToList();

                    var stats = await SessionUnitManager.GetStatsAsync(idList, minMessageId);

                    foreach (var e in entities)
                    {
                        if (stats.TryGetValue(e.Id, out SessionUnitStatModel stat))
                        {
                            e.SetBadge(stat.PublicBadge + stat.PrivateBadge);
                        }
                    }
                }

                return entities;
            });
    }

    /// <inheritdoc/>
    [HttpGet]
    public virtual Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> idList, long minMessageId)
    {
        return SessionUnitManager.GetStatsAsync(idList, minMessageId);
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListDestinationAsync(Guid id, SessionUnitGetListDestinationInput input)
    {
        var entity = await Repository.GetAsync(id);

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled)
            .WhereIf(input.IsKilled.HasValue, x => x.Setting.IsKilled == input.IsKilled)
            .WhereIf(input.IsStatic.HasValue, x => x.Setting.IsStatic == input.IsStatic)
            .WhereIf(input.IsPublic.HasValue, x => x.Setting.IsPublic == input.IsPublic)
            .WhereIf(input.OwnerIdList.IsAny(), x => input.OwnerIdList.Contains(x.OwnerId))
            .WhereIf(input.OwnerTypeList.IsAny(), x => input.OwnerTypeList.Contains(x.Owner.ObjectType.Value))
            .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(x => x.SessionTagId == input.TagId))
            .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(x => x.SessionRoleId == input.RoleId))
            .WhereIf(!input.JoinWay.IsEmpty(), x => x.Setting.JoinWay == input.JoinWay)
            .WhereIf(!input.InviterId.IsEmpty(), x => x.Setting.InviterId == input.InviterId)
            //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Owner.Name.Contains(input.Keyword))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
            ;

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input, q => q.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDisplayName>> GetListDestinationNamesAsync(Guid id, BaseGetListInput input)
    {
        var entity = await Repository.GetAsync(id);

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId && x.Setting.IsEnabled)
            .Where(SessionUnit.GetActivePredicate())
            .Select(x => new SessionUnitDisplayName
            {
                Id = x.Id,
                DisplayName = !string.IsNullOrEmpty(x.Setting.MemberName) ? x.Setting.MemberName : x.Setting.Rename,
            })
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.DisplayName.Contains(input.Keyword));

        return await GetPagedListAsync<SessionUnitDisplayName, SessionUnitDisplayName>(query, input);
    }

    /// <inheritdoc/>
    [HttpGet]
    public virtual async Task<SessionUnitOwnerDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(GetPolicyName);

        var entity = await GetEntityAsync(id);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpGet]
    public virtual async Task<SessionUnitOwnerDetailDto> GetDetailAsync(Guid id)
    {
        await CheckPolicyAsync(GetDetailPolicyName);

        var entity = await GetEntityAsync(id);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDetailDto>(entity);
    }

    /// <inheritdoc/>
    [HttpGet]
    public virtual async Task<SessionUnitDestinationDto> GetDestinationAsync(Guid id, Guid destinationId)
    {
        var destinationEntity = await GetEntityAsync(destinationId);

        var selfEntity = await GetEntityAsync(id);

        Assert.If(selfEntity.SessionId != destinationEntity.SessionId, $"Not in the same session");

        return await MapToDestinationDtoAsync(destinationEntity);
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetListSameDestinationAsync(SessionUnitGetListSameDestinationInput input)
    {
        var query = (await SessionUnitManager.GetSameDestinationQeuryableAsync(input.SourceId, input.TargetId, input.ObjectTypeList))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Destination.Name.Contains(input.Keyword))
            ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input, x => x.OrderByDescending(x => x.Id));
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDto>> GetListSameSessionAsync(SessionUnitGetListSameSessionInput input)
    {
        var query = (await SessionUnitManager.GetSameSessionQeuryableAsync(input.SourceId, input.TargetId, input.ObjectTypeList))
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Destination.Name.Contains(input.Keyword))
            ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input, x => x.OrderByDescending(x => x.Id));
    }

    /// <inheritdoc/>
    [HttpGet]
    public Task<int> GetSameSessionCountAsync(long sourceId, long targetId, List<ChatObjectTypeEnums> objectTypeList)
    {
        return SessionUnitManager.GetSameSessionCountAsync(sourceId, targetId, objectTypeList);
    }

    /// <inheritdoc/>
    [HttpGet]
    public Task<int> GetSameDestinationCountAsync(long sourceId, long targetId, List<ChatObjectTypeEnums> objectTypeList)
    {
        return SessionUnitManager.GetSameDestinationCountAsync(sourceId, targetId, objectTypeList);
    }

    /// <inheritdoc/>
    protected virtual Task<SessionUnitOwnerDto> MapToDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(entity));
    }

    /// <inheritdoc/>
    protected virtual Task<SessionUnitDestinationDto> MapToDestinationDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDestinationDto>(entity));
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetMemberNameAsync(Guid id, string memberName)
    {
        await CheckPolicyAsync(SetMemberNamePolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetMemberNameAsync(entity, memberName);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetRenameAsync(Guid id, string rename)
    {
        await CheckPolicyAsync(SetRenamePolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetRenameAsync(entity, rename);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetToppingAsync(Guid id, bool isTopping)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetToppingAsync(entity, isTopping);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetReadedMessageIdAsync(Guid id, bool isForce = false, long? messageId = null)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetReadedMessageIdAsync(entity, isForce, messageId);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetImmersedAsync(Guid id, bool isImmersed)
    {
        await CheckPolicyAsync(SetImmersedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetImmersedAsync(entity, isImmersed);

        return await MapToDtoAsync(entity);
    }


    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> RemoveAsync(Guid id)
    {
        await CheckPolicyAsync(RemoveSessionPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> KillAsync(Guid id)
    {
        var entity = await GetEntityAsync(id);

        await SessionUnitManager.KillAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ClearMessageAsync(Guid id)
    {
        await CheckPolicyAsync(ClearMessagePolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.ClearMessageAsync(entity);

        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> DeleteMessageAsync(Guid id, long messageId)
    {
        await CheckPolicyAsync(DeleteMessagePolicyName);

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<PagedResultDto<MessageOwnerDto>> GetListMessagesAsync(Guid id, SessionUnitGetMessageListInput input)
    {
        var entity = await GetEntityAsync(id);

        //Assert.NotNull(entity.Session, "session is null");

        var settting = entity.Setting;

        var followingIdList = await FollowManager.GetFollowingIdListAsync(id);

        var query = (await MessageRepository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId)
            .Where(x => !x.IsPrivate || (x.IsPrivate && (x.SenderId == entity.OwnerId || x.ReceiverId == entity.OwnerId)))
            .WhereIf(settting.HistoryFristTime.HasValue, x => x.CreationTime >= settting.HistoryFristTime)
            .WhereIf(settting.HistoryLastTime.HasValue, x => x.CreationTime < settting.HistoryFristTime)
            .WhereIf(settting.ClearTime.HasValue, x => x.CreationTime > settting.ClearTime)
            .WhereIf(input.MessageType.HasValue, x => x.MessageType == input.MessageType)
            .WhereIf(input.IsFollowed.HasValue, x => followingIdList.Contains(x.SessionUnitId.Value))
            .WhereIf(input.IsRemind == true, x => x.IsRemindAll || x.MessageReminderList.Any(x => x.SessionUnitId == id))
            .WhereIf(input.SenderId.HasValue, x => x.SenderId == input.SenderId)
            .WhereIf(input.MinMessageId.HasValue, x => x.Id > input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.Id <= input.MaxMessageId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TextContentList.Any(d => d.Text.Contains(input.Keyword)))
            ;

        return await GetPagedListAsync<Message, MessageOwnerDto>(query, input,
            x => x.OrderByDescending(x => x.Id),
            async entities =>
            {
                foreach (var e in entities)
                {
                    e.IsReaded = await ReadedRecorderManager.IsAnyAsync(id, e.Id);
                    e.IsOpened = await OpenedRecorderManager.IsAnyAsync(id, e.Id);
                    e.IsFavorited = await FavoritedRecorderManager.IsAnyAsync(id, e.Id);
                    e.IsFollowing = followingIdList.Contains(e.SessionUnitId.Value);
                }
                //await Task.CompletedTask;
                return entities;
            });
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<MessageDto> GetMessageAsync(Guid id, long messageId)
    {
        var entity = await GetEntityAsync(id);

        //var message = entity.Session.MessageList.FirstOrDefault(x => x.Id == messageId);

        var message = await MessageRepository.FindAsync(messageId);

        Assert.NotNull(message, "消息不存在!");

        Assert.If(message.IsRollbacked, "消息已撤回!");

        //...是否包含在哪个聊天记录里，是否包含在引用消息里
        //...以下待测试...

        var isCanRead = (await MessageRepository.GetQueryableAsync())
            //本条消息 || 引用这个消息的 || 包含在聊天记录里的
            .Where(x => x.Id == messageId || x.QuotedMessageList.Any(d => d.Id == messageId) || x.HistoryMessageList.Any(x => x.MessageId == messageId))
            .Select(x => x.Session)
            .Any(x => x.UnitList.Any(d => d.OwnerId == entity.OwnerId))
            ;

        Assert.If(!isCanRead, "非法访问!");

        return ObjectMapper.Map<Message, MessageDto>(message);
    }

    /// <inheritdoc/>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<BadgeDto> GetBadgeByOwnerIdAsync(long ownerId, bool? isImmersed = null)
    {
        var badge = await SessionUnitManager.GetBadgeByOwnerIdAsync(ownerId, isImmersed);

        var chatObjectInfo = await ChatObjectManager.GetItemByCacheAsync(ownerId);

        return new BadgeDto()
        {
            AppUserId = chatObjectInfo?.AppUserId,
            ChatObjectId = ownerId,
            Badge = badge
        };
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<BadgeDto> GetBadgeByIdAsync(Guid id, bool? isImmersed = null)
    {
        var entity = await GetEntityAsync(id);

        var badge = await SessionUnitManager.GetBadgeByIdAsync(id, isImmersed);

        return new BadgeDto()
        {
            AppUserId = entity.Owner.AppUserId,
            ChatObjectId = entity.OwnerId,
            Badge = badge
        };
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<List<BadgeDto>> GetBadgeByUserIdAsync(Guid userId, bool? isImmersed = null)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserId(userId);

        var result = new List<BadgeDto>();

        foreach (var chatObjectId in chatObjectIdList)
        {
            result.Add(await GetBadgeByOwnerIdAsync(chatObjectId, isImmersed));
        }
        return result;
    }

    /// <inheritdoc/>
    [HttpGet]
    [Authorize]
    public Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null)
    {
        return GetBadgeByUserIdAsync(CurrentUser.GetId(), isImmersed);
    }

    /// <inheritdoc/>
    [HttpGet]
    [Obsolete("Move to GetListBySessionIdAsync")]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetSessionMemberListAsync(Guid id, SessionUnitGetSessionMemberListInput input)
    {
        var entity = await GetEntityAsync(id);

        var objectTypeList = new List<ChatObjectTypeEnums>()
        {
             ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Official, ChatObjectTypeEnums.ShopKeeper, ChatObjectTypeEnums.Customer, ChatObjectTypeEnums.Robot,
        };

        var query = entity.Session.UnitList.AsQueryable()
           .Where(x => !x.Setting.IsKilled)
           .Where(x => objectTypeList.Contains(x.Owner.ObjectType.Value))
           .WhereIf(entity.Session.OwnerId == null, x => x.Id != id)
           .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(d => d.SessionTagId == input.TagId))
           .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(d => d.SessionRoleId == input.RoleId))
           ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<Guid> FindIdAsync(long ownerId, long destinactionId)
    {
        var entity = await SessionUnitManager.FindAsync(ownerId, destinactionId);

        Assert.NotNull(entity, "No found!");

        return entity.Id;
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<PagedResultDto<SessionUnitCacheItem>> GetListCachesAsync(SessionUnitCacheGetListInput input)
    {
        Assert.If(input.SessionUnitId == null && input.SessionId == null, "SessionUnitId Or SessionId cannot both be null");

        var sessionId = input.SessionId;

        if (input.SessionUnitId.HasValue)
        {
            var entity = await GetEntityAsync(input.SessionUnitId.Value);

            Assert.If(sessionId.HasValue && sessionId != entity.SessionId.Value, "Not in the same session");

            sessionId = entity.SessionId.Value;
        }

        var items = await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId.Value);

        var query = items.AsQueryable()
            .WhereIf(input.SessionUnitId.HasValue, x => x.Id == input.SessionUnitId);

        return await GetPagedListAsync<SessionUnitCacheItem, SessionUnitCacheItem>(query, input);
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<SessionUnitCacheItem> GetCacheAsync(Guid sessionUnitId)
    {
        var entity = await GetEntityAsync(sessionUnitId);

        return await SessionUnitManager.GetCacheItemAsync(entity);
    }

    /// <inheritdoc/>
    [HttpGet]
    public async Task<SessionUnitCounterInfo> GetCounterAsync(Guid sessionUnitId, long minMessageId = 0, bool? isImmersed = null)
    {
        return await SessionUnitManager.GetCounterAsync(sessionUnitId, minMessageId, isImmersed);
    }


    protected virtual async Task CheckEntriesInputAsync(ChatObject chatObject, Dictionary<Guid, List<EntryValueInput>> input)
    {

        foreach (var item in input)
        {
            var entryName = Assert.NotNull(await EntryNameRepository.FindAsync(item.Key), $"不存在EntryNameId:{item.Key}");

            Assert.If(entryName.IsRequired && item.Value.Count == 0, $"${entryName.Name}必填");

            Assert.If(item.Value.Count > entryName.MaxCount, $"${entryName.Name}最大个数：{entryName.MaxCount}");

            Assert.If(item.Value.Count < entryName.MinCount, $"${entryName.Name}最小个数：{entryName.MinCount}");

            foreach (var entryValue in item.Value)
            {
                Assert.If(entryValue.Value.Length > entryName.MaxLenth, $"${entryName.Name}[{item.Value.IndexOf(entryValue) + 1}]最大长度：{entryName.MaxLenth}");

                Assert.If(entryValue.Value.Length < entryName.MinLenth, $"${entryName.Name}[{item.Value.IndexOf(entryValue) + 1}]最小长度：{entryName.MinLenth}");

                //if (entryName.IsUniqued)
                //{
                //    var isAny = !await EntryValueRepository.AnyAsync(x => x.EntryNameId == entryName.Id && x.Value == entryValue.Value);
                //    Assert.If(isAny, $"${entryName.Name}已经存在值：{entryValue.Value}");
                //}
            }
        }
    }

    protected virtual async Task<List<T>> FormatItemsAsync<T>(Dictionary<Guid, List<EntryValueInput>> input, Func<Guid, string, Task<T>> createOrFindEntity)
    {
        var inputItems = new List<T>();

        foreach (var entry in input)
        {
            Assert.If(!await EntryNameRepository.AnyAsync(x => x.Id == entry.Key), $"不存在EntryNameId:{entry.Key}");

            foreach (var valueInput in entry.Value)
            {
                inputItems.Add(await createOrFindEntity(entry.Key, valueInput.Value));
            }
        }
        return inputItems;
    }

    
}
