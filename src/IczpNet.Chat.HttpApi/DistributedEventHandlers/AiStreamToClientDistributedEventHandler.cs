using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.DistributedEventHandlers;

/// <summary>
/// Delivers an Aurora stream lifecycle event through the same distributed
/// event and ChatHub route used by normal IM notifications. This deliberately
/// does not use IChatPusher: that is a separate Pusher transport.
/// </summary>
public class AiStreamToClientDistributedEventHandler(
    ISessionUnitManager sessionUnitManager)
    : SendToClientDistributedEventHandler<AiStreamToClientDistributedEto>, ITransientDependency
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;

    public override async Task HandleEventAsync(AiStreamToClientDistributedEto eventData)
    {
        await MeasureAsync(nameof(SendToRequesterAsync), () => SendToRequesterAsync(eventData));
    }

    protected virtual async Task<bool> SendToRequesterAsync(AiStreamToClientDistributedEto eventData)
    {
        var requester = await SessionUnitManager.GetCacheAsync(eventData.RequesterSessionUnitId);
        if (requester == null || requester.SessionId != eventData.SessionId)
        {
            Logger.LogWarning(
                "Ignoring AI stream event {Command} for source message {SourceMessageId}: requester session unit {RequesterSessionUnitId} is invalid for session {SessionId}",
                eventData.Command,
                eventData.SourceMessageId,
                eventData.RequesterSessionUnitId,
                eventData.SessionId);
            return false;
        }

        var connectionIds = (await OnlineManager.GetConnectionIdsByOwnerAsync(requester.OwnerId)).ToList();
        if (connectionIds.Count == 0)
        {
            Logger.LogInformation(
                "AI stream event {Command} for run {RunId}, source message {SourceMessageId} has no online requester connection; Redis recovery remains available.",
                eventData.Command,
                eventData.RunId,
                eventData.SourceMessageId);
            return true;
        }

        await HubContext.Clients.Clients(connectionIds).ReceivedMessage(new CommandPayload
        {
            Scopes =
            [
                new CommandPayload.ScopeUnit
                {
                    ChatObjectId = requester.OwnerId,
                    SessionUnitId = requester.Id
                }
            ],
            Command = eventData.Command,
            Payload = eventData
        });
        Logger.LogDebug(
            "Delivered AI stream event {Command} for run {RunId}, source message {SourceMessageId} to {ConnectionCount} requester connections",
            eventData.Command,
            eventData.RunId,
            eventData.SourceMessageId,
            connectionIds.Count);

        return true;
    }
}
