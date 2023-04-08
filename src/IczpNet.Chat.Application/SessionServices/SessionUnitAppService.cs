using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionServices;

public class SessionUnitAppService : ChatAppService, ISessionUnitAppService
{
    public virtual string GetListPolicyName { get; set; }
    public virtual string GetPolicyName { get; set; }
    public virtual string GetDetailPolicyName { get; set; }
    public virtual string SetReadedPolicyName { get; set; }
    public virtual string SetImmersedPolicyName { get; set; }
    public virtual string RemoveSessionPolicyName { get; set; }
    public virtual string ClearMessagePolicyName { get; set; }
    public virtual string DeleteMessagePolicyName { get; set; }

    protected IRepository<Friendship, Guid> FriendshipRepository { get; }
    protected IRepository<Session, Guid> SessionRepository { get; }
    protected ISessionUnitRepository Repository { get; }
    protected IMessageRepository MessageRepository { get; }
    protected ISessionManager SessionManager { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionGenerator SessionGenerator { get; }
    protected IChatObjectManager ChatObjectManager { get; }


    public SessionUnitAppService(
        IRepository<Friendship, Guid> chatObjectRepository,
        ISessionManager sessionManager,
        ISessionGenerator sessionGenerator,
        IRepository<Session, Guid> sessionRepository,
        IMessageRepository messageRepository,
        ISessionUnitRepository repository,
        ISessionUnitManager sessionUnitManager,
        IChatObjectManager chatObjectManager)
    {
        FriendshipRepository = chatObjectRepository;
        SessionManager = sessionManager;
        SessionGenerator = sessionGenerator;
        SessionRepository = sessionRepository;
        MessageRepository = messageRepository;
        Repository = repository;
        SessionUnitManager = sessionUnitManager;
        ChatObjectManager = chatObjectManager;
    }

    protected override Task CheckPolicyAsync(string policyName)
    {
        return base.CheckPolicyAsync(policyName);
    }

    protected virtual async Task<SessionUnit> GetEntityAsync(Guid id, bool checkIsKilled = true)
    {
        var entity = await Repository.GetAsync(id);

        Assert.If(checkIsKilled && entity.IsKilled, "已经删除的会话单元!");

        return entity;
    }

    protected virtual async Task<IQueryable<SessionUnit>> GetQueryAsync(SessionUnitGetListInput input)
    {
        return (await Repository.GetQueryableAsync())
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.IsKilled.HasValue, x => x.IsKilled == input.IsKilled)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.Destination.ObjectType == input.DestinationObjectType)
            //.WhereIf(input.MinAutoId.HasValue, x => x.Session.LastMessageId > input.MinAutoId)
            //.WhereIf(input.MaxAutoId.HasValue, x => x.Session.LastMessageId < input.MaxAutoId)
            .WhereIf(input.MinAutoId.HasValue && input.MinAutoId.Value > 0, x => x.LastMessageId > input.MinAutoId)
            .WhereIf(input.MaxAutoId.HasValue && input.MaxAutoId.Value > 0, x => x.LastMessageId < input.MaxAutoId)
            .WhereIf(input.IsTopping == true, x => x.Sorting != 0)
            .WhereIf(input.IsTopping == false, x => x.Sorting == 0)
            .WhereIf(input.IsBadge, x =>
                x.Session.MessageList.Any(d =>
                    //!x.IsRollbacked &&
                    d.Id > x.ReadedMessageId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                )
            )
            .WhereIf(input.IsRemind, x =>
                x.Session.MessageList.Any(d => !d.IsRollbacked && d.IsRemindAll) ||
                x.ReminderList.Any(d => !d.Message.IsRollbacked)
            )
            ;
    }

    protected virtual async Task<IQueryable<SessionUnit>> GetQueryByJoinAsync(SessionUnitGetListInput input)
    {
        return (await Repository.GetQueryableAsync())
            .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
            .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
            .WhereIf(input.IsKilled.HasValue, x => x.IsKilled == input.IsKilled)
            .WhereIf(input.DestinationObjectType.HasValue, x => x.Destination.ObjectType == input.DestinationObjectType)
            .WhereIf(input.MinAutoId.HasValue, x => x.Session.LastMessageId > input.MinAutoId)
            .WhereIf(input.MaxAutoId.HasValue, x => x.Session.LastMessageId < input.MaxAutoId)
            //.WhereIf(input.MinAutoId.HasValue && input.MinAutoId.Value > 0, x => x.LastMessageAutoId > input.MinAutoId)
            //.WhereIf(input.MaxAutoId.HasValue && input.MaxAutoId.Value > 0, x => x.LastMessageAutoId < input.MaxAutoId)
            .WhereIf(input.IsTopping == true, x => x.Sorting != 0)
            .WhereIf(input.IsTopping == false, x => x.Sorting == 0)
            .WhereIf(input.IsBadge, x =>
                x.Session.MessageList.Any(d =>
                    //!x.IsRollbacked &&
                    d.Id > x.ReadedMessageId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                )
            )
            .WhereIf(input.IsRemind, x =>
                x.Session.MessageList.Any(d => !d.IsRollbacked && d.IsRemindAll) ||
                x.ReminderList.Any(d => !d.Message.IsRollbacked)
            )
            ;
    }

    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadCommitted)]
    public virtual async Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyAsync(GetListPolicyName);

        var query = await GetQueryAsync(input);

        //return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input);

        if (!input.IsOrderByBadge)
        {
            return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
                query,
                input,
                x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
        }

        Expression<Func<SessionUnit, int>> p = x =>
                x.Session.MessageList.Count(d =>
                    //!x.IsRollbacked &&
                    d.Id > x.ReadedMessageId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                 );

        return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
            query,
            input,
            x => x.OrderByDescending(d => d.Sorting)
                  .ThenByDescending(p)
                );

    }

    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadCommitted)]
    public virtual async Task<PagedResultDto<SessionUnitOwnerDto>> GetListByJoinAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyAsync(GetListPolicyName);

        var query = await GetQueryByJoinAsync(input);

        return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
            query,
            input,
            x => x.OrderByDescending(d => d.Sorting)
                  .ThenByDescending(d => d.Session.LastMessageId)
                );

    }


    [HttpGet]
    public async Task<PagedResultDto<SessionUnitDestinationDto>> GetDestinationListAsync(Guid id, SessionUnitGetDestinationListInput input)
    {
        var entity = await Repository.GetAsync(id);

        var query = (await Repository.GetQueryableAsync())

            .Where(x => x.SessionId == entity.SessionId)
            .WhereIf(input.IsKilled.HasValue, x => x.IsKilled == input.IsKilled)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic)
            .WhereIf(input.OwnerIdList.IsAny(), x => input.OwnerIdList.Contains(x.OwnerId))
            .WhereIf(input.OwnerTypeList.IsAny(), x => input.OwnerTypeList.Contains(x.Owner.ObjectType.Value))
            .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(x => x.SessionTagId == input.TagId))
            .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(x => x.SessionRoleId == input.RoleId))
            .WhereIf(!input.JoinWay.IsEmpty(), x => x.JoinWay == input.JoinWay)
            .WhereIf(!input.InviterId.IsEmpty(), x => x.InviterId == input.InviterId)
            .WhereIf(!input.InviterUnitId.IsEmpty(), x => x.InviterUnitId == input.InviterUnitId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Owner.Name.Contains(input.Keyword))
            ;

        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input, q => q.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<SessionUnitOwnerDto>> GetListByLinqAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyAsync(GetListPolicyName);

        var query = (await GetQueryAsync(input)).Select(x => new SessionUnitModel
        {
            Id = x.Id,
            OwnerId = x.OwnerId,
            SessionId = x.SessionId,
            Sorting = x.Sorting,
            Destination = x.Destination,
            LastMessage = x.Session.MessageList.FirstOrDefault(m => m.Id == x.Session.MessageList.Where(d =>
                    //!x.IsRollbacked &&
                    d.Id > x.ReadedMessageId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)).Max(d => d.Id)),
            Badge = x.Session.MessageList.Count(d =>
                   //!x.IsRollbacked &&
                   d.Id > x.ReadedMessageId &&
                   d.SenderId != x.OwnerId &&
                   (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                   (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                   (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                 ),
            ReminderAllCount = x.Session.MessageList.Count(x => !x.IsRollbacked && x.IsRemindAll),
            ReminderMeCount = x.ReminderList.Select(x => x.Message).Count(d =>
                    //!x.IsRollbacked &&
                    d.Id > x.ReadedMessageId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                 )
        });

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query.OrderByDescending(x => x.Sorting)
            .OrderByDescending(x => x.LastMessage.Id)
            .OrderByDescending(x => x.Badge);

        query = query.PageBy(input);

        var models = await AsyncExecuter.ToListAsync(query);

        var items = ObjectMapper.Map<List<SessionUnitModel>, List<SessionUnitOwnerDto>>(models);

        return new PagedResultDto<SessionUnitOwnerDto>(totalCount, items);
    }

    [HttpGet]
    public virtual async Task<SessionUnitOwnerDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(GetPolicyName);

        var entity = await GetEntityAsync(id);

        return await MapToDtoAsync(entity);
    }

    [HttpGet]
    public virtual async Task<SessionUnitDestinationDetailDto> GetDetailAsync(Guid id)
    {
        await CheckPolicyAsync(GetDetailPolicyName);

        var entity = await GetEntityAsync(id);

        return ObjectMapper.Map<SessionUnit, SessionUnitDestinationDetailDto>(entity);
    }

    [HttpGet]
    public virtual async Task<SessionUnitDestinationDto> GetDestinationAsync(Guid id, Guid destinationId)
    {
        var destinationEntity = await GetEntityAsync(destinationId);

        var selfEntity = await GetEntityAsync(id);

        Assert.If(selfEntity.SessionId != destinationEntity.SessionId, $"Not in the same session");

        return await MapToDestinationDtoAsync(destinationEntity);
    }

    protected virtual Task<SessionUnitOwnerDto> MapToDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(entity));
    }

    protected virtual Task<SessionUnitDestinationDto> MapToDestinationDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDestinationDto>(entity));
    }

    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetToppingAsync(Guid id, bool isTopping)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetToppingAsync(entity, isTopping);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> SetReadedAsync(Guid id, long messageId, bool isForce = false)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetReadedAsync(entity, messageId, isForce);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public async Task<SessionUnitOwnerDto> SetImmersedAsync(Guid id, bool isImmersed)
    {
        await CheckPolicyAsync(SetImmersedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetImmersedAsync(entity, isImmersed);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> RemoveAsync(Guid id)
    {
        await CheckPolicyAsync(RemoveSessionPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.RemoveAsync(entity);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> KillAsync(Guid id)
    {
        var entity = await GetEntityAsync(id);

        await SessionUnitManager.KillAsync(entity);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> ClearMessageAsync(Guid id)
    {
        await CheckPolicyAsync(ClearMessagePolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.ClearMessageAsync(entity);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitOwnerDto> DeleteMessageAsync(Guid id, long messageId)
    {
        await CheckPolicyAsync(DeleteMessagePolicyName);

        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionUnitGetMessageListInput input)
    {
        var entity = await GetEntityAsync(id);

        Assert.NotNull(entity.Session, "session is null");

        var query = entity.Session.MessageList.AsQueryable()
            //Official
            .WhereIf(entity.Session.Owner != null && entity.Session.Owner.ObjectType == ChatObjectTypeEnums.Official, x =>
                (x.SenderId == entity.OwnerId && x.ReceiverId == entity.Session.OwnerId) ||
                (x.ReceiverId == entity.OwnerId && x.SenderId == entity.Session.OwnerId) ||
                (x.SenderId == x.ReceiverId && x.SenderId == entity.OwnerId)
            )
            .WhereIf(entity.HistoryFristTime.HasValue, x => x.CreationTime > entity.HistoryFristTime)
            .WhereIf(entity.HistoryLastTime.HasValue, x => x.CreationTime < entity.HistoryFristTime)
            .WhereIf(entity.ClearTime.HasValue, x => x.CreationTime > entity.ClearTime)
            .WhereIf(input.MessageType.HasValue, x => x.MessageType == input.MessageType)
            .WhereIf(!input.IsRemind.IsEmpty(), x => x.IsRemindAll || x.MessageReminderList.Any(x => x.SessionUnitId == id))
            .WhereIf(!input.SenderId.IsEmpty(), new SenderMessageSpecification(input.SenderId.GetValueOrDefault()).ToExpression())
            .WhereIf(!input.MinAutoId.IsEmpty(), new MinAutoIdMessageSpecification(input.MinAutoId.GetValueOrDefault()).ToExpression())
            .WhereIf(!input.MaxAutoId.IsEmpty(), new MaxAutoIdMessageSpecification(input.MaxAutoId.GetValueOrDefault()).ToExpression())
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TextContentList.Any(d => d.Text.Contains(input.Keyword)))
            ;
        return await GetPagedListAsync<Message, MessageDto>(query, input, x => x.OrderByDescending(x => x.Id));
    }

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

    [HttpGet]
    public async Task<BadgeDto> GetBadgeAsync(long ownerId, bool? isImmersed = null)
    {
        var badge = await SessionUnitManager.GetBadgeAsync(ownerId, isImmersed);

        var chatObjectInfo = await ChatObjectManager.GetItemByCacheAsync(ownerId);

        return new BadgeDto()
        {
            AppUserId = chatObjectInfo?.AppUserId,
            ChatObjectId = ownerId,
            Badge = badge
        };
    }

    [HttpGet]
    public async Task<List<BadgeDto>> GetBadgeByUserIdAsync(Guid userId, bool? isImmersed = null)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserId(userId);

        var result = new List<BadgeDto>();

        foreach (var chatObjectId in chatObjectIdList)
        {
            result.Add(await GetBadgeAsync(chatObjectId, isImmersed));
        }
        return result;
    }

    [HttpGet]
    [Authorize]
    public Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null)
    {
        return GetBadgeByUserIdAsync(CurrentUser.GetId(), isImmersed);
    }

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
           .Where(x => !x.IsKilled)
           .Where(x => objectTypeList.Contains(x.Owner.ObjectType.Value))
           .WhereIf(entity.Session.OwnerId == null, x => x.Id != id)
           .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(d => d.SessionTagId == input.TagId))
           .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(d => d.SessionRoleId == input.RoleId))
           ;
        return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
    }

    [HttpGet]
    public Task<Guid?> FindIdAsync(long ownerId, long destinactionId)
    {
        return SessionUnitManager.FindIdAsync(ownerId, destinactionId);
    }
}
