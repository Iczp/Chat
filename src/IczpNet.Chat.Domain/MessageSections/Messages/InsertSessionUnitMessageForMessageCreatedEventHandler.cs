using IczpNet.Chat.SessionUnitMessages;
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
public class InsertSessionUnitMessageForMessageCreatedEventHandler(ISessionUnitMessageRepository sessionUnitMessageRepository) : 
    DomainService,
    // 先暂时不启用 SessionUnitMessage
    //ILocalEventHandler<EntityCreatedEventData<Message>>, 
    ITransientDependency
{

    public ISessionUnitMessageRepository SessionUnitMessageRepository { get; } = sessionUnitMessageRepository;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        var message = eventData.Entity;

        Logger.LogInformation($"{nameof(InsertSessionUnitMessageForMessageCreatedEventHandler)} Created message:{message}");

        var stopwatch = Stopwatch.StartNew();

        var result = await SessionUnitMessageRepository.InsertMessagesForAllAsync(message);

        Logger.LogInformation($"{nameof(InsertSessionUnitMessageForMessageCreatedEventHandler)} result={result}, watch={stopwatch.ElapsedMilliseconds}ms");
    }
}
