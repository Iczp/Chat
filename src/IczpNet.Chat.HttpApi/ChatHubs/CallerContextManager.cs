using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq;
using System;
using IczpNet.AppDevice.Devices;
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

    public async Task AbortDeviceSessionsAsync(DeviceChangedEto notice)
    {
        if (notice.Kind != "Logout" || notice.RevokedSessions == null) return;
        // The distributed event reaches every host. Each host only aborts its own live handles.
        foreach (var caller in ConnectionIdToContextMap.Values)
        {
            var principal = caller.Context.User;
            if (principal == null) continue;
            var ticket = DeviceSessionTicket.Read(principal);
            if (ticket == null || ticket.TenantId != notice.TenantId || ticket.AppId != notice.AppId ||
                notice.DeviceKeys?.Contains(ticket.DeviceId) != true) continue;
            if (!notice.RevokedSessions.Any(x => x.UserId == ticket.UserId &&
                (ticket.SessionId.HasValue ? x.SessionId == ticket.SessionId :
                    notice.RevokedAt.HasValue && ticket.IssuedAt.HasValue && ticket.IssuedAt <=
                        new DateTimeOffset(notice.RevokedAt.Value.ToUniversalTime()).ToUnixTimeSeconds()))) continue;
            await AbortAsync(caller.Context.ConnectionId, notice.Reason ?? "该设备的登录已退出，请重新登录。");
        }
    }

    public async Task AbortAsync(string connectionId, string reason)
    {
        if (ConnectionIdToContextMap.TryGetValue(connectionId, out var callerContext))
        {
            Logger.LogWarning($"主动踢出 {connectionId}");

            try
            {
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
            }
            catch (Exception error)
            {
                Logger.LogWarning(error, "Kicked notification failed for connection {ConnectionId}; aborting the connection.", connectionId);
            }
            finally { callerContext.Context.Abort(); }
        }
        else
        {
            Logger.LogWarning($"主动踢出 {connectionId} 失败: 可能已经释放");
        }
    }
}
