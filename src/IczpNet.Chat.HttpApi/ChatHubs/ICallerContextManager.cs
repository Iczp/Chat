using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace IczpNet.Chat.ChatHubs;

public interface ICallerContextManager
{
    Task<CallerContext> ConnectedAsync(CallerContext caller);

    Task<CallerContext> ConnectedAsync(HubCallerContext context, ConnectedEto connectedEto);

    Task<bool> DisconnectedAsyncAsync(string connectionId);

    Task AbortDeviceSessionsAsync(Func<ClaimsPrincipal, bool> matches, string reason);
    Task AbortRevokedTokensAsync(IczpNet.OpenIddict.Tokens.TokenRevokedEto notice);

    Task AbortAsync(string connectionId, string reason);
}
