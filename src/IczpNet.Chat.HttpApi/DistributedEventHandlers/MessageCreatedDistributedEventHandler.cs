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
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Json;

namespace IczpNet.Chat.DistributedEventHandlers;

public class MessageCreatedDistributedEventHandler(
    //IMessageRepository messageRepository,
    //IUnitOfWorkManager unitOfWorkManager,
    ISessionUnitManager sessionUnitManager,
    IConnectionPoolManager connectionPoolManager,
    ICurrentHosted currentHosted,
    IJsonSerializer jsonSerializer,
    IHubContext<ChatHub, IChatClient> hubContext) : DomainService, IDistributedEventHandler<MessageCreatedEto>
{
    //public IMessageRepository MessageRepository { get; } = messageRepository;
    //public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public IHubContext<ChatHub, IChatClient> HubContext { get; } = hubContext;

    public async Task HandleEventAsync(MessageCreatedEto eventData)
    {
        Logger.LogInformation($"[{nameof(MessageCreatedDistributedEventHandler)}] eventData:{eventData}");

        //using var uow = UnitOfWorkManager.Begin();

        //var message = await MessageRepository.GetAsync(eventData.MessageId);

        var ignoreConnections = new List<string>();

        var sessionUnitInfoList = await SessionUnitManager.GetCacheListAsync(eventData.CacheKey);

        if (sessionUnitInfoList == null)
        {
            Logger.LogWarning($"sessionUnitInfoList is null, eventData:{eventData}");
            return;
        }

        Logger.LogInformation($"Target session unit count:{sessionUnitInfoList.Count}");

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

            var payload = new PushPayload()
            {
                AppUserId = item.AppUserId,
                Scopes = units,//sessionUnitCaches.Select(x=>x as object).ToList(),
                //Caches = sessionUnitCaches,
                Command = "NewMessage",
                Payload = eventData,
            };

            Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}]:{connectionPools.Count},payload={JsonSerializer.Serialize(payload)}");

            await HubContext.Clients.Client(item.ConnectionId).ReceivedMessage(payload);
        }
    }
}
