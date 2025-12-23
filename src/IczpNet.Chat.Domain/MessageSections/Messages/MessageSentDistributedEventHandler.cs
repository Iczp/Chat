using IczpNet.Chat.Ai;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Developers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageStats;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageSentDistributedEventHandler(
    IAiResolver aiResolver,
    IChatObjectManager chatObjectManager,
    IDistributedEventBus distributedEventBus,
    IObjectMapper objectMapper,
    IDeveloperManager developerManager,
    IUnitOfWorkManager unitOfWorkManager,
    IBackgroundJobManager backgroundJobManager,
    IIncremenetBadge incremenetBadge,
    IJsonSerializer jsonSerializer,
    IFollowManager followManager,
    ISessionUnitManager sessionUnitManager,
    IMessageStatRepository messageStatRepository,
    IMessageRepository messageRepository,
    ISessionUnitCacheManager sessionUnitCacheManager,
    ICurrentHosted currentHosted,
    IAbpDistributedLock distributedLock) :
    DomainService,
    IDistributedEventHandler<MessageSentEto>,
    ITransientDependency
{
    protected string HandlerName => $"{nameof(MessageSentDistributedEventHandler)}";

    protected IAbpDistributedLock DistributedLock { get; set; } = distributedLock;
    public IAiResolver AiResolver { get; } = aiResolver;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IDeveloperManager DeveloperManager { get; } = developerManager;
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    public IIncremenetBadge IncremenetBadge { get; } = incremenetBadge;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public IFollowManager FollowManager { get; } = followManager;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public IMessageStatRepository MessageStatRepository { get; } = messageStatRepository;
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected ICurrentHosted CurrentHosted { get; } = currentHosted;

    private readonly Dictionary<string, long> ExecutedMilliseconds = [];

    public async Task HandleEventAsync(MessageSentEto eventData)
    {
        Logger.LogInformation($"Handle HostName:{CurrentHosted.Name},eventData:{eventData}");

        var lockerName = $"{HandlerName}-messageId-{eventData.Id}";

        await using var handle = await DistributedLock.TryAcquireAsync(lockerName);

        Logger.LogInformation("Handle=={handle},LockerName={LockerName}", handle, lockerName);

        if (handle == null)
        {
            Logger.LogInformation("Handle==null");
            return;
        }

        var s = Clock.Now.Ticks - eventData.PublishTime?.Ticks ?? 0;

        Logger.LogInformation("Handle NetDelay: {ms}ms", s / 10000);

        Logger.LogWarning("HandleEventAsync: MessageId={Id}, Thread={Thread}", eventData.Id, Environment.CurrentManagedThreadId);

        // 分布式事件要开启工作单元
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        var message = await MessageRepository.GetAsync(eventData.Id);

        //统计消息
        await MeasureAsync($"{nameof(StatMessageAsync)}", () => StatMessageAsync(message));

        //缓存会话单元计数增量
        await MeasureAsync($"{nameof(CachingUnitsAsync)}", () => CachingUnitsAsync(message));

        // 批量更新缓存中的会话单元计数
        await MeasureAsync($"{nameof(BatchIncrementForCacheAsync)}", () => BatchIncrementForCacheAsync(message));

        // 后台任务:数据库会话单元计数增量
        await MeasureAsync($"{nameof(EnqueueSessionUnitIncrementJobAsync)}", () => EnqueueSessionUnitIncrementJobAsync(message));

        // 后台任务:开发者
        await MeasureAsync($"{nameof(EnqueueDeveloperJobAsync)}", () => EnqueueDeveloperJobAsync(message));

        // 后台任务:AI
        await MeasureAsync($"{nameof(EnqueueAiJobAsync)}", () => EnqueueAiJobAsync(message));

        // 推送消息到客户端分布式事件
        await MeasureAsync($"{nameof(PublishSendToClientDistributedAsync)}", () => PublishSendToClientDistributedAsync(message));

        var totalExecutedMilliseconds = ExecutedMilliseconds.Sum(x => x.Value);

        await uow.CompleteAsync();

        Logger.LogInformation($"[{HandlerName}] TotalExecutedMilliseconds: {totalExecutedMilliseconds}ms");
    }

    protected virtual async Task<T> MeasureAsync<T>(string name, Func<Task<T>> func)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var result = await func();
            Logger.LogInformation("[{HandlerName}] [{name}] Elapsed Time: {ms} ms", HandlerName, name, sw.ElapsedMilliseconds);
            ExecutedMilliseconds.TryAdd(name, sw.ElapsedMilliseconds);
            sw.Stop();
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exe: {name} Error:{ex}", name, ex);
            return default;
        }
    }

    protected virtual async Task<bool> StatMessageAsync(Message message)
    {
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        var sessionId = message.SessionId.Value;
        await MessageStatRepository.IncrementAsync(sessionId, message.MessageType, "yyyyMM");
        await MessageStatRepository.IncrementAsync(sessionId, message.MessageType, "yyyyMMdd");
        await MessageStatRepository.IncrementAsync(sessionId, message.MessageType, "yyyyMMddHH");
        await uow.CompleteAsync(); //  提前提交
        return true;
    }


    protected virtual async Task<IEnumerable<SessionUnitCacheItem>> CachingUnitsAsync(Message message)
    {
        var sessionId = message.SessionId.Value;
        return await SessionUnitCacheManager.SetListBySessionIfNotExistsAsync(
            sessionId,
            async (sessionId) =>
            {
                return await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId);
            });
    }

    /// <summary>
    /// 批量更新缓存中的会话单元计数
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected async Task<bool> BatchIncrementForCacheAsync(Message message)
    {
        await SessionUnitCacheManager.BatchIncrementAsync(message);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected async Task<bool> EnqueueSessionUnitIncrementJobAsync(Message message)
    {
        var isPrivateMessage = message.IsPrivateMessage();
        // Args
        var sessionUnitIncrementJobArgs = new SessionUnitIncrementJobArgs()
        {
            SessionId = message.SessionId.Value,
            OwnerId = message.SenderId.Value,
            SenderSessionUnitId = message.SenderSessionUnitId.Value,
            RemindSessionUnitIdList = message.MessageReminderList.Select(x => x.SessionUnitId).ToList(),
            PrivateBadgeSessionUnitIdList = isPrivateMessage ? [message.ReceiverSessionUnitId.Value] : [],
            FollowingSessionUnitIdList = !isPrivateMessage ? await FollowManager.GetFollowerIdListAsync(message.SenderSessionUnitId.Value) : [],
            LastMessageId = message.Id,
            IsRemindAll = message.IsRemindAll,
            MessageCreationTime = message.CreationTime
        };

        Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)}:{JsonSerializer.Serialize(sessionUnitIncrementJobArgs)}");

        var jobId = await BackgroundJobManager.EnqueueAsync(sessionUnitIncrementJobArgs);
        Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)} backgroupJobId:{jobId},messageId={message.Id}");

        //if (await IncremenetBadge.ShouldbeBackgroundJobAsync(message))
        //{
        //    var jobId = await BackgroundJobManager.EnqueueAsync(sessionUnitIncrementJobArgs);
        //    Logger.LogInformation($"{nameof(SessionUnitIncrementJobArgs)} backgroupJobId:{jobId},messageId={message.Id}");
        //}
        //else
        //{
        //    Logger.LogWarning($"BackgroundJobManager.IsAvailable():False, messageId={message.Id}");
        //    await SessionUnitManager.IncremenetAsync(sessionUnitIncrementJobArgs);
        //}
        return true;
    }

    /// <summary>
    /// 开发者后台任务
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<bool> EnqueueDeveloperJobAsync(Message message)
    {
        var senderSessionUnit = await SessionUnitManager.GetAsync(message.SenderSessionUnitId.Value);

        if (message.SenderId == message.ReceiverId)
        {
            Logger.LogInformation($"message.SenderId == message.ReceiverId:{message.ReceiverId}");
            return false;
        }

        if (!await DeveloperManager.IsEnabledAsync(message.ReceiverId))
        {
            Logger.LogInformation($"Message.ReceiverId:{message.ReceiverId},Developer is disabled.");
            return false;
        }

        //var receiver = await ChatObjectManager.GetAsync(message.ReceiverId.Value);
        if (!BackgroundJobManager.IsAvailable())
        {
            Logger.LogWarning($"BackgroundJob IsAvailable: False.");
            return false;
        }

        var developerJobArg = new DeveloperJobArg()
        {
            EventType = EventTypes.Created,
            MessageId = message.Id,
        };
        var jobId = await BackgroundJobManager.EnqueueAsync(developerJobArg);

        Logger.LogInformation($"Message is jobId:{jobId},{nameof(DeveloperJobArg)}:{developerJobArg}");

        return true;
    }

    /// <summary>
    /// 推送消息到客户端分布式事件
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual async Task<bool> PublishSendToClientDistributedAsync(Message message)
    {
        var dbMessage = message;

        var messageDto = ObjectMapper.Map<Message, MessageInfo<object>>(dbMessage);

        //fix: 导航属性没有加载完全 改为手动转换Map
        if (dbMessage.SenderSessionUnit == null && dbMessage.SenderSessionUnitId.HasValue)
        {
            var senderSessionUnit = await SessionUnitManager.GetAsync(dbMessage.SenderSessionUnitId.Value);
            messageDto.SenderSessionUnit = ObjectMapper.Map<SessionUnit, SessionUnitSenderInfo>(senderSessionUnit);
        }

        messageDto.Content ??= message.GetContentDto();

        var command = message.ForwardMessageId.HasValue ? CommandConsts.MessageForwarded : CommandConsts.MessageCreated;

        var eventData = new SendToClientDistributedEto()
        {
            Command = command.ToString(),
            //CacheKey = cacheKey,
            HostName = CurrentHosted.Name,
            MessageId = message.Id,
            Message = messageDto
        };

        Logger.LogInformation($"PublishMessageDistributedEventAsync-eventData:{JsonSerializer.Serialize(eventData)}");

        //await SessionUnitManager.GetOrAddByMessageAsync(message);

        await DistributedEventBus.PublishAsync(eventData, onUnitOfWorkComplete: false);

        return true;
    }

    /// <summary>
    /// AI后台任务
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual async Task<bool> EnqueueAiJobAsync(Message message)
    {
        if (message.SenderId == message.ReceiverId)
        {
            Logger.LogInformation($"message.SenderId == message.ReceiverId:{message.ReceiverId}");
            return false;
        }

        var receiver = await ChatObjectManager.GetAsync(message.ReceiverId.Value);

        if (!AiResolver.HasProvider(receiver.Code))
        {
            Logger.LogInformation($"Not Ai provider:{receiver.Code}");
            return false;
        }

        if (!BackgroundJobManager.IsAvailable())
        {

            Logger.LogWarning($"BackgroundJob IsAvailable: False.");
            return false;
        }
        var aiJobArg = new AiJobArg()
        {
            Provider = receiver.Code,
            EventType = EventTypes.Created,
            MessageId = message.Id,
        };
        var jobId = await BackgroundJobManager.EnqueueAsync(aiJobArg);

        Logger.LogInformation($"Message is jobId:{jobId},{nameof(AiJobArg)}:{aiJobArg}");

        return true;
    }

}
