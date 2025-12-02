using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;

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

public class SendToClientDistributedEventHandler : DomainService, IDistributedEventHandler<SendToClientDistributedEto>, ITransientDependency
{
    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public ISessionUnitCacheManager SessionUnitCacheManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitCacheManager>();
    public IConnectionPoolManager ConnectionPoolManager => LazyServiceProvider.LazyGetRequiredService<IConnectionPoolManager>();
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();
    public IHubContext<ChatHub, IChatClient> HubContext => LazyServiceProvider.LazyGetRequiredService<IHubContext<ChatHub, IChatClient>>();

    public async Task HandleEventAsync(SendToClientDistributedEto eventData)
    {
        Logger.LogInformation($"{nameof(SendToClientDistributedEventHandler)} received eventData[{nameof(SendToClientDistributedEto)}]:{eventData}");

        var cacheKey = eventData.CacheKey;
        //var payload = eventData.CacheKey;
        var command = eventData.Command;
        var ignoreConnections = new HashSet<string>();

        //await SessionUnitManager.GetOrAddByMessageAsync(message);

        //var sessionUnitInfoList = await SessionUnitManager.GetCacheListAsync(cacheKey);
        var sessionId = eventData.Message.SessionId;

        // 使用 SessionUnitCache 代替 SessionUnitManager --2025.12.2
        var sessionUnitInfoList = (await SessionUnitCacheManager.GetListBySessionAsync(sessionId)).ToList();

        Logger.LogInformation($"{nameof(SendToClientDistributedEventHandler)}-Command-{command}-MessageId-{eventData.MessageId}");

        Logger.LogInformation($"{nameof(SendToClientDistributedEventHandler)}-CurrentHost={CurrentHosted.Name},eventData.HostName={eventData.HostName}");

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

        var connectionPools = (await ConnectionPoolManager.CreateQueryableAsync())
            //由后台作业发时,后台作业的HostName与SignalR的HostName 可能不一样,所以先行注释------2025.08.21(Iczp.Net)
            //.Where(x => x.Host == CurrentHosted.Name)
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
                .Select(chatObjectId => new CommandPayload.ScopeUnit
                {
                    ChatObjectId = chatObjectId,
                    SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                }).ToList();

            var commandPayload = new CommandPayload()
            {
                AppUserId = item.UserId,
                Scopes = units,//sessionUnitCaches.Select(x=>x as object).ToList(),
                //Caches = sessionUnitCaches,
                Command = command,
                Payload = eventData,
            };

            Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}]:{connectionPools.Count},commandPayload={JsonSerializer.Serialize(commandPayload)}");

            await HubContext.Clients.Client(item.ConnectionId).ReceivedMessage(commandPayload);
        }
    }
}
