using IczpNet.Chat.ConnectionPools;
using IczpNet.Pusher.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.EventBus;

namespace IczpNet.Chat.ChatHubs;

public class OnDisconnectedLocalEventHandler : ChatHubService, ILocalEventHandler<OnDisconnectedEto>
{
    public async Task HandleEventAsync(OnDisconnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(OnDisconnectedLocalEventHandler)} received eventData[{nameof(OnDisconnectedEto)}]:{eventData}");

        var commandPayload = new PushPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = "OnDisconnected",
            Payload = eventData,
        };

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}],commandPayload={JsonSerializer.Serialize(commandPayload)}");

        // 发送到我的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, commandPayload);
    }
}
