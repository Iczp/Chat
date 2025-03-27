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

public class OnConnectedLocalEventHandler(
    IConnectionPoolManager connectionPoolManager,
    ICurrentHosted currentHosted,
    IJsonSerializer jsonSerializer) : DomainService, ILocalEventHandler<OnConnectedEto>
{
    public IHubContext<ChatHub, IChatClient> HubContext => LazyServiceProvider.LazyGetRequiredService<IHubContext<ChatHub, IChatClient>>();
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;

    public async Task HandleEventAsync(OnConnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(OnConnectedLocalEventHandler)} received eventData[{nameof(OnConnectedEto)}]:{eventData}");

        var connectionPools = (await ConnectionPoolManager.GetAllListAsync())
            .Where(x => x.Host == CurrentHosted.Name)
            .Where(x => x.ChatObjectIdList.Any(d => eventData.ChatObjectIdList.Contains(d)))
            .ToList();

        var commandPayload = new PushPayload()
        {
            AppUserId = eventData.AppUserId,
            Scopes = [],
            Command = "OnConnected",
            Payload = eventData,
        };

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}]:{connectionPools.Count},commandPayload={JsonSerializer.Serialize(commandPayload)}");

        await HubContext.Clients.All.ReceivedMessage(commandPayload);
    }
}
