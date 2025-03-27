using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Pusher.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Json;

namespace IczpNet.Chat.ChatHubs;

public class OnDisconnectedLocalEventHandler(
    IConnectionPoolManager connectionPoolManager,
    ICurrentHosted currentHosted,
    IJsonSerializer jsonSerializer) : DomainService, ILocalEventHandler<OnDisconnectedEto>
{
    public IHubContext<ChatHub, IChatClient> HubContext => LazyServiceProvider.LazyGetRequiredService<IHubContext<ChatHub, IChatClient>>();
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    public async Task HandleEventAsync(OnDisconnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(OnDisconnectedLocalEventHandler)} received eventData[{nameof(OnDisconnectedEto)}]:{eventData}");

       

        var commandPayload = new PushPayload()
        {
            AppUserId = eventData.AppUserId,
            Scopes = [],
            Command = "OnConnected",
            Payload = eventData,
        };

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}],commandPayload={JsonSerializer.Serialize(commandPayload)}");

        await HubContext.Clients.All.ReceivedMessage(commandPayload);
    }
}
