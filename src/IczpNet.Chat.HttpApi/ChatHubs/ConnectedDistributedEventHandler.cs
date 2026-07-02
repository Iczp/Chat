using DeviceDetectorNET.Parser.Device;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatHubs;

public class ConnectedDistributedEventHandler(IUnitOfWorkManager unitOfWorkManager) : ChatHubService, IDistributedEventHandler<ConnectedEto>//, ILocalEventHandler<OnConnectedEto>
{
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    //[UnitOfWork]
    public async Task HandleEventAsync(ConnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(ConnectedDistributedEventHandler)} received eventData[{nameof(ConnectedEto)}]:{eventData}");

        // 分布式事件要开启工作单元
        using var uow = UnitOfWorkManager.Begin();

        if (eventData.UserId == null)
        {
            Logger.LogWarning($"{nameof(ConnectedDistributedEventHandler)} received eventData[{nameof(ConnectedEto)}] UserId is null");
            return;
        }

        //// 当前用户的所有连接
        //var userConnList = (await OnlineManager.GetConnectionsByUserAsync(eventData.UserId.Value)).ToList();

        //var deviceTypes = userConnList.Select(x => x.DeviceType).Distinct().ToList();

        // 发送到其他客户端
        await SendToUserAsync(eventData.UserId.Value, new CommandPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = CommandConsts.MeOnline,
            Payload = eventData,
            //Payload = new MeOnlinePayload()
            //{
            //    Current = eventData.ConnectionId,
            //    Connections = userConnList,
            //},
        });

        // 发送到我的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, CommandConsts.FriendOnline, eventData);

        await uow.CompleteAsync();
    }
}
