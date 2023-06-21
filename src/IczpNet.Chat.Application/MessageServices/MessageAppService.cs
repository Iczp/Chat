using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Uow;
using System.Data;
using IczpNet.AbpCommons;
using IczpNet.Chat.Extensions;

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

    public MessageAppService(
        IMessageRepository repository,
        IReadedRecorderManager readedRecorderManager,
        IOpenedRecorderManager openedRecorderManager,
        IFavoritedRecorderManager favoriteManager,
        IFollowManager followManager) 
    {
        Repository = repository;
        ReadedRecorderManager = readedRecorderManager;
        OpenedRecorderManager = openedRecorderManager;
        FavoritedRecorderManager = favoriteManager;
        FollowManager = followManager;
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
    /// <param name="sessionUnitId"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [UnitOfWork(true, IsolationLevel.ReadUncommitted)]
    public async Task<PagedResultDto<MessageOwnerDto>> GetListAsync(Guid sessionUnitId, SessionUnitGetMessageListInput input)
    {
        var entity = await SessionUnitManager.GetAsync(sessionUnitId);

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
            .WhereIf(input.MinMessageId.HasValue, x => x.Id > input.MinMessageId)
            .WhereIf(input.MaxMessageId.HasValue, x => x.Id <= input.MaxMessageId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TextContentList.Any(d => d.Text.Contains(input.Keyword)))
            ;

        return await query.ToPagedListAsync<Message, MessageOwnerDto>(AsyncExecuter, ObjectMapper, input,
            x => x.OrderByDescending(x => x.Id),
            async entities =>
            {
                foreach (var e in entities)
                {
                    e.IsReaded = await ReadedRecorderManager.IsAnyAsync(sessionUnitId, e.Id);
                    e.IsOpened = await OpenedRecorderManager.IsAnyAsync(sessionUnitId, e.Id);
                    e.IsFavorited = await FavoritedRecorderManager.IsAnyAsync(sessionUnitId, e.Id);
                    e.IsFollowing = followingIdList.Contains(e.SessionUnitId.Value);
                }
                //await Task.Yield();
                return entities;
            });
    }


    /// <summary>
    /// 获取一条消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="messageId">消息Id</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<MessageDto> GetItemAsync(Guid sessionUnitId, long messageId)
    {
        var entity = await SessionUnitManager.GetAsync(sessionUnitId);

        //var message = entity.Session.MessageList.FirstOrDefault(x => x.Id == messageId);

        var message = await Repository.FindAsync(messageId);

        Assert.NotNull(message, "消息不存在!");

        Assert.If(message.IsRollbacked, "消息已撤回!");

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
}
