using DeviceDetectorNET;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.SessionUnits;
using IczpNet.Pusher.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Json;

namespace IczpNet.Chat.ChatHubs;

public abstract class ChatHubService : DomainService
{
    public IHubContext<ChatHub, IChatClient> HubContext => LazyServiceProvider.LazyGetRequiredService<IHubContext<ChatHub, IChatClient>>();
    public IConnectionPoolManager ConnectionPoolManager => LazyServiceProvider.LazyGetRequiredService<IConnectionPoolManager>();
    public ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();

    /// <summary>
    /// 我的朋友
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    protected virtual async Task<List<string>> GetFriendsConnectionIdsAsync(Guid userId)
    {
        var sessionUnitList = await SessionUnitManager.GetUserFriendsAsync(userId);

        var chatObjectIdList = sessionUnitList.Select(x => x.OwnerId);

        // 我的朋友 

        var firendsConnectionIds = (await ConnectionPoolManager.GetAllListAsync())
            .Where(x => x.Host == CurrentHosted.Name)
            .Where(x => x.ChatObjectIdList.Any(d => chatObjectIdList.Contains(d)))
            .Select(x => x.ConnectionId)
            .ToList();
            ;

        return firendsConnectionIds;
    }

    /// <summary>
    /// 发送给朋友
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="commandPayload"></param>
    /// <returns></returns>
    protected virtual async Task SendToFriendsAsync(Guid userId, PushPayload commandPayload)
    {
        var firendsConnectionIds = await GetFriendsConnectionIdsAsync(userId);

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}] FriendsCount:{firendsConnectionIds.Count},commandPayload={JsonSerializer.Serialize(commandPayload)}");

        await HubContext.Clients.Clients(firendsConnectionIds).ReceivedMessage(commandPayload);
    }
}
