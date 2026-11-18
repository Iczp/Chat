using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;


namespace IczpNet.Chat.DistributedEventHandlers;

public class SessionUnitChangedDistributedEventHandler : SendToClientDistributedEventHandler<SessionUnitChangedDistributedEto>, ITransientDependency
{

    public override async Task HandleEventAsync(SessionUnitChangedDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToClientAsync), () => SendToClientAsync(eventData));
    }

    protected async Task<bool> SendToClientAsync(SessionUnitChangedDistributedEto eventData)
    {
        var command = eventData.Command;

        var unit = eventData.SessionUnit;

        var connIdList = await OnlineManager.GetConnectionIdsByOwnerAsync(unit.OwnerId);

        var commandPayload = new CommandPayload()
        {
            //AppUserId = item.UserId,
            Scopes = [new CommandPayload.ScopeUnit{
                ChatObjectId = unit.OwnerId,
                SessionUnitId = unit.Id
            }],
            Command = command,
            Payload = eventData,
        };

        await HubContext.Clients.Clients(connIdList).ReceivedMessage(commandPayload);

        return true;
    }

}
