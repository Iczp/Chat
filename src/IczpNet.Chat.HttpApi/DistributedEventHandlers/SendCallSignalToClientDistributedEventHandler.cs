using IczpNet.Chat.Calls;
using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.DistributedEventHandlers;

public class SendCallSignalToClientDistributedEventHandler(
    IHubContext<ChatHub, IChatClient> hubContext,
    ISessionUnitCacheManager sessionUnitCacheManager,
    IOnlineManager onlineManager)
    : IDistributedEventHandler<SendCallSignalToClientDistributedEto>, ITransientDependency
{
    public async Task HandleEventAsync(SendCallSignalToClientDistributedEto eventData)
    {
        var members = await sessionUnitCacheManager.GetMembersAsync(eventData.SessionId);
        if (eventData.TargetSessionUnitIds is { Count: > 0 })
            members = members.Where(m => eventData.TargetSessionUnitIds.Contains(m.Id)).ToList();
        if (eventData.TargetOwnerIds is { Count: > 0 })
            members = members.Where(m => eventData.TargetOwnerIds.Contains(m.OwnerId)).ToList();

        var ownerIds = members.Select(x => x.OwnerId).Distinct().ToList();
        if (ownerIds.Count == 0) return;

        var ownerDevices = await onlineManager.GetDevicesAsync(ownerIds);
        var ownerUnitMap = members.DistinctBy(x => x.OwnerId).ToDictionary(x => x.OwnerId, x => x.Id);

        var excludeSet = eventData.ExcludeDeviceIds?.ToHashSet() ?? [];
        var connMap = ownerDevices
            .Where(x => ownerUnitMap.ContainsKey(x.Key))
            .SelectMany(x => x.Value)
            .Where(d => string.IsNullOrWhiteSpace(d.DeviceId) || !excludeSet.Contains(d.DeviceId))
            .GroupBy(x => x.ConnectionId)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var item in connMap)
        {
            var units = item.Value.Select(d => new CommandPayload.ScopeUnit
            {
                ChatObjectId = d.OwnerId,
                SessionUnitId = ownerUnitMap.GetOrDefault(d.OwnerId)
            }).ToList();

            await hubContext.Clients.Client(item.Key).ReceivedMessage(new CommandPayload
            {
                Scopes = units,
                Command = eventData.Command,
                Payload = eventData.Payload
            });
        }
    }
}
