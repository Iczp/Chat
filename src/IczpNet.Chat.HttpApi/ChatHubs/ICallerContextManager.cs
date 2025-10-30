using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface ICallerContextManager
{
    Task<CallerContext> ConnectedAsync(CallerContext caller);

    Task<CallerContext> ConnectedAsync(HubCallerContext context, ConnectedEto connectedEto);

    Task<bool> DisconnectedAsyncAsync(string connectionId);

    Task AbortAsync(string connectionId, string reason);
}
