using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.ScanLogins;

[Authorize]
public class ScanLoginHub(
    IObjectMapper objectMapper,
    IScanLoginManager scanLoginManager,
    IScanLoginConnectionPoolManager scanLoginConnectionPoolManager) : HubBase<IScanLoginClient, ConnectionPool>
{
    public IScanLoginManager ScanLoginManager { get; } = scanLoginManager;
    public IObjectMapper ObjectMapper { get; } = objectMapper;
    public IScanLoginConnectionPoolManager ScanLoginConnectionPoolManager { get; } = scanLoginConnectionPoolManager;

    protected override async Task<ConnectionPool> BuildInfoAsync()
    {
        var connectedEto = await base.BuildInfoAsync();
        return connectedEto;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionPool = await BuildInfoAsync();

        await ScanLoginConnectionPoolManager.ConnectedAsync(connectionPool);

        //var generateInfo = await Generate();

        await Clients.Caller.ReceivedMessage(new LoginCommandPayload()
        {
            Command = ScanLoginCommandConsts.Welcome,
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

    public async Task<GeneratedDto> Generate(string state)
    {
        var info = await ScanLoginManager.GenerateAsync(Context.ConnectionId, state);

        Logger.LogInformation($"Generate:{info}");

        return ObjectMapper.Map<GenerateInfo, GeneratedDto>(info);
    }
}
