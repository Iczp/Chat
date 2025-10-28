using DeviceDetectorNET;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Friends;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.SessionUnits;
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
    public IFriendsManager FriendManager => LazyServiceProvider.LazyGetRequiredService<IFriendsManager>();
    public ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetRequiredService<ICurrentHosted>();
    public IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();

    /// <summary>
    /// 我的朋友
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    protected virtual async Task<List<ConnectionPoolCacheItem>> GetFriendsConnectionIdsAsync(Guid userId)
    {
        var sessionUnitList = await FriendManager.GetFriendsAsync(userId);

        // 注意朋友是： DestinationId 不是 OwnerId
        var friendChatObjectIdList = sessionUnitList.Select(x => x.DestinationId);

        // 我的朋友 
        var firendConnectionIdList = (await ConnectionPoolManager.CreateQueryableAsync())
            .Where(x => x.Host == CurrentHosted.Name)
            .Where(x => x.ChatObjectIdList.Any(d => friendChatObjectIdList.Contains(d)))
            //.Select(x => x.ConnectionId)
            .ToList();
        ;

        return firendConnectionIdList;
    }

    /// <summary>
    /// 我的朋友
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    protected virtual async Task<List<string>> GetUserConnectionIdsAsync(Guid userId)
    {
        var userConnectionIds = (await ConnectionPoolManager.CreateQueryableAsync())
        .Where(x => x.Host == CurrentHosted.Name)
        .Where(x => x.UserId == userId)
        .Select(x => x.ConnectionId)
        .ToList();
        ;

        return userConnectionIds;
    }

    /// <summary>
    /// 发送给朋友
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="commandPayload"></param>
    /// <returns></returns>
    protected virtual async Task SendToFriendsAsync(Guid userId, CommandPayload commandPayload)
    {
        var firendConnectionList = await GetFriendsConnectionIdsAsync(userId);

        var firendConnectionIdList = firendConnectionList.Select(x => x.ConnectionId).ToList();

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}] FriendsCount:{firendConnectionIdList.Count},commandPayload={JsonSerializer.Serialize(commandPayload)}");

        foreach( var firendsConnection in firendConnectionList)
        {
            //修改为当前用户
            commandPayload.AppUserId = firendsConnection.UserId;

            await HubContext.Clients.Client(firendsConnection.ConnectionId).ReceivedMessage(commandPayload);

            //await HubContext.Clients.All.ReceivedMessage(commandPayload);
        }
    }

    /// <summary>
    /// 发送给用户
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="commandPayload"></param>
    /// <returns></returns>
    protected virtual async Task SendToUserAsync(Guid userId, CommandPayload commandPayload)
    {
        var userConnectionIds = await GetUserConnectionIdsAsync(userId);

        Logger.LogInformation($"Send [{nameof(IChatClient.ReceivedMessage)}] FriendsCount:{userConnectionIds.Count},commandPayload={JsonSerializer.Serialize(commandPayload)}");

        await HubContext.Clients.Clients(userConnectionIds).ReceivedMessage(commandPayload);
    }
}
