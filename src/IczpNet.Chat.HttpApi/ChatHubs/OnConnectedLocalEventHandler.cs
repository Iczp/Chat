using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
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

        var commandPayload = new CommandPayload()
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
