using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatPushers;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.Settings;
using IczpNet.Chat.Ulids;
using IczpNet.Pusher.ShortIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class MessageManager(
    IDeletedRecorderManager deletedRecorderManager,
    IMessageRepository repository,
    IShortIdGenerator shortIdGenerator,
    IObjectMapper objectMapper,
    IMessageValidator messageValidator,
    IChatPusher chatPusher,
    IDistributedEventBus distributedEventBus,
    ICurrentHosted currentHosted,
    ISessionUnitManager sessionUnitManager,
    ISessionUnitCacheManager sessionUnitCacheManager,
    IUnitOfWorkManager unitOfWorkManager,
    ISessionRepository sessionRepository,
    IChatObjectRepository chatObjectRepository,
    ISettingProvider settingProvider,
    IJsonSerializer jsonSerializer,
    IRepository<MessageReminder> messageReminderRepository,
    ISessionUnitSettingRepository sessionUnitSettingRepository,
    IFollowManager followManager,
    IDistributedCache<SessionMaxMessageIdCacheItem, SessionMaxMessageIdCacheKey> sessionMaxMessageIdCache,
    IDistributedCache<MessageCacheItem, MessageCacheKey> messageCache,
    IOptions<MessageOptions> options,
    IUlidGenerator ulidGenerator,
    ISessionGenerator sessionGenerator) : DomainService, IMessageManager
{
    protected IObjectMapper ObjectMapper { get; } = objectMapper;
    public IDeletedRecorderManager DeletedRecorderManager { get; } = deletedRecorderManager;
    protected IMessageRepository Repository { get; } = repository;
    public IShortIdGenerator ShortIdGenerator { get; } = shortIdGenerator;
    protected IMessageValidator MessageValidator { get; } = messageValidator;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected IChatPusher ChatPusher { get; } = chatPusher;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    protected ISessionRepository SessionRepository { get; } = sessionRepository;
    public IChatObjectRepository ChatObjectRepository { get; } = chatObjectRepository;
    protected ISessionUnitSettingRepository SessionUnitSettingRepository { get; } = sessionUnitSettingRepository;
    public IFollowManager FollowManager { get; } = followManager;
    public IDistributedCache<SessionMaxMessageIdCacheItem, SessionMaxMessageIdCacheKey> SessionMaxMessageIdCache { get; } = sessionMaxMessageIdCache;
    public IDistributedCache<MessageCacheItem, MessageCacheKey> MessageCache { get; } = messageCache;
    public IOptions<MessageOptions> Options { get; } = options;
    public IUlidGenerator UlidGenerator { get; } = ulidGenerator;

    public MessageOptions Config => Options.Value;
    protected ISettingProvider SettingProvider { get; } = settingProvider;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IRepository<MessageReminder> MessageReminderRepository { get; } = messageReminderRepository;
    protected ISessionGenerator SessionGenerator { get; } = sessionGenerator;
    protected virtual DistributedCacheEntryOptions CacheOptions => Config.CacheOptions;

    /// <summary>
    /// 通用游标分片器
    /// </summary>
    /// <param name="baseQuery"></param>
    /// <param name="minMessageId"></param>
    /// <param name="maxMessageId"></param>
    /// <param name="batchSize"></param>
    /// <param name="max"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async IAsyncEnumerable<List<long>> LoadMessageIdsAsync(IQueryable<Message> baseQuery, long? minMessageId = null, long? maxMessageId = null, int batchSize = 1000, int max = int.MaxValue, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        long? cursor = maxMessageId;
        var total = 0;
        var batchIndex = 0;
        var totalSw = Stopwatch.StartNew();
        var method = nameof(LoadMessageIdsAsync);
        while (total < max)
        {
            batchIndex++;

            var swQuery = Stopwatch.StartNew();

            var takeSize = Math.Min(batchSize, max - total);

            var batch = await baseQuery
                .AsNoTracking()
                .WhereIf(cursor.HasValue, x => x.Id < cursor.Value)
                //  不包含 minMessageId
                .WhereIf(minMessageId.HasValue, x => x.Id > minMessageId.Value)
                //倒序
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .Take(takeSize)
                .ToListAsync(cancellationToken);
            swQuery.Stop();

            if (batch.Count == 0)
            {
                Logger.LogInformation(
                       "{Method} finished. batches={BatchIndex}, total={TotalCount}, elapsed={Elapsed}ms",
                       method,
                       batchIndex - 1,
                       total,
                       totalSw.ElapsedMilliseconds);
                yield break;
            }

            Logger.LogInformation(
                    "{Method} batchSize={batchSize}, batch#{BatchIndex} batch count={Count} rows in Elapsed={Elapsed}ms",
                    method,
                    batchSize,
                    batchIndex,
                    batch.Count,
                    swQuery.ElapsedMilliseconds);

            cursor = batch[^1];

            total += batch.Count;

            yield return batch;

            // 已经达到最大数量
            if (total >= max)
            {
                yield break;
            }
        }
    }

    public async Task<List<long>> BuildCacheAsync(Guid sessionId, long? minMessageId, long? maxMessageId, int max = 5000, int batchSize = 1000)
    {
        var method = nameof(BuildCacheAsync);

        Logger.LogInformation("[{method}] minMessageId:{maxMeminMessageIdssageId}, maxMessageId:{maxMessageId}", method, minMessageId, maxMessageId);

        var queryable = (await Repository.GetQueryableAsync())
            .Where(x => x.SessionId == sessionId);

        var result = new List<long>();

        var batchIndex = 0;

        await foreach (var batch in LoadMessageIdsAsync(
            queryable,
            minMessageId: minMessageId,
            maxMessageId: maxMessageId,
            max: max,
            batchSize: batchSize))
        {
            // 处理
            var ids = batch;
            Logger.LogInformation("[{method}] batchIndex:{batchIndex},count:{count},ids: [{start},...,{end}]", method, batchIndex, ids.Count, ids.FirstOrDefault(), ids.LastOrDefault());
            // 写入redis 倒序 保证会话消息连续性
            await SessionUnitCacheManager.AppendSessionMessagesAsync(sessionId, ids);
            batchIndex++;
            result.AddRange(ids);
        }

        return result;
    }

    public async Task<List<long>> BuildAllCacheAsync(Guid sessionId, long? minMessageId, int max = 5000, int batchSize = 1000)
    {
        var loopIndex = 0;
        var result = new List<long>();
        while (loopIndex < 1e4)
        {
            var buildMaxId = await SessionUnitCacheManager.GetMinMessageIdAsync(sessionId);
            var buildResult = await BuildCacheAsync(sessionId, minMessageId, maxMessageId: buildMaxId, max, batchSize);
            if ((buildResult.Count == 0))
            {
                break;
            }
            result.AddRange(buildResult);
            loopIndex++;
        }
        return result;
    }

    public async Task<bool> RemoveCacheAsync(Guid sessionId)
    {
        return await SessionUnitCacheManager.RemoveSessionMessagesAsync(sessionId);
    }


    public async Task<List<long>> GetHistoryAsync(SessionUnitCacheItem unit, long? maxMessageId, int maxResultCount = 20)
    {
        var sessionUnitId = unit.Id;

        var sessionId = unit.SessionId.Value;

        var queryMaxResultCount = maxResultCount;

        var tackCount = queryMaxResultCount * 3;

        var deletedIdSet = await DeletedRecorderManager.GetDeletedMessageIdListAsync(sessionUnitId);

        var result = new List<long>();

        var method = nameof(GetHistoryAsync);

        long cursorMaxMessageId = maxMessageId.HasValue && maxMessageId.Value < long.MaxValue ? maxMessageId.Value  : long.MaxValue;

        var loopIndex = 0;

        while (true)
        {
            loopIndex++;

            Logger.LogInformation(
            "[{Method}] Loop={Loop}, resultCount={ResultCount}, cursorMaxMessageId={cursorMaxMessageId}",
                method,
                loopIndex,
                result.Count,
                cursorMaxMessageId);

            var ids = await SessionUnitCacheManager.GetSessionMessagesAsync(
                    sessionId,
                    minMessageId: 0,
                    maxMessageId: cursorMaxMessageId,
                    skip: 0,
                    take: tackCount,
                    // 反序
                    isDescending: true);

            var cachedList = ids.ToList();

            Logger.LogInformation("[{Method}] Cached Result Count={Count}, First={First}, Last={Last}, Values={Values}",
                method,
                cachedList.Count,
                cachedList.FirstOrDefault(),
                cachedList.LastOrDefault(),
                string.Join(",", cachedList.Take(20)));

            // 判断是否需要补缓存
            if (cachedList.Count == 0)
            {
                // 拼接缓存,所以是取缓存里最小值
                var buildMaxId = await SessionUnitCacheManager.GetMinMessageIdAsync(sessionId);

                var buildResult = await BuildCacheAsync(sessionId, null, maxMessageId: buildMaxId, max: Math.Max(queryMaxResultCount, 5000), batchSize: 1000);

                Logger.LogInformation("[{Method}] Build cache result count={Count}, ids=[{Ids}]", method, buildResult.Count, string.Join(",", buildResult.Take(20)));

                // 数据库也没有
                if (buildResult.Count == 0)
                {
                    break;
                }

                // Build后重新查Redis
                continue;
            }

            foreach (var id in cachedList)
            {
                if (!deletedIdSet.Contains(id))
                {
                    result.Add(id);

                    if (result.Count >= queryMaxResultCount)
                    {
                        break;
                    }
                }
            }
            Logger.LogInformation("[{Method}] Add result count={Count}, ids=[{Ids}]", method, result.Count, result.Take(20).JoinAsString(","));

            // 已满足 || 缓存就没有了
            if (result.Count >= queryMaxResultCount)
            {
                break;
            }

            // 下一页游标(反序，下一页的起始值 为 当前页的最小值)
            cursorMaxMessageId = cachedList[^1] - 1;

            /*
             * Redis还有数据，但是过滤后不足
             * 继续取 Loop
             */
        }

        return result;
    }

    public async Task<List<long>> GetLatestAsync(SessionUnitCacheItem unit, long minMessageId, int maxResultCount = 20)
    {
        var sessionUnitId = unit.Id;

        var sessionId = unit.SessionId.Value;

        var queryMinMessageId = minMessageId - 1;

        var queryMaxResultCount = maxResultCount + 1;

        var deletedIdSet = await DeletedRecorderManager.GetDeletedMessageIdListAsync(sessionUnitId);

        var result = new List<long>();

        var method = nameof(GetLatestAsync);

        var loopIndex = 0;

        long cursorMaxMessageId = long.MaxValue;

        while (true)
        {
            loopIndex++;

            Logger.LogInformation(
            "[{Method}] Loop={Loop}, resultCount={ResultCount}, cursor={Cursor}, minMessageId={MinMessageId}",
                method,
                loopIndex,
                result.Count,
                cursorMaxMessageId,
                minMessageId);

            // 当前缓存查询
            var ids = await SessionUnitCacheManager.GetSessionMessagesAsync(
                sessionId,
                // 包含自己 minMessageId
                minMessageId: queryMinMessageId,
                maxMessageId: cursorMaxMessageId,
                skip: 0,
                take: queryMaxResultCount * 3,
                // 正序
                isDescending: false);

            var cachedList = ids.ToList();

            Logger.LogInformation("[{Method}] Cached Result Count={Count}, First={First}, Last={Last}, Values={Values}",
                method,
                cachedList.Count,
                cachedList.FirstOrDefault(),
                cachedList.LastOrDefault(),
                string.Join(",", cachedList.Take(20)));

            // 判断是否需要补缓存
            if (cachedList.FirstOrDefault() != minMessageId)
            {
                // 拼接缓存,所以是取缓存里最小值
                var buildMaxId = await SessionUnitCacheManager.GetMinMessageIdAsync(sessionId);

                if (buildMaxId.HasValue && buildMaxId.Value < minMessageId)
                {
                    break;
                }

                var buildResult = await BuildCacheAsync(sessionId, queryMinMessageId, maxMessageId: buildMaxId, max: 5000, batchSize: 1000);

                Logger.LogInformation("[{Method}] Build cache result count={Count}, ids=[{Ids}]", method, buildResult.Count, string.Join(",", buildResult.Take(20)));

                // 数据库也没有
                if (buildResult.Count == 0)
                {
                    break;
                }

                // Build后重新查Redis
                continue;
            }

            foreach (var id in cachedList)
            {
                if (!deletedIdSet.Contains(id))
                {
                    result.Add(id);

                    if (result.Count >= queryMaxResultCount)
                    {
                        break;
                    }
                }
            }
            Logger.LogInformation("[{Method}] Add result count={Count}, ids=[{Ids}]", method, result.Count, result.Take(20).JoinAsString(","));

            // 已满足 || 缓存就没有了
            if (result.Count >= queryMaxResultCount || cachedList.Count < queryMaxResultCount)
            {
                break;
            }

            // 下一页游标(正序，下一页的起始值 为 当前页的最小值)
            cursorMaxMessageId = cachedList[0] - 1;

            /*
             * Redis还有数据，但是过滤后不足
             * 继续取 Loop
             */
        }

        // 移除多取一条
        result.Remove(minMessageId);

        return result;
    }

    public async Task<long> GetSessionMessageTotalCountAsync(SessionUnitCacheItem unit, long minMessageId = 0, long maxMessageId = long.MaxValue)
    {
        return await SessionUnitCacheManager.GetSessionMessageTotalCountAsync(unit.SessionId.Value, minMessageId, maxMessageId);
    }

    /// <inheritdoc />
    public virtual async Task CreateSessionUnitByMessageAsync(SessionUnitCacheItem senderSessionUnit)
    {
        //ShopKeeper
        if (senderSessionUnit.DestinationObjectType == ChatObjectTypeEnums.ShopKeeper)
        {
            //await SessionGenerator.AddShopWaitersIfNotContains(senderSessionUnit.Session, senderSessionUnit.Owner, senderSessionUnit.DestinationId.Value);
            var session = await SessionRepository.GetAsync(senderSessionUnit.SessionId.Value);
            var owner = await ChatObjectRepository.GetAsync(senderSessionUnit.OwnerId);
            await SessionGenerator.AddShopWaitersIfNotContains(session, owner, senderSessionUnit.DestinationId.Value);
        }
        await Task.Yield();
    }

    /// <inheritdoc />
    public virtual async Task<Message> CreateMessageAsync(
        SessionUnitCacheItem senderSessionUnit,
        Func<Message, Task<IContentEntity>> action,
        string clientMessageId = null,
        Guid? receiverSessionUnitId = null,
        long? quoteMessageId = null,
        List<Guid> remindList = null)
    {

        Assert.If(!await SettingProvider.IsTrueAsync(ChatSettings.IsMessageSenderEnabled), $"MessageSender main switch is off.");

        Assert.NotNull(senderSessionUnit, $"Unable to send message, senderSessionUnit is null");

        Assert.If(!senderSessionUnit.IsInputEnabled, $"Unable to send message, input status is disabled,senderSessionUnitId:{senderSessionUnit.Id}");

        Assert.If(senderSessionUnit.MuteExpireTime > Clock.Now, $"Unable to send message,sessionUnit has been muted.senderSessionUnitId:{senderSessionUnit.Id}");

        var sw = Stopwatch.StartNew();

        // Create SessionUnit By Message
        await CreateSessionUnitByMessageAsync(senderSessionUnit);

        var sessionId = senderSessionUnit.SessionId.Value;

        //cache
        //await SessionUnitManager.GetOrAddCacheListAsync(sessionId);
        //var unitCacheList = await SessionUnitCacheManager.GetOrSetListBySessionAsync(
        //    sessionId,
        //    async (sessionId) =>
        //    {
        //        return await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId);
        //    });

        if (string.IsNullOrWhiteSpace(clientMessageId))
        {
            clientMessageId = UlidGenerator.Generate();
        }

        var message = new Message(senderSessionUnit)
        {
            CreationTime = Clock.Now,
            ClientMessageId = clientMessageId,
            //SessionKey = "",

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


        if (messageContent.Id == Guid.Empty)
        {
            messageContent.SetId(GuidGenerator.Create());
        }
        message.SetMessageContent(messageContent);

        var contentJson = JsonSerializer.Serialize(message.GetContentDto());

        message.SetContentJson(contentJson);

        //设置提醒
        await ApplyRemindIdListAsync(senderSessionUnit, message, remindList);

        //设置关注者
        await ApplyMessageFollowersAsync(senderSessionUnit, message);

        // Message Validator
        await MessageValidator.CheckAsync(message);

        ////sessionUnitCount
        //var sessionUnitCount = message.IsPrivateMessage() ? 2 : unitCacheList.Count();

        //message.SetSessionUnitCount(sessionUnitCount);

        // Save
        message = await Repository.InsertAsync(message, autoSave: true);

        // 重新获取一下，填充导航属性
        message = await Repository.GetAsync(message.Id);

        // 缓存消息
        await SetCacheAsync(message, senderSessionUnit);

        // update Session LastMessage
        await SessionUnitCacheManager.UpdateLastMessageAsync(senderSessionUnit, message);
        await SessionRepository.UpdateLastMessageIdAsync(sessionId, message.Id);

        // update SessionUnitSetting LastSendMessageId
        await SessionUnitSettingRepository.UpdateLastSendMessageAsync(senderSessionUnit.Id, message.Id, message.CreationTime);

        //更新引用次数
        await UpdateQuoteCountAsync(message.QuotePath);

        ////更新转发次数
        await UpdateForwardCountAsync(message.ForwardPath);

        // 发出事件 (移动到本地事伯 MessageCreated)
        //await PublishDistributedEventAsync(message, message.ForwardMessageId.HasValue ? Command.Forward : Command.Created);

        ////以下可能导致锁表
        //await SessionUnitManager.UpdateLastMessageIdAsync(senderSessionUnit.MessageId, message.MessageId);

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
        //    LastMessageId = message.MessageId,
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

        Logger.LogInformation("CreateMessageAsync {Elapsed}", sw.Elapsed);

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
    //            SenderSessionUnitId = senderSessionUnit.MessageId,
    //            MessageId = message.MessageId,
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
    protected virtual async Task<List<Guid>> ApplyReminderIdListForTextContentAsync(SessionUnitCacheItem senderSessionUnit, Message message)
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
    protected virtual async Task<List<Guid>> ApplyRemindIdListAsync(SessionUnitCacheItem senderSessionUnit, Message message, List<Guid> remindIdList)
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

        //await SessionUnitManager.IncrementRemindMeCountAsync(message.CreationTime, finalRemindIdList);

        return finalRemindIdList;
    }


    /// <summary>
    /// 设置关注
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    protected virtual async Task<List<Guid>> ApplyMessageFollowersAsync(SessionUnitCacheItem senderSessionUnit, Message message)
    {
        var followerIdList = await FollowManager.GetFollowerIdListAsync(senderSessionUnit.Id);
        message.SetFollowerIds(followerIdList);
        return followerIdList;
    }

    /// <inheritdoc />
    public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnitCacheItem senderSessionUnit,
        MessageInput<TContentInfo> input)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity
    {
        var messageContent = ObjectMapper.Map<TContentInfo, TContentEntity>(input.Content);
        return await SendAsync<TContentInfo, TContentEntity>(senderSessionUnit, input, messageContent);
    }

    /// <inheritdoc />
    public virtual async Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnitCacheItem senderSessionUnit,
        MessageInput input,
        TContentEntity contentEntity)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity
    {
        var message = await CreateMessageAsync(senderSessionUnit,
            async (entity) => await Task.FromResult(contentEntity),
            clientMessageId: input.ClientMessageId,
            quoteMessageId: input.QuoteMessageId,
            remindList: input.RemindList,
            receiverSessionUnitId: input.ReceiverSessionUnitId);

        //var output = ObjectMapper.Map<Message, MessageInfo<object>>(message);
        var output = ObjectMapper.Map<Message, MessageInfo<TContentInfo>>(message);
        //var output = new MessageInfo<TContentInfo>() { MessageId = message.MessageId };
        output.SenderSessionUnit ??= await SessionUnitManager.MapToSenderAsync(senderSessionUnit);
        //if (message.IsPrivateMessage())
        //{
        //    var receiverSessionUnit = await SessionUnitManager.GetCacheAsync(input.ReceiverSessionUnitId.Value);

        //    Assert.If(receiverSessionUnit.SessionId != senderSessionUnit.SessionId, $"Fail ReceiverSessionUnitId:{input.ReceiverSessionUnitId}");

        //    await ChatPusher.ExecutePrivateAsync(
        //    [
        //        senderSessionUnit, receiverSessionUnit
        //    ], output, input.IgnoreConnections);
        //}
        //else
        //{
        //    await ChatPusher.ExecuteBySessionIdAsync(message.SessionId.Value, output, input.IgnoreConnections);
        //}
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

        //(移动到本地事伯 MessageCreated)
        //await PublishDistributedEventAsync(message, Command.Rollback);

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
            var targetSessionUnit = await SessionUnitManager.GetCacheAsync(targetSessionUnitId);

            Assert.If(!targetSessionUnit.IsEnabled, $"Target session unit disabled,key:{targetSessionUnit.Id}");

            Assert.If(!targetSessionUnit.IsInputEnabled, $"Target session unit input state is disabled,key:{targetSessionUnit.Id}");

            Assert.If(currentSessionUnit.OwnerId != targetSessionUnit.OwnerId, $"[TargetSessionUnitId:{targetSessionUnitId}] is fail.");

            var newMessage = await CreateMessageAsync(targetSessionUnit, async (x) =>
            {
                x.SetForwardMessage(sourceMessage);
                await Task.Yield();
                return messageContent;
            });
            messageList.Add(newMessage);

            //await PublishDistributedEventAsync(newMessage, Command.Forward);

            var output = ObjectMapper.Map<Message, MessageInfo<object>>(newMessage);

            //var output = new MessageInfo<object>() { MessageId = newMessage.MessageId };

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

    public async Task<List<long>> GetRemindMessageIdListAsync(Guid sessionUnitId, List<long> messageIdList)
    {
        if (messageIdList == null || messageIdList.Count == 0)
        {
            return [];
        }
        return (await MessageReminderRepository.GetQueryableAsync())
            .Where(x => messageIdList.Contains(x.MessageId) && x.SessionUnitId == sessionUnitId)
            .Select(x => x.MessageId)
            .ToList();
    }

    public virtual async Task<MessageCacheItem> SetCacheAsync(
        Message message,
        SessionUnitCacheItem senderSessionUnit = null,
        DistributedCacheEntryOptions options = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        await SetMaxMessageIdAsync(message.SessionId.Value, message.Id);

        var messageInfo = ObjectMapper.Map<Message, MessageCacheItem>(message);

        //fix: 导航属性没有加载完全 改为手动转换Map
        if (message.SenderSessionUnit == null && message.SenderSessionUnitId.HasValue)
        {
            senderSessionUnit ??= await SessionUnitManager.GetCacheAsync(message.SenderSessionUnitId.Value);
            //messageInfo.SenderSessionUnit = ObjectMapper.Map<SessionUnit, SessionUnitSenderInfo>(senderSessionUnit);
            messageInfo.SenderSessionUnit = await SessionUnitManager.MapToSenderAsync(senderSessionUnit);
        }

        messageInfo.Content ??= message.GetContentDto();

        await MessageCache.SetAsync(new MessageCacheKey(messageInfo.Id), messageInfo, options ?? CacheOptions, hideErrors, considerUow, token);

        return messageInfo;
    }

    public Task<MessageCacheItem> GetCacheAsync(
        long messageId,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        return MessageCache.GetAsync(new MessageCacheKey(messageId), hideErrors, considerUow, token);
    }

    public Task<KeyValuePair<MessageCacheKey, MessageCacheItem>[]> GetManyCacheAsync(
        IEnumerable<long> messageIds,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        return MessageCache.GetManyAsync(messageIds.Select(x => new MessageCacheKey(x)), hideErrors, considerUow, token);
    }

    public async Task<KeyValuePair<MessageCacheKey, MessageCacheItem>[]> GetOrAddManyCacheAsync(
        IEnumerable<long> messageIds,
        Func<DistributedCacheEntryOptions> optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        var cacheKeys = messageIds.Select(x => new MessageCacheKey(x));

        var list = await MessageCache.GetOrAddManyAsync(cacheKeys, async (keys) =>
        {
            var messageIds = keys.Select(x => x.MessageId);

            var entities = await Repository.GetListAsync(x => messageIds.Contains(x.Id));

            var dict = entities.ToDictionary(
                x => new MessageCacheKey(x.Id),
                x => ObjectMapper.Map<Message, MessageCacheItem>(x));

            return [.. keys.Select(x =>
                new KeyValuePair<MessageCacheKey, MessageCacheItem>(x, dict.TryGetValue(x, out var cacheItem) ? cacheItem : null)
            )];

        }, optionsFactory ?? (() => CacheOptions), hideErrors, considerUow, token);

        return list;

    }

    public async Task<long> GetMaxMessageIdAsync(Guid sessionId)
    {
        var cache = await SessionMaxMessageIdCache.GetOrAddAsync(new SessionMaxMessageIdCacheKey(sessionId), async () =>
        {
            var queryable = await Repository.GetQueryableAsync();

            var list = await queryable
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .Take(1)
                .ToListAsync();

            var maxMessageId = list.FirstOrDefault();

            return new SessionMaxMessageIdCacheItem()
            {
                MaxMessageId = maxMessageId
            };
        }, () => CacheOptions);

        return cache.MaxMessageId;
    }

    public async Task SetMaxMessageIdAsync(Guid sessionId, long messageId)
    {
        await SessionMaxMessageIdCache.SetAsync(new SessionMaxMessageIdCacheKey(sessionId), new SessionMaxMessageIdCacheItem()
        {
            MaxMessageId = messageId
        });
    }
}
