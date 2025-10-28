using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Devices;
using IczpNet.Chat.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.Clients;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;


namespace IczpNet.Chat.ChatHubs;

[Authorize]
public class ChatHub(
    IConnectionManager connectionManager,
    IWebClientInfoProvider webClientInfoProvider,
    IChatObjectManager chatObjectManager,
    ICurrentHosted currentHosted,
    ICurrentClient currentClient,
    IObjectMapper objectMapper,
    IDistributedEventBus distributedEventBus,
    ICallerContextManager hubCallerContextManager,
    IConnectionPoolManager connectionPoolManager) : AbpHub<IChatClient>
{

    /// <summary>
    /// ConnectionId HubCallerContext
    /// </summary>
    private static readonly ConcurrentDictionary<string, HubCallerContext> ConnectionIdToContextMap = new();
    /// <summary>
    ///  DeviceId HubCallerContext[]
    /// </summary>
    private static readonly ConcurrentDictionary<string, List<HubCallerContext>> DeviceIdToContextMap = new();

    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public IWebClientInfoProvider WebClientInfoProvider { get; } = webClientInfoProvider;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public ICurrentClient CurrentClient { get; } = currentClient;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public ICallerContextManager HubCallerContextManager { get; } = hubCallerContextManager;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;

    [UnitOfWork]
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();

            if (!CurrentUser.Id.HasValue)
            {
                Logger.LogWarning($"User is null");
                Context.Abort();
                return;
            }

            var deviceId = CurrentUser.GetDeviceId() ?? httpContext?.Request.Query["deviceId"];

            var deviceType = CurrentUser.GetDeviceType() ?? httpContext?.Request.Query["deviceType"];

            var queryId = httpContext?.Request.Query["id"];

            Logger.LogWarning($"DeviceId:{deviceId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUser.Id.ToString());

            var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);

            Logger.LogWarning($"chatObjectIdList:[{chatObjectIdList.JoinAsString(",")}]");

            var connectedEto = new ConnectedEto()
            {
                QueryId = queryId,
                ClientId = CurrentClient.Id,
                ConnectionId = Context.ConnectionId,
                Host = CurrentHosted.Name,
                IpAddress = WebClientInfoProvider.ClientIpAddress,
                UserId = CurrentUser.Id,
                UserName = CurrentUser.UserName,
                DeviceId = deviceId,
                DeviceType = deviceType,
                BrowserInfo = WebClientInfoProvider.BrowserInfo,
                DeviceInfo = WebClientInfoProvider.DeviceInfo,
                CreationTime = Clock.Now,
                ChatObjectIdList = chatObjectIdList,
            };

            Logger.LogInformation($"[OnConnectedAsync] connectedEto= {connectedEto}");

            await ConnectionPoolManager.AddAsync(connectedEto);

            await HubCallerContextManager.AddAsync(Context, connectedEto);

            //await Clients.User(CurrentUser.Id?.ToString()).ReceivedMessage(new CommandPayload()
            //{
            //    AppUserId = CurrentUser.Id,
            //    Command = "Welcome",
            //    Payload = connectedEto,
            //});

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

        Logger.LogInformation($"[OnDisconnected],ConnectionId:{connectionId}.UserName: {userName}");

        if (CurrentUser.Id.HasValue)
        {
            await Groups.RemoveFromGroupAsync(connectionId, CurrentUser.Id.Value.ToString());
        }

        try
        {
            var cancellationToken = new CancellationTokenSource().Token;

            // 注：这里的删除操作可能会被取消，所以需要捕获TaskCanceledException异常
            await HubCallerContextManager.RemoveAsync(connectionId);

            // 删除前获取连接
            var connection = await ConnectionPoolManager.GetAsync(connectionId, cancellationToken);

            var disconnectedEto = connection != null
                ? ObjectMapper.Map<ConnectionPoolCacheItem, DisconnectedEto>(connection)
                : new DisconnectedEto(connectionId);

            // 删除连接
            await ConnectionPoolManager.RemoveAsync(connectionId, cancellationToken);

            // 发布事件
            await DistributedEventBus.PublishAsync(disconnectedEto, onUnitOfWorkComplete: false);

            //await Clients.User(CurrentUser.Id?.ToString()).ReceivedMessage(new CommandPayload()
            //{
            //    AppUserId = CurrentUser.Id,
            //    Command = "Goodbye",
            //    Payload = onDisconnectedEto,
            //});

            //await ConnectionManager.RemoveAsync(Context.ConnectionId, new CancellationTokenSource().Token);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"[OnDisconnectedAsync] TaskCanceledException while deleting connection {connectionId}. UserName: {userName}");
            // 使用同步方法删除连接池
            await ConnectionPoolManager.RemoveAsync(connectionId);
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
        return ticks;
    }
}
