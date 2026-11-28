using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace IczpNet.Chat.Calls;

public class CallAppService(CallSessionManager callSessionManager) : ApplicationService, ICallAppService
{
    private (long ownerId, string deviceId, string deviceSessionId) ResolveAuthenticatedDevice()
    {
        var deviceId = CurrentUser.FindClaimValue("device_id");
        var deviceSessionId = CurrentUser.FindClaimValue("device_session_id") ?? deviceId;
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new BusinessException(CallErrorCodes.DeviceNotAuthorized);
        // OwnerId resolved via existing ChatObject/CurrentUser mapping in concrete host
        return (0, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> InviteAsync(CallInviteInput input)
    {
        var (_, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        var unit = await LazyServiceProvider.LazyGetRequiredService<IczpNet.Chat.SessionUnits.ISessionUnitManager>()
            .GetCacheAsync(input.SenderSessionUnitId);
        return await callSessionManager.InviteAsync(input, unit?.OwnerId ?? 0, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> AcceptAsync(CallAcceptInput input)
    {
        var (_, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        return await callSessionManager.AcceptAsync(input, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> JoinAsync(CallActionInput input)
    {
        var (_, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        return await callSessionManager.JoinAsync(input, deviceId, deviceSessionId);
    }

    public async Task RejectAsync(CallRejectInput input)
    {
        var (_, deviceId, _) = ResolveAuthenticatedDevice();
        await callSessionManager.RejectAsync(input, deviceId);
    }

    public async Task CancelAsync(CallActionInput input)
    {
        var (_, deviceId, _) = ResolveAuthenticatedDevice();
        await callSessionManager.CancelAsync(input, deviceId);
    }

    public async Task HangupAsync(CallActionInput input)
    {
        var (_, deviceId, _) = ResolveAuthenticatedDevice();
        await callSessionManager.HangupAsync(input, deviceId);
    }

    public async Task<CallHeartbeatResultDto> HeartbeatAsync(CallHeartbeatInput input)
    {
        var (_, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        return await callSessionManager.HeartbeatAsync(input, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> RefreshTokenAsync(CallHeartbeatInput input)
    {
        var (_, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        return await callSessionManager.ReissueJoinResultAsync(input.CallId, input.SessionUnitId, deviceId, deviceSessionId);
    }

    public async Task<ActiveCallSnapshotDto> GetActiveAsync(System.Guid? sessionId = null)
    {
        return await callSessionManager.GetActiveAsync(CurrentUser.Id ?? System.Guid.Empty, sessionId);
    }
}
