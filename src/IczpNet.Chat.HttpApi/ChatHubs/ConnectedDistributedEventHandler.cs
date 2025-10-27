using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatHubs;

public class ConnectedDistributedEventHandler : ChatHubService, IDistributedEventHandler<ConnectedEto>//, ILocalEventHandler<OnConnectedEto>
{
    [UnitOfWork]
    public async Task HandleEventAsync(ConnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(ConnectedDistributedEventHandler)} received eventData[{nameof(ConnectedEto)}]:{eventData}");

        //var totalCount = await ConnectionPoolManager.GetTotalCountAsync();

        //var commandPayload = new CommandPayload()
        //{
        //    AppUserId = eventData.UserId,
        //    Scopes = [],
        //    Command = "connected",
        //    Payload = eventData,
        //};

        //Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}]:{totalCount},commandPayload={JsonSerializer.Serialize(commandPayload)}");

        // 发送到其他客户端
        await SendToUserAsync(eventData.UserId.Value, new CommandPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = CommandConsts.MeOnline,
            Payload = eventData,
        });

        // 发送到我的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, new CommandPayload()
        {
            //AppUserId = eventData.UserId,
            Scopes = [],
            Command = CommandConsts.FriendOnline,
            Payload = eventData,
        });
    }
}
