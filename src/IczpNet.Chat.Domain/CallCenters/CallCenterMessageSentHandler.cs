using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.CallCenters;

/// <summary>Processes committed messages independently of the existing send path.</summary>
public class CallCenterMessageSentHandler(
    IMessageRepository messages,
    ICallCenterManager callCenter) : IDistributedEventHandler<MessageSentEto>, ITransientDependency
{
    public async Task HandleEventAsync(MessageSentEto eventData)
    {
        var message = await messages.FindAsync(eventData.Id);
        if (message != null) await callCenter.ProcessMessageAsync(message);
    }
}
