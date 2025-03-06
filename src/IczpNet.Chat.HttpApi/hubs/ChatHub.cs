using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Hubs;
[Authorize]
public class ChatHub(
    IConnectionManager connectionManager,
    IWebClientInfoProvider webClientInfoProvider,
    IChatObjectManager chatObjectManager,
    IBackgroundJobManager backgroundJobManager,
    ILocalEventBus localEventBus,
    IConnectionPoolManager connectionPoolManager) : AbpHub// AbpHub<IChatClient>
{

    public IConnectionManager ConnectionManager { get; } = connectionManager;
    public IWebClientInfoProvider WebClientInfoProvider { get; } = webClientInfoProvider;
    public IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    public IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    public ILocalEventBus LocalEventBus { get; } = localEventBus;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;

    //[UnitOfWork]
    public override async Task OnConnectedAsync()
    {
        Logger.LogInformation($"[OnConnected] ConnectionId:{Context.ConnectionId}.UserName: {CurrentUser.UserName}");
        Logger.LogInformation($"[BrowserInfo] {WebClientInfoProvider.BrowserInfo}");
        Logger.LogInformation($"[DeviceInfo] {WebClientInfoProvider.DeviceInfo}");
        Logger.LogInformation($"[ClientIpAddress] {WebClientInfoProvider.ClientIpAddress}");
        var httpContext = Context.GetHttpContext();

        string deviceId = httpContext?.Request.Query["deviceId"];

        string queryId = httpContext?.Request.Query["id"];

        Logger.LogWarning($"DeviceId:{deviceId}");

        if (!CurrentUser.Id.HasValue)
        {
            Logger.LogWarning($"User is null");
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUser.Id.ToString());

        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);

        await ConnectionPoolManager.AddAsync(new ConnectionPoolCacheItem()
        {
            QueryId = queryId,
            ConnectionId = Context.ConnectionId,
            Host = Dns.GetHostName(),
            AppUserId = CurrentUser.Id.Value,
            DeviceId = deviceId,
            ChatObjectIdList = chatObjectIdList,
            CreationTime = Clock.Now,
        });

        //await ConnectionManager.CreateAsync(new Connection(Context.ConnectionId, chatObjectIdList)
        //{
        //    AppUserId = CurrentUser.Id,
        //    IpAddress = WebClientInfoProvider.ClientIpAddress,
        //    //ServerHostId = "host",
        //    DeviceId = deviceId,
        //    BrowserInfo = WebClientInfoProvider.BrowserInfo,
        //    DeviceInfo = WebClientInfoProvider.DeviceInfo,
        //});
        await base.OnConnectedAsync();
    }

    //[UnitOfWork]
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;

        var userName = CurrentUser.UserName;

        Logger.LogInformation($"[OnDisconnected],ConnectionId:{connectionId}.UserName: {userName}");

        if (CurrentUser.Id.HasValue)
        {
            await Groups.RemoveFromGroupAsync(connectionId, CurrentUser.Id.Value.ToString());
        }

        //await LocalEventBus.PublishAsync(new ConnectionRemovedEventData(connectionId), onUnitOfWorkComplete: false);

        try
        {
            //ConnectionPoolManager.Remove(connectionId);

            //var onlineCount = await ConnectionPoolManager.TotalCountAsync(Dns.GetHostName());

            //Logger.LogWarning($"[OnDisconnectedAsync] onlineCount: {onlineCount}");

            // 注：这里的删除操作可能会被取消，所以需要捕获TaskCanceledException异常
            await ConnectionPoolManager.RemoveAsync(connectionId, new CancellationTokenSource().Token);

            //await ConnectionManager.RemoveAsync(Context.ConnectionId);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"TaskCanceledException while deleting connection {connectionId}. UserName: {userName}");

            //if (!BackgroundJobManager.IsAvailable())
            //{
            //    Logger.LogWarning($"BackgroundJobManager.IsAvailable(): False");
            //}
            //else
            //{
            //    Logger.LogWarning($"[{nameof(ConnectionRemoveJob)}] deleting connection {connectionId}. UserName: {userName}");
            //    // 使用后台任务删除连接
            //    await BackgroundJobManager.EnqueueAsync(new ConnectionRemoveJobArgs(connectionId), BackgroundJobPriority.High);
            //}
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

        await Clients.All.SendAsync("ReceivedMessage", new { all });
        //await Clients
        //    .User(targetUser.Id.ToString())
        //    .SendAsync("ReceiveMessage", message);
    }
}
