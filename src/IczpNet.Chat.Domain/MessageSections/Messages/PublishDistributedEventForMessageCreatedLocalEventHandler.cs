using IczpNet.Chat.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.MessageSections.Messages;

/// <summary>
/// 发布分布式事件(发送消息)
/// </summary>
public class PublishDistributedEventForMessageCreatedLocalEventHandler(
    ICurrentHosted currentHosted,
    IDistributedEventBus distributedEventBus)
    :
    DomainService,
    ILocalEventHandler<EntityCreatedEventData<Message>>,
    ITransientDependency
{
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;

    protected string HandlerName => $"[{nameof(PublishDistributedEventForMessageCreatedLocalEventHandler)}]";
    //[UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        var message = eventData.Entity;
        Logger.LogInformation($"{HandlerName} Created message:{message}");
        await PublishDistributedEventAsync(message);
    }

    protected virtual async Task PublishDistributedEventAsync(Message message, bool onUnitOfWorkComplete = true, bool useOutbox = true)
    {
        var eventData = new MessageSentEto()
        {
            Id = message.Id,
            HostName = CurrentHosted.Name,
            PublishTime = Clock.Now,
        };

        await DistributedEventBus.PublishAsync(eventData, onUnitOfWorkComplete, useOutbox);

        Logger.LogInformation($"{HandlerName} {eventData}");
    }
}
