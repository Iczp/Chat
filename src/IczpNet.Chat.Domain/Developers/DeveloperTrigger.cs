using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Json;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Developers;

public class DeveloperTrigger(
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IJsonSerializer jsonSerializer,
    IBackgroundJobManager backgroundJobManager,
    IChatObjectManager chatObjectManager,
    IDeveloperManager developerManager) 
    : 
    DomainService, 
    //ILocalEventHandler<EntityCreatedEventData<Message>>,
    ITransientDependency
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    protected IDeveloperManager DeveloperManager { get; } = developerManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();

        var message = eventData.Entity;

        var senderSessionUnit = await SessionUnitManager.GetAsync(message.SenderSessionUnitId.Value);

        if (message.SenderId == message.ReceiverId)
        {
            Logger.LogInformation($"message.SenderId == message.ReceiverId:{message.ReceiverId}");
            return;
        }

        if (!await DeveloperManager.IsEnabledAsync(message.ReceiverId))
        {
            Logger.LogInformation($"Message.ReceiverId:{message.ReceiverId},Developer is disabled.");
            return;
        }

        //var receiver = await ChatObjectManager.GetAsync(message.ReceiverId.Value);
        if (BackgroundJobManager.IsAvailable())
        {
            var developerJobArg = new DeveloperJobArg()
            {
                EventType = EventTypes.Created,
                MessageId = message.Id,
            };
            var jobId = await BackgroundJobManager.EnqueueAsync(developerJobArg);

            Logger.LogInformation($"Message is jobId:{jobId},{nameof(DeveloperJobArg)}:{developerJobArg}");
        }
        else
        {
            Logger.LogWarning($"BackgroundJob IsAvailable: False.");
        }
    }
}
