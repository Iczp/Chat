using System;
using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
namespace IczpNet.Chat.ChatHubs;

public sealed class DeviceSessionHubFilter(IDeviceManager devices, IConfiguration configuration,
    IczpNet.OpenIddict.Tokens.ITokenRevocationChecker tokens, ICallerContextManager callers) : IHubFilter
{
    private async Task Validate(HubCallerContext context)
    {
        if (context.User != null && !await tokens.IsTokenValidAsync(context.User, context.ConnectionAborted))
        {
            await callers.AbortAsync(context.ConnectionId, "登录凭据已撤销，请重新登录。");
            context.Abort();
            throw new HubException("登录凭据已撤销，请重新登录。");
        }
        if (!configuration.GetValue("Device:UseAppDevice", false) || context.User == null) return;
        var ticket = DeviceSessionTicket.Read(context.User);
        if (ticket == null || await devices.IsSessionValidAsync(ticket, context.ConnectionAborted)) return;
        context.Abort();
        throw new HubException("该设备的登录已退出，请重新登录。");
    }
    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        await Validate(context.Context);
        await next(context);
        // A revocation can arrive while the hub is building/registering the connection.
        await Validate(context.Context);
    }
    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object>> next)
    { await Validate(context.Context); return await next(context); }
}
