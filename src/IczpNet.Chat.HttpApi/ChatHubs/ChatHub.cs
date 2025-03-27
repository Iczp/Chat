using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Hosting;
using IczpNet.Pusher.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.ChatHubs;

[Authorize]
public class ChatHub(
    IConnectionManager connectionManager,
    IWebClientInfoProvider webClientInfoProvider,
    IChatObjectManager chatObjectManager,
    ICurrentHosted currentHosted,
    IDistributedEventBus distributedEventBus,
    IConnectionPoolManager connectionPoolManager) : AbpHub<IChatClient>
{

    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public IWebClientInfoProvider WebClientInfoProvider { get; } = webClientInfoProvider;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;

    [UnitOfWork]
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = Context.GetHttpContext();

            string deviceId = httpContext?.Request.Query["deviceId"];

            string clientId = httpContext?.Request.Query["id"];

            Logger.LogWarning($"DeviceId:{deviceId}");

            if (!CurrentUser.Id.HasValue)
            {
                Logger.LogWarning($"User is null");
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUser.Id.ToString());

            var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);

            Logger.LogWarning($"chatObjectIdList:[{chatObjectIdList.JoinAsString(",")}]");

            var connectedEto = new OnConnectedEto()
            {
                ClientId = clientId,
                ConnectionId = Context.ConnectionId,
                Host = CurrentHosted.Name,
                IpAddress = WebClientInfoProvider.ClientIpAddress,
                UserId = CurrentUser.Id.Value,
                UserName = CurrentUser.UserName,
                DeviceId = deviceId,
                BrowserInfo = WebClientInfoProvider.BrowserInfo,
                DeviceInfo = WebClientInfoProvider.DeviceInfo,
                CreationTime = Clock.Now,
                ChatObjectIdList = chatObjectIdList,
            };

            Logger.LogInformation($"[OnConnectedAsync] connectedEto= {connectedEto}");

            await ConnectionPoolManager.AddAsync(connectedEto);

            await Clients.Caller.ReceivedMessage(new PushPayload()
            {
                AppUserId = CurrentUser.Id,
                Command = "Welcome",
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

        Logger.LogInformation($"[OnDisconnected],ConnectionId:{connectionId}.UserName: {userName}");

        if (CurrentUser.Id.HasValue)
        {
            await Groups.RemoveFromGroupAsync(connectionId, CurrentUser.Id.Value.ToString());
        }

        try
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var connection = await ConnectionPoolManager.GetAsync(connectionId, cancellationToken);

            // 注：这里的删除操作可能会被取消，所以需要捕获TaskCanceledException异常
            await ConnectionPoolManager.RemoveAsync(connectionId, cancellationToken);

            var onDisconnectedEto = connection?.As<OnDisconnectedEto>() ?? new OnDisconnectedEto(connectionId);
            // 发布事件
            await DistributedEventBus.PublishAsync(onDisconnectedEto, onUnitOfWorkComplete: false);

            //await ConnectionManager.RemoveAsync(Context.ConnectionId, new CancellationTokenSource().Token);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"TaskCanceledException while deleting connection {connectionId}. UserName: {userName}");
            // 使用同步方法删除连接池
            ConnectionPoolManager.Remove(connectionId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while deleting connection {connectionId}. UserName: {userName}");
            // 处理其他异常
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageAsync(string targetUserName, string message)
    {
        var all = await ConnectionPoolManager.GetAllListAsync();

        await Clients.All.ReceivedMessage(new PushPayload()
        {
            Command = message,
            Payload = all.ToList(),
        });
    }
}
