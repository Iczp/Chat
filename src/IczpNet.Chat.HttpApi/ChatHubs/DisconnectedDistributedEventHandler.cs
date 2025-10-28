using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatHubs;

public class DisconnectedDistributedEventHandler(IUnitOfWorkManager unitOfWorkManager) : ChatHubService, IDistributedEventHandler<DisconnectedEto>//, ILocalEventHandler<OnDisconnectedEto>
{
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    //[UnitOfWork]
    public async Task HandleEventAsync(DisconnectedEto eventData)
    {
        Logger.LogInformation($"{nameof(DisconnectedDistributedEventHandler)} received eventData[{nameof(DisconnectedEto)}]:{eventData}");

        if (!eventData.UserId.HasValue)
        {
            Logger.LogWarning($"{nameof(DisconnectedDistributedEventHandler)}],userId is null, eventData={JsonSerializer.Serialize(eventData)}");
            return;
        }

        // 分布式事件要开启工作单元
        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        // 发送到用户其他客户端
        await SendToUserAsync(eventData.UserId.Value, new CommandPayload()
        {
            AppUserId = eventData.UserId,
            Scopes = [],
            Command = CommandConsts.MeOffline,
            Payload = eventData,
        });

        // 发送到用户的朋友 
        await SendToFriendsAsync(eventData.UserId.Value , CommandConsts.FriendOffline, eventData);

        // 提交工作单元
        await uow.CompleteAsync();
    }
}
