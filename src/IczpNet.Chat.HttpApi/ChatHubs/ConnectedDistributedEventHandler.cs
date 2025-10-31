using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
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
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        // 发送到其他客户端
        await SendToUserAsync(eventData.UserId.Value, new CommandPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = CommandConsts.MeOnline,
            Payload = eventData,
        });

        // 发送到我的朋友 
        await SendToFriendsAsync(eventData.UserId.Value, CommandConsts.FriendOnline, eventData);

        await uow.CompleteAsync();
    }
}
