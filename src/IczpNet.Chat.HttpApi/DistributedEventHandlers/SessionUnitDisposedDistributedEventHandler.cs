using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.SessionUnits;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.DistributedEventHandlers;

/// <summary>
/// 会话处置（移出/删除/隐藏/清空）分布式事件处理器，向客户端推送 SignalR 事件
/// </summary>
public class SessionUnitDisposedDistributedEventHandler : SendToClientDistributedEventHandler<SessionUnitDisposedDistributedEto>, ITransientDependency
{
    public override async Task HandleEventAsync(SessionUnitDisposedDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToClientAsync), () => SendToClientAsync(eventData));
    }

    protected async Task<bool> SendToClientAsync(SessionUnitDisposedDistributedEto eventData)
    {
        var command = eventData.Command;
        var connIdList = (await OnlineManager.GetConnectionIdsByOwnerAsync(eventData.OwnerId)).ToList();

        var commandPayload = new CommandPayload
        {
            Scopes = [new CommandPayload.ScopeUnit
            {
                ChatObjectId = eventData.OwnerId,
                SessionUnitId = eventData.SessionUnitId
            }],
            Command = command,
            Payload = eventData,
        };

        await HubContext.Clients.Clients(connIdList).ReceivedMessage(commandPayload);
        return true;
    }
}
