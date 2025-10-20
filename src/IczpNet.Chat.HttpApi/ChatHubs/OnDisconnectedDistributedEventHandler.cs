using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.ChatHubs;

public class OnDisconnectedDistributedEventHandler : ChatHubService, IDistributedEventHandler<OnDisconnectedEto>//, ILocalEventHandler<OnDisconnectedEto>
{
    public async Task HandleEventAsync(OnDisconnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(OnDisconnectedDistributedEventHandler)} received eventData[{nameof(OnDisconnectedEto)}]:{eventData}");

        var commandPayload = new CommandPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = "OnDisconnected",
            Payload = eventData,
        };

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}],commandPayload={JsonSerializer.Serialize(commandPayload)}");

        //发送到其他客户端
        await SendToUserAsync(eventData.UserId.Value, commandPayload);

        // 发送到我的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, commandPayload);
    }
}
