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

namespace IczpNet.Chat.Ai;

public class AiTrigger(
    IAiResolver aiResolver,
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IJsonSerializer jsonSerializer,
    IBackgroundJobManager backgroundJobManager,
    IChatObjectManager chatObjectManager) : 
    DomainService, 
    //ILocalEventHandler<EntityCreatedEventData<Message>>,
    ITransientDependency
{
    protected IAiResolver AiResolver { get; } = aiResolver;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();

        var message = eventData.Entity;

        if (message.SenderId == message.ReceiverId)
        {
            Logger.LogInformation($"message.SenderId == message.ReceiverId:{message.ReceiverId}");
            return;
        }

        var receiver = await ChatObjectManager.GetAsync(message.ReceiverId.Value);

        if (!AiResolver.HasProvider(receiver.Code))
        {
            Logger.LogInformation($"Not Ai provider:{receiver.Code}");
            return;
        }

        if (BackgroundJobManager.IsAvailable())
        {
            var aiJobArg = new AiJobArg()
            {
                Provider = receiver.Code,
                EventType = EventTypes.Created,
                MessageId = message.Id,
            };
            var jobId = await BackgroundJobManager.EnqueueAsync(aiJobArg);

            Logger.LogInformation($"Message is jobId:{jobId},{nameof(AiJobArg)}:{aiJobArg}");
        }
        else
        {
            Logger.LogWarning($"BackgroundJob IsAvailable: False.");
        }
    }
}
