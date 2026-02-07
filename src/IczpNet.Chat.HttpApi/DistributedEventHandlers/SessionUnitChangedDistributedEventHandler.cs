using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.SessionUnits;
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

        foreach (var connectionId in connIdList)
        {
            var scope = new CommandPayload.ScopeUnit
            {
                ChatObjectId = unit.OwnerId,
                //SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                SessionUnitId = unit.Id
            };

            var commandPayload = new CommandPayload()
            {
                //AppUserId = item.UserId,
                Scopes = [scope],
                Command = command,
                Payload = eventData,
            };

            await HubContext.Clients.Client(connectionId).ReceivedMessage(commandPayload);
        }

        return true;
    }

}
