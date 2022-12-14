using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.Specifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionServices;

public class SessionUnitAppService : ChatAppService, ISessionUnitAppService
{
    protected IRepository<Friendship, Guid> FriendshipRepository { get; }
    protected IRepository<Session, Guid> SessionRepository { get; }
    protected IRepository<SessionUnit, Guid> Repository { get; }
    protected IRepository<Message, Guid> MessageRepository { get; }
    protected ISessionManager SessionManager { get; }
    protected ISessionUnitManager SessionUnitManager { get; }
    protected ISessionGenerator SessionGenerator { get; }
    public virtual string GetListPolicyName { get; private set; }
    public virtual string GetPolicyName { get; private set; }
    public virtual string GetDetailPolicyName { get; private set; }
    public virtual string SetReadedPolicyName { get; private set; }
    public virtual string RemoveSessionPolicyName { get; private set; }
    public virtual string ClearMessagePolicyName { get; private set; }
    public virtual string DeleteMessagePolicyName { get; private set; }

    public SessionUnitAppService(
        IRepository<Friendship, Guid> chatObjectRepository,
        ISessionManager sessionManager,
        ISessionGenerator sessionGenerator,
        IRepository<Session, Guid> sessionRepository,
        IRepository<Message, Guid> messageRepository,
        IRepository<SessionUnit, Guid> repository,
        ISessionUnitManager sessionUnitManager)
    {
        FriendshipRepository = chatObjectRepository;
        SessionManager = sessionManager;
        SessionGenerator = sessionGenerator;
        SessionRepository = sessionRepository;
        MessageRepository = messageRepository;
        Repository = repository;
        SessionUnitManager = sessionUnitManager;
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
            .WhereIf(input.MinAutoId.HasValue, x => x.Session.LastMessageAutoId > input.MinAutoId)
            .WhereIf(input.MaxAutoId.HasValue, x => x.Session.LastMessageAutoId < input.MaxAutoId)
            .WhereIf(input.IsBadge, x =>
                x.Session.MessageList.Any(d =>
                    //!x.IsRollbacked &&
                    d.AutoId > x.ReadedMessageAutoId &&
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
    public virtual async Task<PagedResultDto<SessionUnitDto>> GetListAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyAsync(GetListPolicyName);

        var query = await GetQueryAsync(input);

        if (!input.IsOrderByBadge)
        {
            return await GetPagedListAsync<SessionUnit, SessionUnitDto>(
                query,
                input,
                x => x.OrderByDescending(d => d.Sorting).OrderByDescending(x => x.Session.LastMessageAutoId));
        }

        Expression<Func<SessionUnit, int>> p = x =>
                x.Session.MessageList.Count(d =>
                    //!x.IsRollbacked &&
                    d.AutoId > x.ReadedMessageAutoId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                 );

        return await GetPagedListAsync<SessionUnit, SessionUnitDto>(
            query,
            input,
            x => x.OrderByDescending(d => d.Sorting).ThenByDescending(p)
                );

    }
    [HttpGet]
    public virtual async Task<PagedResultDto<SessionUnitDto>> GetListByLinqAsync(SessionUnitGetListInput input)
    {
        await CheckPolicyAsync(GetListPolicyName);

        var query = (await GetQueryAsync(input)).Select(x => new SessionUnitModel
        {
            Id = x.Id,
            OwnerId = x.OwnerId,
            SessionId = x.SessionId,
            Sorting = x.Sorting,
            Destination = x.Destination,
            LastMessage = x.Session.MessageList.FirstOrDefault(m => m.AutoId == x.Session.MessageList.Where(d =>
                    //!x.IsRollbacked &&
                    d.AutoId > x.ReadedMessageAutoId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)).Max(d => d.AutoId)),
            Badge = x.Session.MessageList.Count(d =>
                   //!x.IsRollbacked &&
                   d.AutoId > x.ReadedMessageAutoId &&
                   d.SenderId != x.OwnerId &&
                   (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                   (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                   (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                 ),
            ReminderAllCount = x.Session.MessageList.Count(x => !x.IsRollbacked && x.IsRemindAll),
            ReminderMeCount = x.ReminderList.Select(x => x.Message).Count(d =>
                    //!x.IsRollbacked &&
                    d.AutoId > x.ReadedMessageAutoId &&
                    d.SenderId != x.OwnerId &&
                    (!x.HistoryFristTime.HasValue || d.CreationTime > x.HistoryFristTime) &&
                    (!x.HistoryLastTime.HasValue || d.CreationTime < x.HistoryLastTime) &&
                    (!x.ClearTime.HasValue || d.CreationTime > x.ClearTime)
                 )
        });

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query.OrderByDescending(x => x.Sorting)
            .OrderByDescending(x => x.LastMessage.AutoId)
            .OrderByDescending(x => x.Badge);

        query = query.PageBy(input);

        var models = await AsyncExecuter.ToListAsync(query);

        var items = ObjectMapper.Map<List<SessionUnitModel>, List<SessionUnitDto>>(models);

        return new PagedResultDto<SessionUnitDto>(totalCount, items);
    }

    [HttpGet]
    public virtual async Task<SessionUnitDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(GetPolicyName);

        var entity = await GetEntityAsync(id);

        return await MapToDtoAsync(entity);
    }

    [HttpGet]
    public virtual async Task<SessionUnitDetailDto> GetDetailAsync(Guid id)
    {
        await CheckPolicyAsync(GetDetailPolicyName);

        var entity = await GetEntityAsync(id);

        return ObjectMapper.Map<SessionUnit, SessionUnitDetailDto>(entity);
    }

    private Task<SessionUnitDto> MapToDtoAsync(SessionUnit entity)
    {
        return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDto>(entity));
    }

    [HttpPost]
    public virtual async Task<SessionUnitDto> SetToppingAsync(Guid id, bool isTopping)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetToppingAsync(entity, isTopping);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitDto> SetReadedAsync(Guid id, Guid messageId, bool isForce = false)
    {
        await CheckPolicyAsync(SetReadedPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.SetReadedAsync(entity, messageId, isForce);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitDto> RemoveSessionAsync(Guid id)
    {
        await CheckPolicyAsync(RemoveSessionPolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.RemoveSessionAsync(entity);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitDto> KillSessionAsync(Guid id)
    {
        var entity = await GetEntityAsync(id);

        await SessionUnitManager.KillSessionAsync(entity);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitDto> ClearMessageAsync(Guid id)
    {
        await CheckPolicyAsync(ClearMessagePolicyName);

        var entity = await GetEntityAsync(id);

        await SessionUnitManager.ClearMessageAsync(entity);

        return await MapToDtoAsync(entity);
    }

    [HttpPost]
    public virtual async Task<SessionUnitDto> DeleteMessageAsync(Guid id, Guid messageId)
    {
        await CheckPolicyAsync(DeleteMessagePolicyName);

        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionUnitGetMessageListInput input)
    {
        var entity = await GetEntityAsync(id);

        var query = entity.Session.MessageList.AsQueryable()
            .WhereIf(entity.Session.Owner.ObjectType == ChatObjectTypes.Official, x =>
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
        return await GetPagedListAsync<Message, MessageDto>(query, input, x => x.OrderByDescending(x => x.AutoId));
    }

    [HttpGet]
    public async Task<MessageDto> GetMessageAsync(Guid id, Guid messageId)
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
            .Where(x => x.Id == id || x.QuotedMessageList.Any(d => d.Id == messageId) || x.HistoryMessageList.Any(x => x.MessageId == messageId))
            .Select(x => x.Session)
            .Any(x => x.UnitList.Any(d => d.OwnerId == entity.OwnerId))
            ;

        Assert.If(!isCanRead, "非法访问!");

        return ObjectMapper.Map<Message, MessageDto>(message);
    }

    [HttpGet]
    public Task<int> GetBadgeAsync(Guid ownerId)
    {
        return SessionUnitManager.GetBadgeAsync(ownerId); ;
    }

    [HttpGet]
    public async Task<PagedResultDto<SessionUnitOwnerDto>> GetSessionMemberListAsync(Guid id, SessionUnitGetSessionMemberListInput input)
    {
        var entity = await GetEntityAsync(id);

        var objectTypeList = new List<ChatObjectTypes>()
        {
             ChatObjectTypes.Personal, ChatObjectTypes.Official, ChatObjectTypes.ShopKeeper, ChatObjectTypes.Customer, ChatObjectTypes.Robot,
        };

        var query = entity.Session.UnitList.AsQueryable()
           .Where(x => !x.IsKilled)
           .Where(x => objectTypeList.Contains(x.Owner.ObjectType.Value))
           .WhereIf(entity.Session.OwnerId == null, x => x.Id != id)
           .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(d => d.SessionTagId == input.TagId))
           .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(d => d.SessionRoleId == input.RoleId))
           ;
        return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(query, input);
    }


}
