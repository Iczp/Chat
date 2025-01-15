using IczpNet.Chat.Developers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;

namespace IczpNet.Chat.MessageSections.Messages;

internal class MessageEventHandler(IDeveloperManager developerManager) : DomainService, ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
{
    protected IDeveloperManager DeveloperManager { get; } = developerManager;


    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();
        var entity = eventData.Entity;
        Logger.LogInformation($"Created message:{entity}");

        var receiverType = entity.ReceiverType;

        if(await DeveloperManager.IsEnabledAsync(entity.ReceiverId))
        {
            Logger.LogInformation($"Developer IsEnabled");
        }
    }
}
