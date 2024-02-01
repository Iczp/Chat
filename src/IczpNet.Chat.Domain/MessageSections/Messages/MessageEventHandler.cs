using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace IczpNet.Chat.MessageSections.Messages;

internal class MessageEventHandler : ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
{
    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();
    }
}
