using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// SessionUnitMessage 处理
/// </summary>
public class BatchIncrementForMessageCreatedEventHandler(
    ISessionUnitManager sessionUnitManager,
    ISessionUnitRedisStore redisStore) :
    DomainService,
    // 先暂时不启用 SessionUnitMessage
    ILocalEventHandler<EntityCreatedEventData<Message>>,
    ITransientDependency
{
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitRedisStore RedisStore { get; } = redisStore;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        var message = eventData.Entity;

        var sessionId = message.SessionId;

        Logger.LogInformation($"{nameof(BatchIncrementForMessageCreatedEventHandler)} Created message:{message}");

        var stopwatch = Stopwatch.StartNew();

        await RedisStore.SetListBySessionIfNotExistsAsync(sessionId.Value, async (sessionId) => await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId));

        await RedisStore.BatchIncrementBadgeAndSetLastMessageAsync(message);

        Logger.LogInformation($"{nameof(BatchIncrementForMessageCreatedEventHandler)}  watch={stopwatch.ElapsedMilliseconds}ms");

    }
}
