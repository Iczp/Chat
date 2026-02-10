using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;


namespace IczpNet.Chat.ChatHubs;

[Authorize]
public class ChatHub(
    IOnlineManager onlineManager,
    IConnectionManager connectionManager,
    IChatObjectManager chatObjectManager,
    IObjectMapper objectMapper,
    IDistributedEventBus distributedEventBus,
    ICallerContextManager hubCallerContextManager,
    IConnectionPoolManager connectionPoolManager) : HubBase<IChatClient, ConnectedEto>
{

    /// <summary>
    /// ConnectionId HubCallerContext
    /// </summary>
    private static readonly ConcurrentDictionary<string, HubCallerContext> ConnectionIdToContextMap = new();
    /// <summary>
    ///  DeviceId HubCallerContext[]
    /// </summary>
    private static readonly ConcurrentDictionary<string, List<HubCallerContext>> DeviceIdToContextMap = new();

    public IOnlineManager OnlineManager { get; } = onlineManager;
    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public ICallerContextManager HubCallerContextManager { get; } = hubCallerContextManager;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;


    protected override async Task<ConnectedEto> BuildInfoAsync()
    {
        var connectedEto = await base.BuildInfoAsync();

        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);

        Logger.LogWarning($"chatObjectIdList:[{chatObjectIdList.JoinAsString(",")}]");

        connectedEto.ChatObjectIdList = chatObjectIdList;

        return connectedEto;

    }
    //[UnitOfWork]
    public override async Task OnConnectedAsync()
    {
        try
        {
            if (!CurrentUser.Id.HasValue)
            {
                Logger.LogWarning($"User is null");
                Context.Abort();
                return;
            }

            var connectedEto = await BuildInfoAsync();

            Logger.LogInformation($"[OnConnectedAsync] connectedEto= {connectedEto}");

            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUser.Id.ToString());

            await ConnectionPoolManager.ConnectedAsync(connectedEto);

            await HubCallerContextManager.ConnectedAsync(Context, connectedEto);

            await OnlineManager.ConnectedAsync(connectedEto);

            await Clients.Caller.ReceivedMessage(new CommandPayload()
            {
                AppUserId = CurrentUser.Id,
                Command = CommandConsts.Welcome,
                Payload = connectedEto,
            });

            // 发布事件
            await DistributedEventBus.PublishAsync(connectedEto, onUnitOfWorkComplete: false);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"[OnConnectedAsync] TaskCanceledException 任务取消");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OnConnectedAsync]: 连接失败");
        }
        await base.OnConnectedAsync();
    }

    //[UnitOfWork]
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;

        var userName = CurrentUser.UserName;

        Logger.LogInformation(exception, $"[OnDisconnected],Exception:{exception?.Message}");

        Logger.LogInformation($"[OnDisconnected],UserId:{connectionId}.UserName: {userName}");

        if (CurrentUser.Id.HasValue)
        {
            await Groups.RemoveFromGroupAsync(connectionId, CurrentUser.Id.Value.ToString());
        }

        try
        {
            var cancellationToken = new CancellationTokenSource().Token;

            // 注：这里的删除操作可能会被取消，所以需要捕获TaskCanceledException异常
            await HubCallerContextManager.DisconnectedAsyncAsync(connectionId);

            // 删除前获取连接
            var connection = await ConnectionPoolManager.GetAsync(connectionId, cancellationToken);

            var disconnectedEto = connection != null
                ? ObjectMapper.Map<ConnectionPoolCacheItem, DisconnectedEto>(connection)
                : new DisconnectedEto(connectionId);

            // 发布事件(先发布事件，再删除)
            await DistributedEventBus.PublishAsync(disconnectedEto, onUnitOfWorkComplete: false);

            // 删除连接
            await ConnectionPoolManager.DisconnectedAsync(connectionId, cancellationToken);

            await OnlineManager.DisconnectedAsync(connectionId);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"[OnDisconnectedAsync] TaskCanceledException while deleting connection {connectionId}. UserName: {userName}");
            // 使用同步方法删除连接池
            await ConnectionPoolManager.DisconnectedAsync(connectionId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"[OnDisconnectedAsync] Error while deleting connection {connectionId}. UserName: {userName}");
            // 处理其他异常
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageAsync(string targetUserName, string message)
    {
        var all = await ConnectionPoolManager.CreateQueryableAsync();

        await Clients.All.ReceivedMessage(new CommandPayload()
        {
            Command = message,
            Payload = all.ToList(),
        });
    }

    public async Task<long> Heartbeat(long ticks)
    {
        Logger.LogInformation($"Heartbeat:{ticks}");

        var connection = await ConnectionPoolManager.UpdateActiveTimeAsync(Context.ConnectionId);

        if (connection != null)
        {
            var activedEto = ObjectMapper.Map<ConnectionPoolCacheItem, ActivedEto>(connection);
            await DistributedEventBus.PublishAsync(activedEto, onUnitOfWorkComplete: false);
        }

        await OnlineManager.UpdateActiveTimeAsync(Context.ConnectionId);

        return ticks;
    }
}
