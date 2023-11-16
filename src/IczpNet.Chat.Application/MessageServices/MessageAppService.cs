using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageServices;

/// <summary>
/// 消息管理器
/// </summary>
public class MessageAppService : ChatAppService, IMessageAppService
{
    protected IMessageRepository Repository { get; }
    protected IReadedRecorderManager ReadedRecorderManager { get; }
    protected IOpenedRecorderManager OpenedRecorderManager { get; }
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; }
    protected IFollowManager FollowManager { get; }
    protected ISessionUnitRepository SessionUnitRepository { get; }

    public MessageAppService(
        IMessageRepository repository,
        IReadedRecorderManager readedRecorderManager,
        IOpenedRecorderManager openedRecorderManager,
        IFavoritedRecorderManager favoriteManager,
        IFollowManager followManager,
        ISessionUnitRepository sessionUnitRepository)
    {
        Repository = repository;
        ReadedRecorderManager = readedRecorderManager;
        OpenedRecorderManager = openedRecorderManager;
        FavoritedRecorderManager = favoriteManager;
        FollowManager = followManager;
        SessionUnitRepository = sessionUnitRepository;
    }

    /// <summary>
    /// 获取禁止转发的消息类型
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public Task<List<EnumDto>> GetDisabledForwardListAsync()
    {
        var result = MessageExtentions.DisabledForwardList
            .Select(x => new EnumDto()
            {
                Name = Enum.GetName(x),
                Description = x.GetDescription(),
                Value = (int)x
            })
            .ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// 获取消息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<PagedResultDto<MessageOwnerDto>> GetListAsync(MessageGetListInput input)
    {
        var sessionUnitId = input.SessionUnitId;

        var entity = await GetAndCheckPolicyAsync(GetListPolicyName, sessionUnitId);

        //Assert.NotNull(entity.Session, "session is null");

        var settting = entity.Setting;

        var followingIdList = await FollowManager.GetFollowingIdListAsync(sessionUnitId);

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == entity.SessionId)
            .Where(x => !x.IsPrivate || (x.IsPrivate && (x.SenderId == entity.OwnerId || x.ReceiverId == entity.OwnerId)))
            .WhereIf(settting.HistoryFristTime.HasValue, x => x.CreationTime >= settting.HistoryFristTime)
            .WhereIf(settting.HistoryLastTime.HasValue, x => x.CreationTime < settting.HistoryFristTime)
            .WhereIf(settting.ClearTime.HasValue, x => x.CreationTime > settting.ClearTime)
            .WhereIf(input.MessageType.HasValue, x => x.MessageType == input.MessageType)
            .WhereIf(input.IsFollowed.HasValue, x => followingIdList.Contains(x.SessionUnitId.Value))
            .WhereIf(input.IsRemind == true, x => x.IsRemindAll || x.MessageReminderList.Any(x => x.SessionUnitId == sessionUnitId))
            .WhereIf(input.SenderId.HasValue, x => x.SenderId == input.SenderId)
            .WhereIf(input.ForwardDepth.HasValue, x => x.ForwardDepth == input.ForwardDepth.Value)
            .WhereIf(input.QuoteDepth.HasValue, x => x.QuoteDepth == input.QuoteDepth.Value)
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
                    if (e.SenderId.HasValue && entity.OwnerId != e.SenderId)
                    {
                        var friendshipSessionUnit = await SessionUnitManager.FindAsync(entity.OwnerId, e.SenderId.Value);
                        e.SenderDisplayName = friendshipSessionUnit?.Setting.Rename;
                        e.FriendshipSessionUnitId = friendshipSessionUnit?.Id;
                    }
                    e.IsReaded = await ReadedRecorderManager.IsAnyAsync(sessionUnitId, e.Id);
                    e.IsOpened = await OpenedRecorderManager.IsAnyAsync(sessionUnitId, e.Id);
                    
                    e.IsFavorited = await FavoritedRecorderManager.IsAnyAsync(sessionUnitId, e.Id);
                    e.IsFollowing = e.SessionUnitId.HasValue && followingIdList.Contains(e.SessionUnitId.Value);
                }
                //await Task.Yield();
                return entities;
            });
    }


    /// <summary>
    /// 获取一条消息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<MessageDto> GetItemAsync(MessageGetItemInput input)
    {
        var sessionUnitId = input.SessionUnitId;

        var messageId = input.MessageId;

        var entity = await GetAndCheckPolicyAsync(GetListPolicyName, sessionUnitId);

        //var message = entity.Session.MessageList.FirstOrDefault(x => x.Id == messageId);

        var message = await Repository.FindAsync(input.MessageId);

        Assert.NotNull(message, "消息不存在!");

        Assert.If(message.IsRollbacked, "消息已撤回!");

        var isSameSession = await IsInSameSessionAsync(message.SessionId.Value, entity.OwnerId);

        if (isSameSession)
        {
            return ObjectMapper.Map<Message, MessageDto>(message);
        }

        var isInQuoteSession = await Repository.AnyAsync(x => x.QuotedMessageList.Any(d => d.Id == messageId));
        //...是否包含在哪个聊天记录里，是否包含在引用消息里
        //...以下待测试...

        var isCanRead = (await Repository.GetQueryableAsync())
            //本条消息 || 引用这个消息的 || 包含在聊天记录里的
            .Where(x => x.Id == messageId || x.QuotedMessageList.Any(d => d.Id == messageId) || x.HistoryMessageList.Any(x => x.MessageId == messageId))
            .Select(x => x.Session)
            .Any(x => x.UnitList.Any(d => d.OwnerId == entity.OwnerId))
            ;

        Assert.If(!isCanRead, "非法访问!");

        return ObjectMapper.Map<Message, MessageDto>(message);
    }

    protected virtual Task<bool> IsInSameSessionAsync(List<Guid> sessionIdList, long ownerId)
    {
        return SessionUnitRepository.AnyAsync(x => sessionIdList.Contains(x.SessionId.Value) && x.OwnerId == ownerId);
    }

    protected virtual Task<bool> IsInSameSessionAsync(Guid sessionId, long ownerId)
    {
        return SessionUnitRepository.AnyAsync(x => x.SessionId == sessionId && x.OwnerId == ownerId);
    }
}
