using IczpNet.Chat.Ai;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Developers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageReports;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
    IMessageReportManager messageReportManager,
    IMessageManager messageManager,
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
    public IMessageReportManager MessageReportManager { get; } = messageReportManager;
    public IMessageManager MessageManager { get; } = messageManager;
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected ICurrentHosted CurrentHosted { get; } = currentHosted;

    private readonly ConcurrentDictionary<string, long> ExecutedMilliseconds = [];

    public async Task HandleEventAsync(MessageSentEto eventData)
    {
        try
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

            Logger.LogWarning("HandleEventAsync Start: MessageId={Id}, Thread={Thread}", eventData.Id, Environment.CurrentManagedThreadId);

            // 分布式事件要开启工作单元
            using var uow = UnitOfWorkManager.Begin();

            Logger.LogWarning("After UOW: MessageId={Id}", eventData.Id);

            // 解决“事务提交可见性延迟”问题（即 onUnitOfWorkComplete 为 true 但数据库还没查到）
            var message = await FindWithRetryAsync(eventData.Id);

            if (message == null)
            {
                Logger.LogError("Message is Null: MessageId={Id}", eventData.Id);
                return;
            }

            Logger.LogWarning("MessageRepository.GetAsync: MessageId={Id}", message.Id);

            var messageCacheItem = await MessageManager.GetCacheAsync(eventData.Id);

            //统计消息
            await MeasureAsync($"{nameof(StatMessageAsync)}", () => StatMessageAsync(messageCacheItem));

            //缓存会话单元计数增量
            await MeasureAsync($"{nameof(CachingUnitsAsync)}", () => CachingUnitsAsync(messageCacheItem));

            //// 批量更新缓存中的会话单元计数
            await MeasureAsync($"{nameof(BatchIncrementForCacheAsync)}", () => BatchIncrementForCacheAsync(messageCacheItem, eventData.ReminderIdList, eventData.FollowerIdList));

            // 后台任务:数据库会话单元计数增量
            await MeasureAsync($"{nameof(EnqueueSessionUnitIncrementJobAsync)}", () => EnqueueSessionUnitIncrementJobAsync(messageCacheItem, eventData.ReminderIdList, eventData.FollowerIdList));

            // 后台任务:开发者
            await MeasureAsync($"{nameof(EnqueueDeveloperJobAsync)}", () => EnqueueDeveloperJobAsync(messageCacheItem));

            // 后台任务:AI
            await MeasureAsync($"{nameof(EnqueueAiJobAsync)}", () => EnqueueAiJobAsync(messageCacheItem));

            // 推送消息到客户端分布式事件
            await MeasureAsync($"{nameof(PublishSendToClientDistributedAsync)}", () => PublishSendToClientDistributedAsync(messageCacheItem, eventData.ReminderIdList, eventData.FollowerIdList));

            var totalExecutedMilliseconds = ExecutedMilliseconds.Sum(x => x.Value);

            await uow.CompleteAsync();

            Logger.LogWarning("Complete UOW: MessageId={Id}", eventData.Id);

            Logger.LogInformation($"[{HandlerName}] TotalExecutedMilliseconds: {totalExecutedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "HandleEventAsync Failed for MessageId={Id}: {Error}", eventData.Id, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 带有重试逻辑的消息查询
    /// </summary>
    private async Task<Message> FindWithRetryAsync(long id, int maxRetries = 10, int delayMs = 500)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            var entity = await MessageRepository.FindAsync(id);
            if (entity != null)
            {
                return entity;
            }
            Logger.LogWarning($"FindWithRetryAsync [Retry {i + 1}] MessageId={id} not found, waiting {delayMs}ms...");
            await Task.Delay(delayMs);
        }
        return null;
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

    protected virtual async Task<bool> StatMessageAsync(MessageCacheItem message)
    {
        await MessageReportManager.StatAsync(message);

        //using var uow = UnitOfWorkManager.Begin();
        //await MessageReportManager.IncrementAsync(message);
        //await uow.CompleteAsync(); //  提前提交

        return true;
    }


    protected virtual async Task<IEnumerable<SessionUnitCacheItem>> CachingUnitsAsync(MessageCacheItem message)
    {
        return await SessionUnitManager.LoadMembersIfNotExistsAsync(message.SessionId);
    }

    /// <summary>
    /// 批量更新缓存中的会话单元计数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="reminderIdList"></param>
    /// <param name="followerIdList"></param>
    /// <returns></returns>
    protected async Task<bool> BatchIncrementForCacheAsync(MessageCacheItem message, List<Guid> reminderIdList, List<Guid> followerIdList)
    {
        await SessionUnitCacheManager.BatchIncrementAsync(message, reminderIds: reminderIdList, followerIds: followerIdList);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="remindSessionUnitIdList"></param>
    /// <param name="followingSessionUnitIdList"></param>
    /// <returns></returns>
    protected async Task<bool> EnqueueSessionUnitIncrementJobAsync(MessageCacheItem message, List<Guid> remindSessionUnitIdList, List<Guid> followingSessionUnitIdList)
    {
        var isPrivateMessage = message.IsPrivate;
        //var remindSessionUnitIdList = message.MessageReminderList.Select(x => x.SessionUnitId).ToList();
        //var remindSessionUnitIdList = new List<Guid>();
        //var followingSessionUnitIdList = await FollowManager.GetFollowerIdListAsync(message.SenderSessionUnitId.Value);
        // Args
        var sessionUnitIncrementJobArgs = new SessionUnitIncrementJobArgs()
        {
            SessionId = message.SessionId,
            OwnerId = message.SenderId.Value,
            SenderSessionUnitId = message.SenderSessionUnitId.Value,
            RemindSessionUnitIdList = remindSessionUnitIdList,
            PrivateBadgeSessionUnitIdList = isPrivateMessage ? [message.ReceiverSessionUnitId.Value] : [],
            FollowingSessionUnitIdList = !isPrivateMessage ? followingSessionUnitIdList : [],
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
    public async Task<bool> EnqueueDeveloperJobAsync(MessageCacheItem message)
    {
        var senderSessionUnit = await SessionUnitManager.GetCacheAsync(message.SenderSessionUnitId.Value);

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

    protected virtual async Task<bool> PublishSendToClientDistributedAsync(MessageCacheItem message, List<Guid> remindSessionUnitIdList, List<Guid> followingSessionUnitIdList)
    {
        var command = message.ForwardMessageId.HasValue ? CommandConsts.MessageForwarded : CommandConsts.MessageCreated;

        var eventData = new SendMessageToClientDistributedEto()
        {
            Command = command.ToString(),
            //CacheKey = cacheKey,
            HostName = CurrentHosted.Name,
            MessageId = message.Id,
            Message = message,
            ReminderIdList = remindSessionUnitIdList,
            FollowerIdList = followingSessionUnitIdList,
        };

        Logger.LogInformation($"PublishMessageDistributedEventAsync-eventData:{JsonSerializer.Serialize(eventData)}");

        await DistributedEventBus.PublishAsync(eventData, onUnitOfWorkComplete: false);

        return true;
    }



    /// <summary>
    /// AI后台任务
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual async Task<bool> EnqueueAiJobAsync(MessageCacheItem message)
    {
        if (message.SenderId == message.ReceiverId)
        {
            Logger.LogInformation($"message.SenderId == message.ReceiverId:{message.ReceiverId}");
            return false;
        }

        var receiver = await ChatObjectManager.GetItemByCacheAsync(message.ReceiverId.Value);

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
