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
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Hubs;
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
        Logger.LogInformation($"[OnConnected] ConnectionId:{Context.ConnectionId}.UserName: {CurrentUser.UserName}");
        Logger.LogInformation($"[BrowserInfo] {WebClientInfoProvider.BrowserInfo}");
        Logger.LogInformation($"[DeviceInfo] {WebClientInfoProvider.DeviceInfo}");
        Logger.LogInformation($"[ClientIpAddress] {WebClientInfoProvider.ClientIpAddress}");
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
            AppUserId = CurrentUser.Id.Value,
            DeviceId = deviceId,
            BrowserInfo = WebClientInfoProvider.BrowserInfo,
            DeviceInfo = WebClientInfoProvider.DeviceInfo,
            CreationTime = Clock.Now,
            ChatObjectIdList = chatObjectIdList,
        };

        try
        {
            await ConnectionPoolManager.AddAsync(connectedEto);

            // 发布事件
            await DistributedEventBus.PublishAsync(connectedEto, onUnitOfWorkComplete: false);

            //Logger.LogWarning($"[OnConnectedAsync] ConnectionManager.CreateAsync");

            //var cancellationToken = new CancellationTokenSource().Token;

            //await ConnectionManager.CreateAsync(new Connection(Context.ConnectionId, chatObjectIdList)
            //{
            //    AppUserId = CurrentUser.Id,
            //    IpAddress = WebClientInfoProvider.ClientIpAddress,
            //    ServerHostId = CurrentHosted.Name,
            //    ClientId = clientId,
            //    DeviceId = deviceId,
            //    BrowserInfo = WebClientInfoProvider.BrowserInfo,
            //    DeviceInfo = WebClientInfoProvider.DeviceInfo,
            //});
            //Logger.LogWarning($"[OnConnectedAsync] ConnectionManager.CreateAsync [End]");
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"[OnConnectedAsync] TaskCanceledException 任务取消");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OnConnectedAsync]: 写入数据库失败");
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
            await ConnectionPoolManager.RemoveAsync(connectionId, cancellationToken);
            // 发布事件
            await DistributedEventBus.PublishAsync(new OnDisconnectedEto(connectionId), onUnitOfWorkComplete: false);

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
        message = $"{CurrentUser.UserName}: {message}";

        var all = await ConnectionPoolManager.GetAllListAsync();

        await Clients.All.ReceivedMessage(new PushPayload()
        {
            Command = "NewMessage",
            Payload = all,
        });
        //await Clients
        //    .User(targetUser.Id.ToString())
        //    .SendAsync("ReceiveMessage", message);
    }
}
