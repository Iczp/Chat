using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;

namespace IczpNet.Chat.DistributedEventHandlers;

public class SendMessageToClientDistributedEventHandler : SendToClientDistributedEventHandler<SendMessageToClientDistributedEto>, ITransientDependency
{
    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();
    public IConnectionPoolManager ConnectionPoolManager => LazyServiceProvider.LazyGetRequiredService<IConnectionPoolManager>();

    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();

    public override async Task HandleEventAsync(SendMessageToClientDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToClientBySessionAsync), () => SendToClientBySessionAsync(eventData));
    }

    protected async Task<bool> SendToClientBySessionAsync(SendMessageToClientDistributedEto eventData)
    {
        var sessionId = eventData.Message.SessionId;
        var command = eventData.Command;
        var connDict = await OnlineManager.GetConnectionsBySessionAsync(sessionId);

        var onlineOwnerIds = connDict.SelectMany(x => x.Value).Distinct().ToList();

        var members = await SessionUnitCacheManager.GetMembersAsync(sessionId);
        var ownerUnitDict = members.Where(x => onlineOwnerIds.Contains(x.OwnerId)).ToDictionary(x => x.OwnerId, x => x.Id);

        //await HubContext.Clients.Group(sessionId.ToString()).ReceivedMessage(commandPayload);

        foreach (var item in connDict)
        {
            var connectionId = item.Key;
            var chatObjectIdList = item.Value;

            var units = chatObjectIdList
                .Select(chatObjectId => new CommandPayload.ScopeUnit
                {
                    ChatObjectId = chatObjectId,
                    //SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                    SessionUnitId = ownerUnitDict[chatObjectId]
                }).ToList();

            var commandPayload = new CommandPayload()
            {
                //AppUserId = item.UserId,
                Scopes = units,
                Command = command,
                Payload = eventData,
            };

            await HubContext.Clients.Client(connectionId).ReceivedMessage(commandPayload);
        }

        return true;
    }
}
