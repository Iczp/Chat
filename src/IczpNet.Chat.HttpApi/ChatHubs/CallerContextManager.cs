using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public class CallerContextManager : ChatHubService, ICallerContextManager
{

    /// <summary>
    /// ConnectionId HubCallerContext
    /// </summary>
    private static readonly ConcurrentDictionary<string, CallerContext> ConnectionIdToContextMap = new();

    public virtual Task<CallerContext> ConnectedAsync(CallerContext caller)
    {
        var connectionId = caller.Context.ConnectionId;

        ConnectionIdToContextMap.TryAdd(connectionId, caller);

        return Task.FromResult(caller);
    }

    public virtual Task<CallerContext> ConnectedAsync(HubCallerContext context, ConnectedEto connectedEto)
    {
        return ConnectedAsync(new CallerContext(context, connectedEto));
    }

    public virtual Task<bool> DisconnectedAsyncAsync(string connectionId)
    {
        var result = ConnectionIdToContextMap.TryRemove(connectionId, out _);

        return Task.FromResult(result);
    }

    public async Task AbortAsync(string connectionId, string reason)
    {
        if (ConnectionIdToContextMap.TryGetValue(connectionId, out var callerContext))
        {
            Logger.LogWarning($"主动踢出 {connectionId}");

            await HubContext.Clients.Client(connectionId).ReceivedMessage(new CommandPayload()
            {
                AppUserId = callerContext.Connect.UserId,
                Scopes = [],
                Command = CommandConsts.Kicked,
                Payload = new
                {
                    Reason = reason
                }
            });
            callerContext.Context.Abort();
        }
        else
        {
            Logger.LogWarning($"主动踢出 {connectionId} 失败: 可能已经释放");
        }
    }
}
