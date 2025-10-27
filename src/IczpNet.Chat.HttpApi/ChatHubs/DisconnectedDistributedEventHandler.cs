using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatHubs;

public class DisconnectedDistributedEventHandler : ChatHubService, IDistributedEventHandler<DisconnectedEto>//, ILocalEventHandler<OnDisconnectedEto>
{
    [UnitOfWork]
    public async Task HandleEventAsync(DisconnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(DisconnectedDistributedEventHandler)} received eventData[{nameof(DisconnectedEto)}]:{eventData}");

        //var commandPayload = new CommandPayload()
        //{
        //    AppUserId = eventData.UserId,
        //    Scopes = [],
        //    Command = "disconnected",
        //    Payload = eventData,
        //};

        //Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}],commandPayload={JsonSerializer.Serialize(commandPayload)}");

        // 发送到用户其他客户端
        await SendToUserAsync(eventData.UserId.Value, new CommandPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = CommandConsts.MeOffline,
            Payload = eventData,
        });

        // 发送到用户的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, new CommandPayload()
        {
            Command = CommandConsts.FriendOffline,
            Payload = eventData,
        });
    }
}
