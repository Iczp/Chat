using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.DistributedEventHandlers;

public class MessageCreatedDistributedEventHandler : DomainService, IDistributedEventHandler<MessageCreatedEto>
{
    public async Task HandleEventAsync(MessageCreatedEto eventData)
    {
        Logger.LogInformation($"[{nameof(MessageCreatedDistributedEventHandler)}] MessageId:{eventData.MessageId}");

        await Task.CompletedTask;

    }
}
