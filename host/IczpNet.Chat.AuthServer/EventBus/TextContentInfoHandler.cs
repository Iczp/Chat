using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.EventBus;

public class TextContentInfoHandler : DomainService, IDistributedEventHandler<TextContentInfo>, ITransientDependency
{
    public async Task HandleEventAsync(TextContentInfo eventData)
    {
        await Task.Yield();
        Logger.LogInformation($"收到分布事件:${eventData.Text}");

    }
}
