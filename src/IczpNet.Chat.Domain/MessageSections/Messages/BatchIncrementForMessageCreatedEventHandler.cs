

using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// SessionUnitCache 批量增量事件处理
/// </summary>
public class BatchIncrementForMessageCreatedEventHandler(
    IObjectMapper objectMapper,
    ISessionUnitManager sessionUnitManager,
    ISessionUnitCacheManager sessionUnitCacheManager)
    :
    DomainService,
    //ILocalEventHandler<EntityCreatedEventData<Message>>,
    ITransientDependency
{
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        var message = eventData.Entity;

        var sessionId = message.SessionId;

        Logger.LogInformation($"{nameof(BatchIncrementForMessageCreatedEventHandler)} Created message:{message}");

        var stopwatch = Stopwatch.StartNew();

        await SessionUnitManager.LoadMembersIfNotExistsAsync(sessionId.Value);

        var messageCacheItem = ObjectMapper.Map<Message, MessageCacheItem>(message);

        await SessionUnitCacheManager.BatchIncrementAsync(
            messageCacheItem,
            reminderIds: message.MessageReminderList.Select(x => x.SessionUnitId).ToList(),
            followerIds: message.MessageFollowerList.Select(x => x.SessionUnitId).ToList());

        Logger.LogInformation($"{nameof(BatchIncrementForMessageCreatedEventHandler)}  watch={stopwatch.ElapsedMilliseconds}ms");

    }
}
