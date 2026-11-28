using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Users;

namespace IczpNet.Chat.Calls;

[Authorize]
public class CallAppService(CallSessionManager callSessionManager) : ApplicationService, ICallAppService
{
    private (Guid userId, string deviceId, string deviceSessionId) ResolveAuthenticatedDevice()
    {
        if (!CurrentUser.IsAuthenticated || !CurrentUser.Id.HasValue)
            throw new AbpAuthorizationException("User is not authenticated.");

        var deviceId = CurrentUser.FindClaimValue("device_id");
        var deviceSessionId = CurrentUser.FindClaimValue("device_session_id") ?? deviceId;
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new BusinessException(CallErrorCodes.DeviceNotAuthorized);

        return (CurrentUser.Id.Value, deviceId, deviceSessionId);
    }

    private async Task<SessionUnitCacheItem> EnsureSessionUnitOwnershipAsync(Guid sessionUnitId, Guid userId)
    {
        var sessionUnitManager = LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
        var unit = await sessionUnitManager.GetCacheAsync(sessionUnitId)
            ?? throw new BusinessException(CallErrorCodes.NotSessionMember);

        var chatObjectManager = LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>();
        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(userId);
        if (!ownerIds.Contains(unit.OwnerId))
            throw new BusinessException(CallErrorCodes.NotSessionMember);

        return unit;
    }

    public async Task<CallJoinResultDto> InviteAsync(CallInviteInput input)
    {
        var (userId, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        var unit = await EnsureSessionUnitOwnershipAsync(input.SenderSessionUnitId, userId);
        return await callSessionManager.InviteAsync(input, unit.OwnerId, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> AcceptAsync(CallAcceptInput input)
    {
        var (userId, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        return await callSessionManager.AcceptAsync(input, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> JoinAsync(CallActionInput input)
    {
        var (userId, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        return await callSessionManager.JoinAsync(input, deviceId, deviceSessionId);
    }

    public async Task RejectAsync(CallRejectInput input)
    {
        var (userId, deviceId, _) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        await callSessionManager.RejectAsync(input, deviceId);
    }

    public async Task CancelAsync(CallActionInput input)
    {
        var (userId, deviceId, _) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        await callSessionManager.CancelAsync(input, deviceId);
    }

    public async Task HangupAsync(CallActionInput input)
    {
        var (userId, deviceId, _) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        await callSessionManager.HangupAsync(input, deviceId);
    }

    public async Task<CallHeartbeatResultDto> HeartbeatAsync(CallHeartbeatInput input)
    {
        var (userId, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        return await callSessionManager.HeartbeatAsync(input, deviceId, deviceSessionId);
    }

    public async Task<CallJoinResultDto> RefreshTokenAsync(CallHeartbeatInput input)
    {
        var (userId, deviceId, deviceSessionId) = ResolveAuthenticatedDevice();
        await EnsureSessionUnitOwnershipAsync(input.SessionUnitId, userId);
        return await callSessionManager.ReissueJoinResultAsync(input.CallId, input.SessionUnitId, deviceId, deviceSessionId);
    }

    public async Task<ActiveCallSnapshotDto> GetActiveAsync(Guid? sessionId = null)
    {
        var (userId, _, _) = ResolveAuthenticatedDevice();
        return await callSessionManager.GetActiveAsync(userId, sessionId);
    }
}
