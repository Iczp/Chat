using IczpNet.Chat.CommandPayloads;
using IczpNet.Chat.Commands;
using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using IczpNet.OpenIddict.Tokens;

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

    public async Task AbortRevokedTokensAsync(TokenRevokedEto notice)
    {
        foreach (var caller in ConnectionIdToContextMap.Values)
        {
            var principal = caller.Context.User;
            if (principal == null) continue;
            var matches = notice.All || (!string.IsNullOrEmpty(notice.TokenId) && principal.FindFirst(TokenRevocationKeys.TokenIdClaim)?.Value == notice.TokenId)
                || (!string.IsNullOrEmpty(notice.AuthorizationId) && principal.FindFirst(TokenRevocationKeys.AuthorizationIdClaim)?.Value == notice.AuthorizationId)
                || (!string.IsNullOrEmpty(notice.Subject) &&
                    (principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value) == notice.Subject);
            // A delayed subject event must not kick a fresh login created after the revocation.
            if (matches && !await LazyServiceProvider.LazyGetRequiredService<ITokenRevocationChecker>()
                .IsTokenValidAsync(principal))
                await AbortAsync(caller.Context.ConnectionId, "登录凭据已撤销，请重新登录。");
        }
    }

    public async Task AbortDeviceSessionsAsync(Func<ClaimsPrincipal, bool> matches, string reason)
    {
        foreach (var caller in ConnectionIdToContextMap.Values)
        {
            var principal = caller.Context.User;
            if (principal != null && matches(principal))
                await AbortAsync(caller.Context.ConnectionId, reason);
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
