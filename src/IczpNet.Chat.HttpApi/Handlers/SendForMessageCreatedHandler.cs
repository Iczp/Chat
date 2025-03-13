using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using IczpNet.Pusher.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;
using Volo.Abp.Json;

namespace IczpNet.Chat.Handlers;

public class SendForMessageCreatedEventHandler(
    IHubContext<ChatHub, IChatClient> hubContext,
    IConnectionPoolManager connectionPoolManager,
    IJsonSerializer jsonSerializer,
    ISessionUnitManager sessionUnitManager) : DomainService, ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
{
    public IHubContext<ChatHub, IChatClient> HubContext { get; } = hubContext;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
    public IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;

    public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
    {
        await Task.Yield();
        var message = eventData.Entity;
        Logger.LogInformation($"Created message:{message}");

        var isPrivateMessage = message.IsPrivateMessage();
        var sessionUnitInfoList = await SessionUnitManager.GetOrAddCacheListAsync(message.SessionId.Value);

        var dic = sessionUnitInfoList.Where(x => x.AppUserId.HasValue)
            .GroupBy(x => x.AppUserId)
            .ToDictionary(x => x.Key, x => x.Select(d => d.Id).ToList());

        var chatObjectIdList = sessionUnitInfoList
            .Where(x => x != null)
            //.Where(x => x.ServiceStatus == ServiceStatus.Online)
            .Select(x => x.OwnerId)
            .ToList();

        var onlineList = (await ConnectionPoolManager.GetAllListAsync())
            .Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .ToList();



        Logger.LogInformation($"Online count:{onlineList.Count}");

        //
        foreach (var poolInfo in onlineList)
        {
            //if (ignoreConnections != null && ignoreConnections.Any(x => x == poolInfo.ConnectionId))
            //{
            //    continue;
            //}

            //var sessionUnitCaches = sessionUnitInfoList.Where(x=> chatObjectIdList.Contains(x.OwnerId)).ToList();

            var units = poolInfo.ChatObjectIdList
                .Where(chatObjectIdList.Contains)
                .Select(chatObjectId => new ScopeUnit
                {
                    ChatObjectId = chatObjectId,
                    SessionUnitId = sessionUnitInfoList.Find(x => x.OwnerId == chatObjectId).Id
                }).ToList();

            var payload = new PushPayload()
            {
                AppUserId = poolInfo.AppUserId,
                Scopes = units,//sessionUnitCaches.Select(x=>x as object).ToList(),
                //Caches = sessionUnitCaches,
                //Command = commandPayload.Command,
                Command = "MessageCreated",
                Payload = new
                {
                    MessageId = message.Id,
                    SessionUnitIdList = units,
                },
            };

            //await PoolsManager.SendMessageAsync(poolInfo, payload);

            Logger.LogInformation($"Send [ReceivedMessage]:{onlineList.Count},payload={JsonSerializer.Serialize(payload)}");

            await HubContext.Clients.Client(poolInfo.ConnectionId).ReceivedMessage(payload);
        }
    }
}
