using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using IczpNet.Pusher.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;

namespace IczpNet.Chat.DistributedEventHandlers;

public class MessageChangedDistributedEventHandler : DomainService, IDistributedEventHandler<MessageChangedDistributedEto>, ITransientDependency
{
    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public IConnectionPoolManager ConnectionPoolManager => LazyServiceProvider.LazyGetRequiredService<IConnectionPoolManager>();
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();
    public IHubContext<ChatHub, IChatClient> HubContext => LazyServiceProvider.LazyGetRequiredService<IHubContext<ChatHub, IChatClient>>();

    public async Task HandleEventAsync(MessageChangedDistributedEto eventData)
    {
        Logger.LogInformation($"{nameof(MessageChangedDistributedEventHandler)} received eventData[{nameof(MessageChangedDistributedEto)}]:{eventData}");

        var cacheKey = eventData.CacheKey;
        var payload = eventData.CacheKey;
        var command = eventData.Command;
        var ignoreConnections = new HashSet<string>();

        var sessionUnitInfoList = await SessionUnitManager.GetCacheListAsync(cacheKey);

        if (sessionUnitInfoList == null)
        {
            Logger.LogWarning($"sessionUnitInfoList is null, eventData:{eventData}");
            return;
        }

        Logger.LogInformation($"sessionUnitInfoList.count:{sessionUnitInfoList.Count}");

        var chatObjectIdList = sessionUnitInfoList
            .Where(x => x != null)
            //.Where(x => x.ServiceStatus == ServiceStatus.Online)
            .Select(x => x.OwnerId)
            .ToList();

        var connectionPools = (await ConnectionPoolManager.GetAllListAsync())
            .Where(x => x.Host == CurrentHosted.Name)
            .Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .ToList();

        Logger.LogInformation($"Online count:{connectionPools.Count}");

        //
        foreach (var item in connectionPools)
        {
            if (ignoreConnections != null && ignoreConnections.Any(x => x == item.ConnectionId))
            {
                continue;
            }

            var units = item.ChatObjectIdList
                .Where(chatObjectIdList.Contains)
                .Select(chatObjectId => new ScopeUnit
                {
                    ChatObjectId = chatObjectId,
                    SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                }).ToList();

            var commandPayload = new PushPayload()
            {
                AppUserId = item.AppUserId,
                Scopes = units,//sessionUnitCaches.Select(x=>x as object).ToList(),
                //Caches = sessionUnitCaches,
                Command = command,
                Payload = payload,
            };

            Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}]:{connectionPools.Count},commandPayload={JsonSerializer.Serialize(commandPayload)}");

            await HubContext.Clients.Client(item.ConnectionId).ReceivedMessage(commandPayload);
        }
    }
}
