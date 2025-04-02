using IczpNet.Chat.ConnectionPools;
using IczpNet.Pusher.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.EventBus;

namespace IczpNet.Chat.ChatHubs;

public class OnConnectedLocalEventHandler : ChatHubService, ILocalEventHandler<OnConnectedEto>
{
    public async Task HandleEventAsync(OnConnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(OnConnectedLocalEventHandler)} received eventData[{nameof(OnConnectedEto)}]:{eventData}");

        var totalCount = await ConnectionPoolManager.GetTotalCountAsync();

        var commandPayload = new PushPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = "OnConnected",
            Payload = eventData,
        };

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}]:{totalCount},commandPayload={JsonSerializer.Serialize(commandPayload)}");

        // 发送到我的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, commandPayload);
    }
}
