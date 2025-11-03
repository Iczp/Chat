using AutoMapper.Internal.Mappers;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Devices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Clients;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.QrLogins;

[Authorize]
public class QrLoginHub(ICurrentClient currentClient) : AbpHub<IQrLoginClient>
{
    public ICurrentClient CurrentClient { get; } = currentClient;

    public override async Task OnConnectedAsync()
    {
        var clienId = CurrentClient.Id;

        if (string.IsNullOrWhiteSpace(clienId))
        {
            Logger.LogWarning($"ClientId is null");
            //Context.Abort();
            return;
        }

        var httpContext = Context.GetHttpContext();

        var deviceId = httpContext?.Request.Query["deviceId"];

        var deviceType = httpContext?.Request.Query["deviceType"];

        var queryId = httpContext?.Request.Query["id"];

        Logger.LogWarning($"clienId={clienId},deviceId={deviceId},deviceType={deviceType},queryId={queryId}");

        await Groups.AddToGroupAsync(Context.ConnectionId, CurrentUser.Id.ToString());

        await Clients.Caller.ReceivedMessage(new LoginCommandPayload()
        {
            Command = "UserId",
            Payload = Context.ConnectionId,
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<long> Heartbeat(long ticks)
    {
        await Task.Yield();
        Logger.LogInformation($"Heartbeat:{ticks}");
        return ticks;
    }
}
