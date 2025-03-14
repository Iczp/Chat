using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using IczpNet.Pusher.ShortIds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class MessageManager(
    IMessageRepository repository,
    IShortIdGenerator shortIdGenerator,
    IChatObjectManager chatObjectManager,
    IObjectMapper objectMapper,
    IMessageValidator messageValidator,
    IChatPusher chatPusher,
    IDistributedEventBus distributedEventBus,
    ICurrentHosted currentHosted,
    ISessionUnitManager sessionUnitManager,
    IUnitOfWorkManager unitOfWorkManager,
    IFollowManager followManager,
    IBackgroundJobManager backgroundJobManager,
    ISessionRepository sessionRepository,
    ISettingProvider settingProvider,
    IJsonSerializer jsonSerializer,
    IRepository<MessageReminder> messageReminderRepository,
    IReadOnlyBasicRepository<Message, long> messageReadOnlyRepository,
    ISessionUnitSettingRepository sessionUnitSettingRepository,
    ISessionGenerator sessionGenerator) : DomainService, IMessageManager
{
    protected IObjectMapper ObjectMapper { get; } = objectMapper;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IMessageRepository Repository { get; } = repository;
    public IShortIdGenerator ShortIdGenerator { get; } = shortIdGenerator;
    public IReadOnlyBasicRepository<Message, long> MessageReadOnlyRepository { get; } = messageReadOnlyRepository;
    protected IMessageValidator MessageValidator { get; } = messageValidator;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected IChatPusher ChatPusher { get; } = chatPusher;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    protected ISessionRepository SessionRepository { get; } = sessionRepository;
    protected ISessionUnitSettingRepository SessionUnitSettingRepository { get; } = sessionUnitSettingRepository;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    protected ISettingProvider SettingProvider { get; } = settingProvider;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IRepository<MessageReminder> MessageReminderRepository { get; } = messageReminderRepository;

    protected ISessionGenerator SessionGenerator { get; } = sessionGenerator;

    protected virtual void TryToSetOwnerId<T, TKey>(T entity, TKey ownerId)
    {
        if (entity is IChatOwner<TKey>)
        {
            var propertyInfo = entity.GetType().GetProperty(nameof(IChatOwner<TKey>.OwnerId));

            if (propertyInfo == null || propertyInfo.GetSetMethod(true) == null)
            {
                return;
            }

            propertyInfo.SetValue(entity, ownerId);
        }
    }

    /// <inheritdoc />
    public virtual async Task CreateSessionUnitByMessageAsync(SessionUnit senderSessionUnit)
    {
        //ShopKeeper
        if (senderSessionUnit.Destination?.ObjectType == ChatObjectTypeEnums.ShopKeeper)
        {
            await SessionGenerator.AddShopWaitersIfNotContains(senderSessionUnit.Session, senderSessionUnit.Owner, senderSessionUnit.DestinationId.Value);
        }
        await Task.Yield();
    }

    /// <inheritdoc />
    public virtual async Task<Message> CreateMessageAsync(
        SessionUnit senderSessionUnit,
        Func<Message, Task<IContentEntity>> action,
        Guid? receiverSessionUnitId = null,
        long? quoteMessageId = null,
        List<Guid> remindList = null)
    {

        Assert.If(!await SettingProvider.IsTrueAsync(ChatSettings.IsMessageSenderEnabled), $"MessageSender main switch is off.");

        Assert.NotNull(senderSessionUnit, $"Unable to send message, senderSessionUnit is null");

        Assert.If(!senderSessionUnit.Setting.IsInputEnabled, $"Unable to send message, input status is disabled,senderSessionUnitId:{senderSessionUnit.Id}");

        Assert.If(senderSessionUnit.Setting.MuteExpireTime > Clock.Now, $"Unable to send message,sessionUnit has been muted.senderSessionUnitId:{senderSessionUnit.Id}");

        // Create SessionUnit By Message
        await CreateSessionUnitByMessageAsync(senderSessionUnit);

        //cache
        //await SessionUnitManager.GetOrAddCacheListAsync(senderSessionUnit.SessionId.Value);

        var message = new Message(senderSessionUnit)
        {
            CreationTime = Clock.Now
        };
        message.SetShortId(shortId: ShortIdGenerator.Create());

        //senderSessionUnit.Setting.SetLastSendMessage(message);//并发时可能导致锁表

        // 私有消息
        if (receiverSessionUnitId.HasValue)
        {
            message.SetPrivateMessage(receiverSessionUnitId.Value);
        }

        //quote message
        if (quoteMessageId.HasValue && quoteMessageId.Value > 0)
        {
            var quoteMessage = await Repository.GetAsync(quoteMessageId.Value);

            Assert.If(quoteMessage.SessionId != senderSessionUnit.SessionId, $"QuoteMessageId:{quoteMessageId.Value} not in same session");

            message.SetQuoteMessage(quoteMessage);
        }

        // message content
        var messageContent = await action(message);

        Assert.NotNull(messageContent, $"Message content is null");

        //TryToSetOwnerId(messageContent, senderSessionUnit.SessionUnitId);
        messageContent.SetOwnerId(senderSessionUnit.OwnerId);

        messageContent.SetId(GuidGenerator.Create());

        message.SetMessageContent(messageContent);

        var contentJson = JsonSerializer.Serialize(message.GetContentDto());

        message.SetContentJson(contentJson);

        //设置提醒
        await ApplyRemindIdListAsync(senderSessionUnit, message, remindList);

        // Message Validator
        await MessageValidator.CheckAsync(message);

        //sessionUnitCount
        var sessionUnitCount = message.IsPrivate ? 2 : await SessionUnitManager.GetCountBySessionIdAsync(senderSessionUnit.SessionId.Value);

        message.SetSessionUnitCount(sessionUnitCount);

        await Repository.InsertAsync(message, autoSave: true);

        // LastMessage
        await SessionRepository.UpdateLastMessageIdAsync(senderSessionUnit.SessionId.Value, message.Id);

        // Last send message
        await SessionUnitSettingRepository.UpdateLastSendMessageAsync(senderSessionUnit.Id, message.Id, message.CreationTime);

        //更新引用次数
        await UpdateQuoteCountAsync(message.QuotePath);

        ////更新转发次数
        await UpdateForwardCountAsync(message.ForwardPath);

        ////以下可能导致锁表
        //await SessionUnitRepository.UpdateLastMessageIdAsync(senderSessionUnit.Id, message.Id);

        //await CurrentUnitOfWork.SaveChangesAsync();

        //var isPrivateMessage = message.IsPrivateMessage();
        //// Args
        //var sessionUnitIncrementJobArgs = new SessionUnitIncrementJobArgs()
        //{
        //    SessionId = message.SessionId.Value,
        //    SenderSessionUnitId = message.SenderSessionUnitId.Value,
        //    RemindSessionUnitIdList = message.MessageReminderList.Select(x => x.SessionUnitId).ToList(),
        //    PrivateBadgeSessionUnitIdList = isPrivateMessage ? [message.ReceiverSessionUnitId.Value] : [],
        //    FollowingSessionUnitIdList = !isPrivateMessage ? await FollowManager.GetFollowerIdListAsync(message.SenderSessionUnitId.Value) : [],
        //    LastMessageId = message.Id,
        //    IsRemindAll = message.IsRemindAll,
        //    MessageCreationTime = message.CreationTime
        //};

        //Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)}:{JsonSerializer.Serialize(sessionUnitIncrementJobArgs)}");

        //if (await ShouldbeBackgroundJobAsync(senderSessionUnit, message))
        //{
        //    var jobId = await BackgroundJobManager.EnqueueAsync(sessionUnitIncrementJobArgs);
        //    Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)} backgroupJobId:{jobId}");
        //}
        //else
        //{
        //    Logger.LogWarning($"BackgroundJobManager.IsAvailable():False");
        //    await SessionUnitManager.IncremenetAsync(sessionUnitIncrementJobArgs);
        //}

        return message;
    }

    /// <summary>
    /// 更新转发次数
    /// </summary>
    /// <param name="forwardPath"></param>
    /// <returns></returns>
    protected virtual async Task UpdateForwardCountAsync(string forwardPath)
    {
        var messageIdList = await ResolveMessageIdAsync(forwardPath);

        if (messageIdList.Count != 0)
        {
            await Repository.IncrementForwardCountAsync(messageIdList);
        }
    }

    /// <summary>
    /// 更新引用次数
    /// </summary>
    /// <param name="quotePath"></param>
    /// <returns></returns>
    protected virtual async Task UpdateQuoteCountAsync(string quotePath)
    {
        var messageIdList = await ResolveMessageIdAsync(quotePath);

        if (messageIdList.Count != 0)
        {
            await Repository.IncrementQuoteCountAsync(messageIdList);
        }
    }

    /// <summary>
    /// 解析消息Id
    /// </summary>
    /// <param name="messageIdPath"></param>
    /// <returns></returns>
    protected virtual Task<List<long>> ResolveMessageIdAsync(string messageIdPath)
    {
        var messageIdList = new List<long>();

        if (!messageIdPath.IsNullOrWhiteSpace())
        {
            messageIdList = messageIdPath.Split(Message.Delimiter)
                .Where(x => long.TryParse(x, out _))
                .Select(long.Parse)
                .ToList();
        }

        return Task.FromResult(messageIdList);
    }

    //protected virtual async Task<bool> ShouldbeBackgroundJobAsync(SessionUnit senderSessionUnit, Message message)
    //{
    //    await Task.Yield();

    //    return BackgroundJobManager.IsAvailable();

    //    //var useBackgroundJobSenderMinSessionUnitCount = await SettingProvider.GetWalletAsync<int>(ChatSettings.UseBackgroundJobSenderMinSessionUnitCount);

    //    //return BackgroundJobManager.IsAvailable() && !message.IsPrivate && message.SessionUnitCount > useBackgroundJobSenderMinSessionUnitCount;

    //    ////return false;
    //}

    //protected virtual async Task BatchUpdateSessionUnitAsync(SessionUnit senderSessionUnit, Message message)
    //{
    //    Logger.LogInformation($"BatchUpdateSessionUnitAsync");

    //    await SessionUnitManager.UpdateCachesAsync(senderSessionUnit, message);

    //    if (await ShouldbeBackgroundJobAsync(senderSessionUnit, message))
    //    {
    //        var jobId = await BackgroundJobManager.EnqueueAsync(new UpdateStatsForSessionUnitArgs()
    //        {
    //            SenderSessionUnitId = senderSessionUnit.Id,
    //            MessageId = message.Id,
    //        });

    //        Logger.LogInformation($"JobId:{jobId}");

    //        return;
    //    }
    //    //
    //    await SessionUnitManager.BatchUpdateAsync(senderSessionUnit, message);
    //}


    [GeneratedRegex("@([^@ ]+) ?")]
    private static partial Regex RemindNameRegex();

    /// <summary>
    /// 提取提醒人Id列表
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual async Task<List<Guid>> ApplyReminderIdListForTextContentAsync(SessionUnit senderSessionUnit, Message message)
    {
        var unitIdList = new List<Guid>();
        //@XXX
        if (message.MessageType != MessageTypes.Text)
        {
            return unitIdList;
        }
        //Guid.TryParse(message.Receiver, out Guid roomId);
        var textContent = message.GetContentEntity() as TextContent;

        Assert.If(textContent == null, "TextContent is null");

        var text = textContent.Text;

        var reg = RemindNameRegex();

        //例如我想提取 @中的NAME值
        var match = reg.Match(text);

        var nameList = new List<string>();

        for (var i = 0; i < match.Groups.Count; i++)
        {
            string value = match.Groups[i].Value;

            if (!value.IsNullOrWhiteSpace() && !value.StartsWith("@"))
            {
                nameList.Add(value);
            }
        }
        if (nameList.Count == 0)
        {
            return unitIdList;
        }
        var textList = new string[] { "所有人", "everyone" };

        if (nameList.Any(x => textList.Contains(x)))
        {
            //creator or manager
            //if (senderSessionUnit.Setting.IsCreator)
            //{
            message.SetRemindAll();
            //}
            return unitIdList;
        }

        unitIdList = await SessionUnitManager.GetIdListByNameAsync(senderSessionUnit.SessionId.Value, nameList);

        return unitIdList;
    }

    /// <summary>
    /// 设置提醒
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="message"></param>
    /// <param name="remindIdList"></param>
    /// <returns></returns>
    protected virtual async Task<List<Guid>> ApplyRemindIdListAsync(SessionUnit senderSessionUnit, Message message, List<Guid> remindIdList)
    {
        //私有消息不设置提醒
        if (message.IsPrivateMessage())
        {
            return [];
        }

        var finalRemindIdList = await ApplyReminderIdListForTextContentAsync(senderSessionUnit, message);

        if (remindIdList.IsAny())
        {
            finalRemindIdList = finalRemindIdList.Concat(remindIdList).Distinct().ToList();
        }

        if (finalRemindIdList.Count == 0)
        {
            return [];
        }

        message.SetReminder(finalRemindIdList, ReminderTypes.Normal);

        //await SessionUnitRepository.IncrementRemindMeCountAsync(message.CreationTime, finalRemindIdList);

        return finalRemindIdList;
    }

    /// <inheritdoc />
    public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnit senderSessionUnit,
        MessageInput<TContentInfo> input)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity
    {
        var messageContent = ObjectMapper.Map<TContentInfo, TContentEntity>(input.Content);
        return await SendAsync<TContentInfo, TContentEntity>(senderSessionUnit, input, messageContent);
    }

    /// <inheritdoc />
    public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnit senderSessionUnit,
        MessageInput input,
        TContentEntity contentEntity)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity
    {
        var message = await CreateMessageAsync(senderSessionUnit,
            async (entity) => await Task.FromResult(contentEntity),
            quoteMessageId: input.QuoteMessageId,
            remindList: input.RemindList,
            receiverSessionUnitId: input.ReceiverSessionUnitId);

        var cacheKey = await SessionUnitManager.GetCacheKeyByMessageAsync(message);

        await SessionUnitManager.GetOrAddByMessageAsync(message);

        await DistributedEventBus.PublishAsync(new MessageCreatedEto()
        {
            CacheKey = cacheKey,
            MessageId = message.Id,
            HostName = CurrentHosted.Name,
        });

        //var output = ObjectMapper.Map<Message, MessageInfo<object>>(message);
        var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);
        //var output = new MessageInfo<TContentInfo>() { Id = message.Id };

        if (message.IsPrivateMessage())
        {
            var receiverSessionUnit = await SessionUnitManager.GetAsync(input.ReceiverSessionUnitId.Value);

            Assert.If(receiverSessionUnit.SessionId != senderSessionUnit.SessionId, $"Fail ReceiverSessionUnitId:{input.ReceiverSessionUnitId}");

            await ChatPusher.ExecutePrivateAsync(
            [
                senderSessionUnit, receiverSessionUnit
            ], output, input.IgnoreConnections);
        }
        else
        {
            await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, output, input.IgnoreConnections);
        }
        return output;
    }

    /// <inheritdoc />
    public virtual async Task<Dictionary<string, long>> RollbackAsync(Message message)
    {
        int allowRollbackHours = await SettingProvider.GetAsync<int>(ChatSettings.AllowRollbackHours);

        var nowTime = Clock.Now;

        //var message = await Repository.GetWalletAsync(messageId);

        //Assert.If(message.Sender != LoginInfo.UserId, $"无权限撤回别人消息！");

        Assert.If(nowTime > message.CreationTime.AddHours(allowRollbackHours), $"超过{allowRollbackHours}小时的消息不能被撤回！");

        message.Rollback(nowTime);

        //await Repository.UpdateAsync(message, true);
        await UnitOfWorkManager.Current.SaveChangesAsync();

        return await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, new RollbackMessageCommandPayload
        {
            MessageId = message.Id,
        });
    }

    /// <inheritdoc />
    public virtual async Task<List<Message>> ForwardAsync(Guid sessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList)
    {
        var currentSessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        Assert.If(!currentSessionUnit.Setting.IsEnabled, $"Current session unit disabled.", nameof(currentSessionUnit.Setting.IsEnabled));

        var sourceMessage = await Repository.GetAsync(sourceMessageId);

        Assert.If(sourceMessage.IsRollbackMessage(), $"message already rollback：{sourceMessageId}", nameof(currentSessionUnit.Setting.IsEnabled));

        Assert.If(sourceMessage.IsDisabledForward(), $"MessageType:'{sourceMessage.MessageType}' is disabled forward");

        Assert.If(sourceMessage.IsPrivateMessage(), $"Private messages cannot be forward");

        Assert.If(currentSessionUnit.SessionId != sourceMessage.SessionId, $"The sender and message are not in the same session, messageSessionId:{sourceMessage.SessionId}", nameof(currentSessionUnit.SessionId));

        var messageContent = sourceMessage.GetTypedContentEntity();

        Assert.NotNull(messageContent, $"MessageContent is null. Source message:{sourceMessage}");

        var messageList = new List<Message>();

        var args = new List<(Guid, MessageInfo<object>)>();

        foreach (var targetSessionUnitId in targetSessionUnitIdList.Distinct())
        {
            var targetSessionUnit = await SessionUnitManager.GetAsync(targetSessionUnitId);

            Assert.If(!targetSessionUnit.Setting.IsEnabled, $"Target session unit disabled,id:{targetSessionUnit.Id}");

            Assert.If(!targetSessionUnit.Setting.IsInputEnabled, $"Target session unit input state is disabled,id:{targetSessionUnit.Id}");

            Assert.If(currentSessionUnit.OwnerId != targetSessionUnit.OwnerId, $"[TargetSessionUnitId:{targetSessionUnitId}] is fail.");

            var newMessage = await CreateMessageAsync(targetSessionUnit, async (x) =>
            {
                x.SetForwardMessage(sourceMessage);
                await Task.Yield();
                return messageContent;
            });
            messageList.Add(newMessage);

            var output = ObjectMapper.Map<Message, MessageInfo<object>>(newMessage);

            //var output = new MessageInfo<object>() { Id = newMessage.Id };

            args.Add((newMessage.SessionId.Value, output));
        }

        //push
        foreach (var arg in args)
        {
            await ChatPusher.ExecuteBySessionIdAsync(arg.Item1, arg.Item2, null);
        }

        return messageList;
    }

    /// <inheritdoc />
    public Task<bool> IsRemindAsync(long messageId, Guid sessionUnitId)
    {
        return MessageReminderRepository.AnyAsync(x => x.MessageId == messageId && x.SessionUnitId == sessionUnitId);
    }
}
