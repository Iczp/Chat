using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnitSettings.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageServices;

/// <summary>
/// 消息管理器
/// </summary>
public class MessageAppService(
    IDeletedRecorderManager deletedRecorderManager,
    IMessageRepository repository,
    IReadedRecorderManager readedRecorderManager,
    IOpenedRecorderManager openedRecorderManager,
    IFavoritedRecorderManager favoriteManager,
    IFollowManager followManager,
    ISessionUnitRepository sessionUnitRepository,
    ISessionUnitSettingManager sessionUnitSettingManager,
    IMessageManager messageManager) : ChatAppService, IMessageAppService
{
    public IDeletedRecorderManager DeletedRecorderManager { get; } = deletedRecorderManager;
    protected IMessageRepository Repository { get; } = repository;
    protected IReadedRecorderManager ReadedRecorderManager { get; } = readedRecorderManager;
    protected IOpenedRecorderManager OpenedRecorderManager { get; } = openedRecorderManager;
    protected IFavoritedRecorderManager FavoritedRecorderManager { get; } = favoriteManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    public ISessionUnitSettingManager SessionUnitSettingManager { get; } = sessionUnitSettingManager;
    protected IMessageManager MessageManager { get; } = messageManager;

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

    private async Task<IQueryable<Message>> CreateQueryableAsync(SessionUnit entity, MessageGetListInput input)
    {

        var sessionId = entity.SessionId;
        var sessionUnitId = entity.Id;
        //var setting = entity.Setting;

        var setting = await SessionUnitSettingManager.GetOrAddCacheAsync(sessionUnitId);

        //var deletedIdList = await DeletedRecorderManager.GetDeletedMessageIdListAsync(sessionUnitId);

        //var followingIdList = await FollowManager.GetFollowingIdListAsync(sessionUnitId);

        var query = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionId)
            //排除已删除的消息
            .Where(x => !x.DeletedList.Any(d => d.SessionUnitId == sessionUnitId && d.MessageId == x.Id))
            //排除已删除的消息 -- 删除数据量大小，不稳定
            //.WhereIf(deletedIdList.Count != 0, x => !deletedIdList.Contains(x.Id))
            .WhereIf(setting.HistoryFristTime.HasValue, x => x.CreationTime >= setting.HistoryFristTime)
            .WhereIf(setting.HistoryLastTime.HasValue, x => x.CreationTime < setting.HistoryLastTime)
            .WhereIf(setting.ClearTime.HasValue, x => x.CreationTime > setting.ClearTime)

            .WhereIf(input.StartTime.HasValue, x => x.CreationTime >= input.StartTime)
            .WhereIf(input.EndTime.HasValue, x => x.CreationTime < input.EndTime)

            .WhereIf(input.MessageTypes.IsAny(), x => input.MessageTypes.Contains(x.MessageType))

            //.WhereIf(input.IsFollowed.HasValue, x => followingIdList.Contains(x.SenderSessionUnitId.Value))
            .WhereIf(input.IsFollowed.HasValue, x => x.MessageFollowerList.Any(d => d.SessionUnitId == sessionUnitId))

            .WhereIf(input.IsRemind == true, x => x.IsRemindAll || x.MessageReminderList.Any(x => x.SessionUnitId == sessionUnitId))
            .WhereIf(input.SenderId.HasValue, x => x.SenderId == input.SenderId)
            .WhereIf(input.ForwardDepth.HasValue, x => x.ForwardDepth == input.ForwardDepth.Value)
            .WhereIf(input.QuoteDepth.HasValue, x => x.QuoteDepth == input.QuoteDepth.Value)
            .WhereIf(input.MinMessageId.HasValue, x => x.Id > input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.Id < input.MaxMessageId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TextContentList.Any(d => d.Text.Contains(input.Keyword)))
            ;

        return query;

    }

    /// <summary>
    /// 填充设置信息
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task FillSettingsAsync(PagedResultDto<MessageOwnerDto> result)
    {
        var senderUnitIdList = result.Items
            .Where(x => x.SenderSessionUnitId != null)
            .Select(x => x.SenderSessionUnitId.Value)
            .Distinct()
            .ToList();

        var settings = await SessionUnitSettingManager.GetOrAddManyCacheAsync(senderUnitIdList);

        var settingsMap = settings.ToDictionary(x => x.Key, x => x.Value);

        foreach (var item in result.Items)
        {
            if (item.SenderSessionUnitId.HasValue && settingsMap.TryGetValue(item.SenderSessionUnitId.Value, out var senderSetting))
            {
                item.SenderSessionUnit.Setting = ObjectMapper.Map<SessionUnitSettingCacheItem, SessionUnitSettingSimpleDto>(senderSetting);
            }
        }
    }

    /// <summary>
    /// 统计信息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task FillStatisticsAsync(Guid sessionUnitId, PagedResultDto<MessageOwnerDto> result)
    {
        var followingIdList = await FollowManager.GetFollowingIdListAsync(sessionUnitId);

        // 一次性获取(实时查询)
        var messageIdList = result.Items.Select(x => x.Id).ToList();
        var readMessageIdList = await ReadedRecorderManager.GetRecorderMessageIdListAsync(sessionUnitId, messageIdList);
        var openedMessageIdList = await OpenedRecorderManager.GetRecorderMessageIdListAsync(sessionUnitId, messageIdList);
        var favoritedMessageIdList = await FavoritedRecorderManager.GetRecorderMessageIdListAsync(sessionUnitId, messageIdList);
        var remindMessageIdList = await MessageManager.GetRemindMessageIdListAsync(sessionUnitId, messageIdList);

        foreach (var e in result.Items)
        {
            e.IsReaded = readMessageIdList.Contains(e.Id);
            e.IsOpened = openedMessageIdList.Contains(e.Id);
            e.IsFavorited = favoritedMessageIdList.Contains(e.Id);
            e.IsFollowing = e.SenderSessionUnitId.HasValue && followingIdList.Contains(e.SenderSessionUnitId.Value);
            e.IsRemindMe = remindMessageIdList.Contains(e.Id);
        }

    }

    /// <summary>
    /// 好友关系
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task FillFriendshipAsync(long ownerId, PagedResultDto<MessageOwnerDto> result)
    {
        // friendship
        var distnactionIds = result.Items
            .Where(x => x.SenderSessionUnit != null)
            .Select(x => x.SenderSessionUnit.OwnerId)
            .Distinct()
            .ToList();

        var destnactions = await SessionUnitManager.FindManyAsync(ownerId, distnactionIds);

        var friendsMap = destnactions
            .GroupBy(x => x.DestinationId)
            .ToDictionary(x => x.Key, g => g.FirstOrDefault());

        foreach (var item in result.Items)
        {
            if (item.SenderSessionUnit == null)
            {
                Logger.LogWarning($"item.SenderSessionUnit is null, MessageId:{item.Id}");
                continue;
            }

            var friendshipSessionUnit = friendsMap.GetValueOrDefault(item.SenderSessionUnit.OwnerId);

            item.SenderSessionUnit.IsFriendship = friendshipSessionUnit != null;
            item.SenderSessionUnit.FriendshipSessionUnitId = friendshipSessionUnit?.Id;
            item.SenderSessionUnit.FriendshipName = friendshipSessionUnit?.Setting.Rename;
            item.SenderSessionUnit.MemberName = friendshipSessionUnit?.Setting.MemberName;
        }
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

        var followingIdList = await FollowManager.GetFollowingIdListAsync(sessionUnitId);

        var query = await CreateQueryableAsync(entity, input);

        var result = await GetPagedListAsync<Message, MessageOwnerDto>(query, input,
            x => x.OrderByDescending(x => x.Id));

        await FillStatisticsAsync(sessionUnitId, result);

        await FillFriendshipAsync(entity.OwnerId, result);

        return result;
    }



    public async Task<PagedResultDto<MessageOwnerDto>> GetListFastAsync(MessageGetListInput input)
    {
        var sessionUnitId = input.SessionUnitId;

        var entity = await GetAndCheckPolicyAsync(GetListPolicyName, sessionUnitId);

        var query = await CreateQueryableAsync(entity, input);

        var idQueryable = query.Select(x => x.Id);

        var pagedResult = await GetPagedListAsync(
            idQueryable,
            input,
            x => x.OrderByDescending(x => x));

        var messages = await MessageManager.GetOrAddManyCacheAsync(pagedResult.Items);

        var cacheItems = messages.Select(x => x.Value).ToList();

        var items = await MapToMessagesAsync(cacheItems);

        var result = new PagedResultDto<MessageOwnerDto>(pagedResult.TotalCount, items);

        await FillStatisticsAsync(sessionUnitId, result);

        await FillFriendshipAsync(entity.OwnerId, result);

        await FillSettingsAsync(result);

        return result;
    }

    /// <summary>
    /// 获取消息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<PagedResultDto<MessageByDateDto>> GetListByDateAsync(MessageGetListByDateInput input)
    {
        var sessionUnitId = input.SessionUnitId;

        var entity = await GetAndCheckPolicyAsync(GetListPolicyName, sessionUnitId);

        //Assert.NotNull(entity.Session, "session is null");

        var query = await CreateQueryableAsync(entity, input);

        var q = query.GroupBy(x => x.CreationTime.Date).Select(x => new MessageByDateDto
        {
            Date = x.Key,
            Count = x.Count(),
            MessageIdList = input.TakeCount.HasValue ? x.Select(d => d.Id).Take(input.TakeCount.Value).ToList() : null,
        }).ToList().AsQueryable();

        var result = await GetPagedListAsync(q, input, q => q.OrderByDescending(x => x.Date));

        return result;
    }


    /// <summary>
    /// 获取一条消息
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<MessageOwnerDto> GetItemAsync(MessageGetItemInput input)
    {
        var message = await GetMessageAsync(input);

        return await MapToMessageAsync(message);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual async Task<MessageOwnerDto> MapToMessageAsync(Message message)
    {
        await Task.Yield();
        return ObjectMapper.Map<Message, MessageOwnerDto>(message);
    }

    protected virtual async Task<MessageOwnerDto> MapToMessageAsync(MessageCacheItem message)
    {
        await Task.Yield();
        return ObjectMapper.Map<MessageCacheItem, MessageOwnerDto>(message);
    }

    protected virtual async Task<List<MessageOwnerDto>> MapToMessagesAsync(List<MessageCacheItem> messages)
    {
        await Task.Yield();
        return ObjectMapper.Map<List<MessageCacheItem>, List<MessageOwnerDto>>(messages);
    }

    /// <summary>
    /// 获取一条消息
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<Message> GetMessageAsync(MessageGetItemInput input)
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
            return message;
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

        return message;
    }

    protected virtual Task<bool> IsInSameSessionAsync(List<Guid> sessionIdList, long ownerId)
    {
        return SessionUnitRepository.AnyAsync(x => sessionIdList.Contains(x.SessionId.Value) && x.OwnerId == ownerId);
    }

    protected virtual Task<bool> IsInSameSessionAsync(Guid sessionId, long ownerId)
    {
        return SessionUnitRepository.AnyAsync(x => x.SessionId == sessionId && x.OwnerId == ownerId);
    }

    /// <summary>
    /// 获取一条消息(File)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<MessageOwnerDto> GetFileAsync(MessageGetItemInput input)
    {
        var message = await GetMessageAsync(input);

        throw new NotImplementedException();

    }
}
