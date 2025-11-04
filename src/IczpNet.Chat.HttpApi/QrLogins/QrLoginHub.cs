using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Devices;
using IczpNet.Chat.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.Clients;

namespace IczpNet.Chat.QrLogins;

[Authorize]
public class QrLoginHub(
    IWebClientInfoProvider webClientInfoProvider,
    ICurrentHosted currentHosted,
    ICurrentClient currentClient,
    IScanLoginConnectionPoolManager scanLoginConnectionPoolManager) : AbpHub<IQrLoginClient>
{
    public ICurrentClient CurrentClient { get; } = currentClient;
    public IWebClientInfoProvider WebClientInfoProvider { get; } = webClientInfoProvider;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;
    public IScanLoginConnectionPoolManager ScanLoginConnectionPoolManager { get; } = scanLoginConnectionPoolManager;

    protected async Task<ConnectionPool> BuildInfoAsync()
    {
        await Task.Yield();

        var httpContext = Context.GetHttpContext();

        var appId = CurrentUser.GetAppId() ?? httpContext?.Request.Query["appId"];

        var deviceId = CurrentUser.GetDeviceId() ?? httpContext?.Request.Query["deviceId"];

        var deviceType = CurrentUser.GetDeviceType() ?? httpContext?.Request.Query["deviceType"];

        var queryId = httpContext?.Request.Query["id"];

        Logger.LogWarning($"DeviceId:{deviceId}");

        var connectedEto = new ConnectionPool()
        {
            AppId = appId,
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
        };

        return connectedEto;

    }

    public override async Task OnConnectedAsync()
    {
        var connectionPool = await BuildInfoAsync();

        await ScanLoginConnectionPoolManager.ConnectedAsync(connectionPool);

        await Clients.Caller.ReceivedMessage(new LoginCommandPayload()
        {
            Command = "Welcome",
            Payload = connectionPool,
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;

        try
        {
            var cancellationToken = new CancellationTokenSource().Token;

            //// 删除前获取连接
            //var connection = await ScanLoginConnectionPoolManager.GetAsync(connectionId, cancellationToken);

            // 删除连接
            await ScanLoginConnectionPoolManager.DisconnectedAsync(connectionId, cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, $"[OnDisconnectedAsync] TaskCanceledException while deleting connection {connectionId}.");
            // 使用同步方法删除连接池
            await ScanLoginConnectionPoolManager.DisconnectedAsync(connectionId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"[OnDisconnectedAsync] Error while deleting connection {connectionId}.");
            // 处理其他异常
            var _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                await ScanLoginConnectionPoolManager.DisconnectedAsync(connectionId);
            });
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<long> Heartbeat(long ticks)
    {
        Logger.LogInformation($"Heartbeat:{ticks}");

        var connection = await ScanLoginConnectionPoolManager.UpdateActiveTimeAsync(Context.ConnectionId);

        if (connection != null)
        {
            // 可以发布事件
        }
        return ticks;
    }
}
