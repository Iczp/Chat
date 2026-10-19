using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface ICallerContextManager
{
    Task<CallerContext> ConnectedAsync(CallerContext caller);

    Task<CallerContext> ConnectedAsync(HubCallerContext context, ConnectedEto connectedEto);

    Task<bool> DisconnectedAsyncAsync(string connectionId);

    Task AbortDeviceSessionsAsync(IczpNet.AppDevice.Devices.DeviceChangedEto notice);
    Task AbortRevokedTokensAsync(IczpNet.OpenIddict.Tokens.TokenRevokedEto notice);

    Task AbortAsync(string connectionId, string reason);
}
