using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.AppDevice.Devices;
using IczpNet.Chat.ChatHubs;
using IczpNet.Chat.ConnectionPools;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.AppDevice;

public sealed class AppDeviceChatDeviceContext(IDeviceResolver resolver) : IDeviceContext
{
    public Task<string?> GetDeviceIdAsync() => resolver.GetDeviceIdAsync();
}

public sealed class AppDeviceChatSessionValidator(IDeviceManager devices) : IChatDeviceSessionValidator
{
    public async Task<bool> IsValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var ticket = DeviceSessionTicket.Read(principal);
        return ticket == null || await devices.IsSessionValidAsync(ticket, cancellationToken);
    }
}

public sealed class DeviceLogoutEventHandler(ICallerContextManager callers)
    : IDistributedEventHandler<DeviceChangedEto>, ITransientDependency
{
    public Task HandleEventAsync(DeviceChangedEto notice)
    {
        if (notice.Kind != "Logout" || notice.RevokedSessions == null) return Task.CompletedTask;
        return callers.AbortDeviceSessionsAsync(principal =>
        {
            var ticket = DeviceSessionTicket.Read(principal);
            if (ticket == null || ticket.TenantId != notice.TenantId || ticket.AppId != notice.AppId ||
                notice.DeviceKeys?.Contains(ticket.DeviceId) != true) return false;
            return notice.RevokedSessions.Any(x => x.UserId == ticket.UserId &&
                (ticket.SessionId.HasValue ? x.SessionId == ticket.SessionId :
                    notice.RevokedAt.HasValue && ticket.IssuedAt.HasValue && ticket.IssuedAt <=
                        new DateTimeOffset(notice.RevokedAt.Value.ToUniversalTime()).ToUnixTimeSeconds()));
        }, notice.Reason ?? "该设备的登录已退出，请重新登录。");
    }
}

public sealed class AppDeviceChatDeviceLogout(IDeviceManager devices, IOnlineManager online,
    ICurrentPrincipalAccessor principalAccessor, ICurrentTenant currentTenant) : IChatDeviceLogout
{
    public async Task LogoutConnectionsAsync(List<string> connectionIds, string reason)
    {
        var principal = principalAccessor.Principal;
        var connections = await online.GetManyAsync(connectionIds);
        using var tenant = currentTenant.Change(DeviceExtensions.ResolveTenantId(principal.Claims));
        foreach (var target in connections.Select(x => x.Value).Where(x => x?.UserId != null &&
            !string.IsNullOrWhiteSpace(x.AppId) && !string.IsNullOrWhiteSpace(x.DeviceId))
            .DistinctBy(x => (x.UserId, x.AppId, x.DeviceId))
            .OrderBy(x => x.AppId, StringComparer.Ordinal).ThenBy(x => x.DeviceId, StringComparer.Ordinal).ThenBy(x => x.UserId))
        {
            devices.RequireClientApp(principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value, target.AppId);
            try { await devices.LogoutAsync(currentTenant.Id, target.AppId, target.DeviceId, target.UserId, reason); }
            catch (BusinessException error) when (error.Code == "AppDevice:Unavailable") { }
        }
    }
}
