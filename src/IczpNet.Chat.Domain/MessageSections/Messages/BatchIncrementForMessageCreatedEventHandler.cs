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
    ISessionUnitCacheManager sessionUnitCacheManager) 
    : 
    DomainService, 
    ILocalEventHandler<EntityCreatedEventData<Message>>, 
    ITransientDependency
{
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        var message = eventData.Entity;

        var sessionId = message.SessionId;

        Logger.LogInformation($"{nameof(BatchIncrementForMessageCreatedEventHandler)} Created message:{message}");

        var stopwatch = Stopwatch.StartNew();

        await SessionUnitCacheManager.SetListBySessionIfNotExistsAsync(
            sessionId.Value,
            async (sessionId) => await SessionUnitManager.GetCacheListBySessionIdAsync(sessionId));

        await SessionUnitCacheManager.BatchIncrementAsync(message);

        Logger.LogInformation($"{nameof(BatchIncrementForMessageCreatedEventHandler)}  watch={stopwatch.ElapsedMilliseconds}ms");

    }
}
