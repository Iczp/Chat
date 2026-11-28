using IczpNet.Chat.Commands;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Calls;

public class CallSessionManager(
    IRepository<CallRecord, Guid> callRecordRepository,
    IRepository<CallParticipant, Guid> participantRepository,
    IRepository<CallRoomLifecycle, Guid> roomLifecycleRepository,
    IRepository<CallIdempotencyRecord, Guid> idempotencyRepository,
    ISessionUnitManager sessionUnitManager,
    ILiveKitClient liveKitClient,
    IDistributedEventBus distributedEventBus,
    IOptions<LiveKitOptions> liveKitOptions,
    IUnitOfWorkManager unitOfWorkManager,
    IClock clock) : DomainService
{
    private LiveKitOptions Options => liveKitOptions.Value;

    public static string ComputeSummaryClientMessageId(Guid callId)
    {
        var b64 = Convert.ToBase64String(callId.ToByteArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"c_{b64}"; // 2 + 22 = 24 chars (<= 26 char limit)
    }

    public static string BuildRoomName(Guid callId, int roomEpoch) => $"gotoim_call_{callId:N}_v{roomEpoch}";

    public async Task<CallJoinResultDto> InviteAsync(
        CallInviteInput input,
        long currentOwnerId,
        string deviceId,
        string deviceSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(deviceSessionId))
            throw new BusinessException(CallErrorCodes.DeviceNotAuthorized);

        var senderUnit = await sessionUnitManager.GetCacheAsync(input.SenderSessionUnitId)
            ?? throw new BusinessException(CallErrorCodes.NotSessionMember);

        if (senderUnit.OwnerId != currentOwnerId || senderUnit.SessionId != input.SessionId || !senderUnit.IsEnabled)
            throw new BusinessException(CallErrorCodes.NotSessionMember);

        if (input.CallId.HasValue)
            return await InviteMoreAsync(input, senderUnit, deviceId, deviceSessionId, cancellationToken);

        // Check idempotency
        var existingIdem = await idempotencyRepository.FindAsync(
            x => x.SessionUnitId == input.SenderSessionUnitId && x.DeviceId == deviceId && x.Action == "invite" && x.RequestId == input.RequestId,
            cancellationToken: cancellationToken);
        if (existingIdem != null)
            return await ReissueJoinResultAsync(existingIdem.CallId, input.SenderSessionUnitId, deviceId, deviceSessionId, cancellationToken);

        var targets = input.TargetSessionUnitIds.Where(x => x != input.SenderSessionUnitId).Distinct().ToList();
        if (input.CallMode == CallMode.Direct && targets.Count != 1)
            throw new BusinessException(CallErrorCodes.InvalidStateTransition);
        if (input.CallMode == CallMode.Group && (targets.Count == 0 || 1 + targets.Count > Options.MaxGroupParticipants))
            throw new BusinessException(CallErrorCodes.RoomFull);

        var now = clock.Now;
        var callId = GuidGenerator.Create();
        var roomName = BuildRoomName(callId, 1);
        var ringExpireAt = now.AddSeconds(CallConsts.DefaultRingTimeoutSeconds);
        var connectExpireAt = now.AddSeconds(CallConsts.DefaultConnectTimeoutSeconds);

        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = new CallRecord(callId)
            {
                SessionId = input.SessionId,
                CallMode = input.CallMode,
                MediaType = input.MediaType,
                State = CallSessionState.Ringing,
                StateVersion = 1,
                InitiatorUnitId = senderUnit.Id,
                InitiatorOwnerId = senderUnit.OwnerId,
                InitiatorDeviceId = deviceId,
                RoomEpoch = 1,
                RoomName = roomName,
                BannerNotifyState = input.CallMode == CallMode.Group ? (byte)1 : (byte)0,
                RingExpireAt = ringExpireAt,
                LastActiveAt = now,
                NextCheckAt = connectExpireAt,
                SummaryClientMessageId = ComputeSummaryClientMessageId(callId),
                SummaryDispatchStatus = 0
            };
            await callRecordRepository.InsertAsync(record, autoSave: true, cancellationToken);

            await participantRepository.InsertAsync(new CallParticipant(GuidGenerator.Create())
            {
                CallId = callId,
                SessionUnitId = senderUnit.Id,
                OwnerId = senderUnit.OwnerId,
                IsActiveBusy = true,
                Status = CallParticipantStatus.Accepted,
                InviteRound = 0,
                InviteNotifyState = 0,
                DeviceId = deviceId,
                DeviceSessionId = deviceSessionId,
                ConnectExpireAt = connectExpireAt,
                AcceptedAt = now
            }, autoSave: true, cancellationToken);

            foreach (var targetId in targets)
            {
                var targetUnit = await sessionUnitManager.GetCacheAsync(targetId)
                    ?? throw new BusinessException(CallErrorCodes.NotSessionMember);
                if (targetUnit.SessionId != input.SessionId || !targetUnit.IsEnabled)
                    throw new BusinessException(CallErrorCodes.NotSessionMember);

                await participantRepository.InsertAsync(new CallParticipant(GuidGenerator.Create())
                {
                    CallId = callId,
                    SessionUnitId = targetUnit.Id,
                    OwnerId = targetUnit.OwnerId,
                    IsActiveBusy = true,
                    Status = CallParticipantStatus.Invited,
                    InviteRound = 1,
                    InviteNotifyState = 1, // Pending
                    RingExpireAt = ringExpireAt
                }, autoSave: true, cancellationToken);
            }

            await roomLifecycleRepository.InsertAsync(new CallRoomLifecycle(GuidGenerator.Create())
            {
                CallId = callId,
                RoomEpoch = 1,
                RoomName = roomName,
                ActionState = 1, // Creating
                RotationNotifyState = 0,
                CreateDeadlineAt = now.AddSeconds(CallConsts.DefaultCreateDeadlineSeconds)
            }, autoSave: true, cancellationToken);

            await idempotencyRepository.InsertAsync(new CallIdempotencyRecord(GuidGenerator.Create())
            {
                SessionUnitId = senderUnit.Id,
                DeviceId = deviceId,
                Action = "invite",
                RequestId = input.RequestId,
                CallId = callId,
                ResultCode = "OK",
                CreatedAt = now
            }, autoSave: true, cancellationToken);

            await uow.CompleteAsync(cancellationToken);
        }

        // Explicit LiveKit room creation (auto_create: false)
        try
        {
            var maxP = input.CallMode == CallMode.Direct ? 2 : Options.MaxGroupParticipants;
            var lkRoom = await liveKitClient.CreateRoomAsync(roomName, 30, maxP, cancellationToken);

            var shouldCleanupLate = false;
            using (var uow2 = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var rec = await callRecordRepository.GetAsync(callId, cancellationToken: cancellationToken);
                var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == callId && x.RoomEpoch == 1, cancellationToken: cancellationToken);
                if (rec.State == CallSessionState.Ended || rec.RoomEpoch != 1)
                {
                    shouldCleanupLate = true;
                    if (lc != null)
                    {
                        lc.ActionState = 3; // Closing
                        lc.LiveKitRoomSid = lkRoom?.Sid;
                        lc.CloseReason = "LateCreateCleanup";
                    }
                }
                else
                {
                    rec.LiveKitRoomSid = lkRoom?.Sid;
                    if (lc != null)
                    {
                        lc.ActionState = 2; // Active
                        lc.LiveKitRoomSid = lkRoom?.Sid;
                    }
                }
                await uow2.CompleteAsync(cancellationToken);
            }

            if (shouldCleanupLate)
            {
                await liveKitClient.DeleteRoomAsync(roomName, cancellationToken);
                throw new BusinessException(CallErrorCodes.CallEnded);
            }
        }
        catch (BusinessException) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LiveKit CreateRoom failed for call {CallId}", callId);
            await SettleCallEndedInTransactionAsync(callId, CallEndReason.NetworkError, cancellationToken);
            throw new BusinessException(CallErrorCodes.RoomCreateFailed);
        }

        await DispatchPendingInvitesAndBannerAsync(callId, cancellationToken);
        return await ReissueJoinResultAsync(callId, senderUnit.Id, deviceId, deviceSessionId, cancellationToken);
    }

    private async Task<CallJoinResultDto> InviteMoreAsync(
        CallInviteInput input,
        SessionUnitCacheItem senderUnit,
        string deviceId,
        string deviceSessionId,
        CancellationToken cancellationToken)
    {
        var callId = input.CallId!.Value;
        var now = clock.Now;
        var ringExpireAt = now.AddSeconds(CallConsts.DefaultRingTimeoutSeconds);

        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = await callRecordRepository.GetAsync(callId, cancellationToken: cancellationToken);
            if (record.CallMode != CallMode.Group || record.State == CallSessionState.Ended)
                throw new BusinessException(CallErrorCodes.CallEnded);

            var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == callId && x.RoomEpoch == record.RoomEpoch, cancellationToken: cancellationToken);
            if (lc == null || lc.ActionState != 2)
                throw new BusinessException(CallErrorCodes.RoomNotReady);

            var existingParticipants = await participantRepository.GetListAsync(x => x.CallId == callId, cancellationToken: cancellationToken);
            var occupiedCount = existingParticipants.Count(x => x.IsActiveBusy);

            foreach (var targetId in input.TargetSessionUnitIds.Distinct())
            {
                var existing = existingParticipants.FirstOrDefault(x => x.SessionUnitId == targetId);
                if (existing != null && existing.IsActiveBusy) continue;

                if (occupiedCount + 1 > Options.MaxGroupParticipants)
                    throw new BusinessException(CallErrorCodes.RoomFull);

                var targetUnit = await sessionUnitManager.GetCacheAsync(targetId)
                    ?? throw new BusinessException(CallErrorCodes.NotSessionMember);
                if (targetUnit.SessionId != record.SessionId || !targetUnit.IsEnabled)
                    throw new BusinessException(CallErrorCodes.NotSessionMember);

                if (existing != null)
                {
                    existing.IsActiveBusy = true;
                    existing.Status = CallParticipantStatus.Invited;
                    existing.InviteRound += 1;
                    existing.InviteNotifyState = 1;
                    existing.RingExpireAt = ringExpireAt;
                    existing.DeviceId = null;
                    existing.DeviceSessionId = null;
                    existing.LiveKitParticipantSid = null;
                }
                else
                {
                    await participantRepository.InsertAsync(new CallParticipant(GuidGenerator.Create())
                    {
                        CallId = callId,
                        SessionUnitId = targetUnit.Id,
                        OwnerId = targetUnit.OwnerId,
                        IsActiveBusy = true,
                        Status = CallParticipantStatus.Invited,
                        InviteRound = 1,
                        InviteNotifyState = 1,
                        RingExpireAt = ringExpireAt
                    }, autoSave: true, cancellationToken);
                }
                occupiedCount++;
            }

            record.StateVersion += 1;
            record.BannerNotifyState = 1;
            await uow.CompleteAsync(cancellationToken);
        }

        await DispatchPendingInvitesAndBannerAsync(callId, cancellationToken);
        return await ReissueJoinResultAsync(callId, senderUnit.Id, deviceId, deviceSessionId, cancellationToken);
    }

    public async Task<CallJoinResultDto> AcceptAsync(
        CallAcceptInput input,
        string deviceId,
        string deviceSessionId,
        CancellationToken cancellationToken = default)
    {
        var existingIdem = await idempotencyRepository.FindAsync(
            x => x.SessionUnitId == input.SessionUnitId && x.DeviceId == deviceId && x.Action == "accept" && x.RequestId == input.RequestId,
            cancellationToken: cancellationToken);
        if (existingIdem != null)
            return await ReissueJoinResultAsync(existingIdem.CallId, input.SessionUnitId, deviceId, deviceSessionId, cancellationToken);

        var now = clock.Now;
        var connectExpireAt = now.AddSeconds(CallConsts.DefaultConnectTimeoutSeconds);
        CallRecord record;

        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            record = await callRecordRepository.GetAsync(input.CallId, cancellationToken: cancellationToken);
            if (record.State == CallSessionState.Ended)
                throw new BusinessException(CallErrorCodes.CallEnded);

            var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == record.Id && x.RoomEpoch == record.RoomEpoch, cancellationToken: cancellationToken);
            if (lc == null || lc.ActionState != 2)
                throw new BusinessException(CallErrorCodes.RoomNotReady);

            var participant = await participantRepository.FindAsync(
                x => x.CallId == input.CallId && x.SessionUnitId == input.SessionUnitId,
                cancellationToken: cancellationToken);

            if (participant == null
                || participant.Status != CallParticipantStatus.Invited
                || participant.InviteRound != input.InviteRound
                || (participant.RingExpireAt.HasValue && participant.RingExpireAt.Value <= now))
            {
                throw new BusinessException(CallErrorCodes.AlreadyHandled);
            }

            if (record.CallMode == CallMode.Direct)
            {
                if (record.State != CallSessionState.Ringing || record.RingExpireAt <= now)
                    throw new BusinessException(CallErrorCodes.AlreadyHandled);
                record.State = CallSessionState.Connecting;
            }

            participant.Status = CallParticipantStatus.Accepted;
            participant.IsActiveBusy = true;
            participant.DeviceId = deviceId;
            participant.DeviceSessionId = deviceSessionId;
            participant.ConnectExpireAt = connectExpireAt;
            participant.AcceptedAt = now;

            record.StateVersion += 1;
            record.NextCheckAt = record.NextCheckAt.HasValue && record.NextCheckAt.Value < connectExpireAt
                ? record.NextCheckAt.Value
                : connectExpireAt;

            await idempotencyRepository.InsertAsync(new CallIdempotencyRecord(GuidGenerator.Create())
            {
                SessionUnitId = input.SessionUnitId,
                DeviceId = deviceId,
                Action = "accept",
                RequestId = input.RequestId,
                CallId = record.Id,
                ResultCode = "OK",
                CreatedAt = now
            }, autoSave: true, cancellationToken);

            await uow.CompleteAsync(cancellationToken);
        }

        // Notify other devices of same owner to stop ringing, and notify caller
        await distributedEventBus.PublishAsync(new SendCallSignalToClientDistributedEto
        {
            Command = CommandConsts.CallHandledElsewhere,
            SessionId = record.SessionId,
            TargetOwnerIds = [ (await sessionUnitManager.GetCacheAsync(input.SessionUnitId)).OwnerId ],
            ExcludeDeviceIds = [deviceId],
            Payload = new CallSignalPayload
            {
                CallId = record.Id.ToString("D"),
                SessionId = record.SessionId,
                StateVersion = record.StateVersion.ToString(),
                InviteRound = input.InviteRound,
                CallMode = (int)record.CallMode,
                MediaType = (int)record.MediaType,
                State = (int)record.State,
                RoomEpoch = record.RoomEpoch,
                ActorUnitId = input.SessionUnitId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        });

        return await ReissueJoinResultAsync(record.Id, input.SessionUnitId, deviceId, deviceSessionId, cancellationToken);
    }

    public async Task RejectAsync(
        CallRejectInput input,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var now = clock.Now;
        bool shouldSettleDirect = false;

        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = await callRecordRepository.GetAsync(input.CallId, cancellationToken: cancellationToken);
            if (record.State == CallSessionState.Ended) return;

            var participant = await participantRepository.FindAsync(
                x => x.CallId == input.CallId && x.SessionUnitId == input.SessionUnitId,
                cancellationToken: cancellationToken);

            if (participant == null
                || participant.Status != CallParticipantStatus.Invited
                || participant.InviteRound != input.InviteRound
                || (participant.RingExpireAt.HasValue && participant.RingExpireAt.Value <= now))
            {
                throw new BusinessException(CallErrorCodes.AlreadyHandled);
            }

            participant.Status = CallParticipantStatus.Rejected;
            participant.IsActiveBusy = false;
            participant.LeftAt = now;
            record.StateVersion += 1;

            if (record.CallMode == CallMode.Direct)
                shouldSettleDirect = true;

            await uow.CompleteAsync(cancellationToken);
        }

        if (shouldSettleDirect)
        {
            var reason = string.Equals(input.Reason, "busy", StringComparison.OrdinalIgnoreCase)
                ? CallEndReason.Busy
                : CallEndReason.Rejected;
            await SettleCallEndedInTransactionAsync(input.CallId, reason, cancellationToken);
        }
    }

    public async Task<CallJoinResultDto> JoinAsync(
        CallActionInput input,
        string deviceId,
        string deviceSessionId,
        CancellationToken cancellationToken = default)
    {
        var existingIdem = await idempotencyRepository.FindAsync(
            x => x.SessionUnitId == input.SessionUnitId && x.DeviceId == deviceId && x.Action == "join" && x.RequestId == input.RequestId,
            cancellationToken: cancellationToken);
        if (existingIdem != null)
            return await ReissueJoinResultAsync(existingIdem.CallId, input.SessionUnitId, deviceId, deviceSessionId, cancellationToken);

        var unit = await sessionUnitManager.GetCacheAsync(input.SessionUnitId)
            ?? throw new BusinessException(CallErrorCodes.NotSessionMember);

        var now = clock.Now;
        var connectExpireAt = now.AddSeconds(CallConsts.DefaultConnectTimeoutSeconds);

        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = await callRecordRepository.GetAsync(input.CallId, cancellationToken: cancellationToken);
            if (record.CallMode != CallMode.Group || record.State == CallSessionState.Ended)
                throw new BusinessException(CallErrorCodes.CallEnded);
            if (unit.SessionId != record.SessionId || !unit.IsEnabled)
                throw new BusinessException(CallErrorCodes.NotSessionMember);

            var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == record.Id && x.RoomEpoch == record.RoomEpoch, cancellationToken: cancellationToken);
            if (lc == null || lc.ActionState != 2)
                throw new BusinessException(CallErrorCodes.RoomNotReady);

            var participant = await participantRepository.FindAsync(
                x => x.CallId == input.CallId && x.SessionUnitId == input.SessionUnitId,
                cancellationToken: cancellationToken);

            if (participant != null && participant.IsActiveBusy)
            {
                if (participant.DeviceId != deviceId)
                    throw new BusinessException(CallErrorCodes.AlreadyBusy);
            }
            else
            {
                var occupied = await participantRepository.CountAsync(x => x.CallId == input.CallId && x.IsActiveBusy, cancellationToken: cancellationToken);
                if (occupied + 1 > Options.MaxGroupParticipants)
                    throw new BusinessException(CallErrorCodes.RoomFull);

                if (participant != null)
                {
                    participant.IsActiveBusy = true;
                    participant.Status = CallParticipantStatus.Accepted;
                    participant.DeviceId = deviceId;
                    participant.DeviceSessionId = deviceSessionId;
                    participant.ConnectExpireAt = connectExpireAt;
                    participant.AcceptedAt = now;
                }
                else
                {
                    await participantRepository.InsertAsync(new CallParticipant(GuidGenerator.Create())
                    {
                        CallId = record.Id,
                        SessionUnitId = unit.Id,
                        OwnerId = unit.OwnerId,
                        IsActiveBusy = true,
                        Status = CallParticipantStatus.Accepted,
                        DeviceId = deviceId,
                        DeviceSessionId = deviceSessionId,
                        ConnectExpireAt = connectExpireAt,
                        AcceptedAt = now
                    }, autoSave: true, cancellationToken);
                }
            }

            record.StateVersion += 1;
            await idempotencyRepository.InsertAsync(new CallIdempotencyRecord(GuidGenerator.Create())
            {
                SessionUnitId = input.SessionUnitId,
                DeviceId = deviceId,
                Action = "join",
                RequestId = input.RequestId,
                CallId = record.Id,
                ResultCode = "OK",
                CreatedAt = now
            }, autoSave: true, cancellationToken);

            await uow.CompleteAsync(cancellationToken);
        }

        return await ReissueJoinResultAsync(input.CallId, input.SessionUnitId, deviceId, deviceSessionId, cancellationToken);
    }

    public async Task CancelAsync(
        CallActionInput input,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        bool shouldSettle = false;
        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = await callRecordRepository.GetAsync(input.CallId, cancellationToken: cancellationToken);
            if (record.State == CallSessionState.Ended) return;
            if (record.InitiatorUnitId != input.SessionUnitId)
                throw new BusinessException(CallErrorCodes.InvalidStateTransition);

            if (record.CallMode == CallMode.Direct && record.State == CallSessionState.Ringing)
                shouldSettle = true;
            else if (record.CallMode == CallMode.Group)
            {
                var othersJoined = await participantRepository.CountAsync(
                    x => x.CallId == input.CallId && x.SessionUnitId != input.SessionUnitId
                        && (x.Status == CallParticipantStatus.Accepted || x.Status == CallParticipantStatus.Connecting || x.Status == CallParticipantStatus.Connected),
                    cancellationToken: cancellationToken);
                if (othersJoined == 0) shouldSettle = true;
            }
            await uow.CompleteAsync(cancellationToken);
        }

        if (shouldSettle)
            await SettleCallEndedInTransactionAsync(input.CallId, CallEndReason.Canceled, cancellationToken);
    }

    public async Task<ActiveCallSnapshotDto> GetActiveAsync(
        Guid currentUserId,
        Guid? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var snapshot = new ActiveCallSnapshotDto
        {
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var chatObjectManager = LazyServiceProvider.LazyGetRequiredService<IczpNet.Chat.ChatObjects.IChatObjectManager>();
        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(currentUserId);
        if (ownerIds != null && ownerIds.Count > 0)
        {
            var myActiveParticipants = await participantRepository.GetListAsync(
                x => ownerIds.Contains(x.OwnerId) && x.IsActiveBusy,
                cancellationToken: cancellationToken);

            foreach (var p in myActiveParticipants)
            {
                var record = await callRecordRepository.FindAsync(p.CallId, cancellationToken: cancellationToken);
                if (record == null || record.State == CallSessionState.Ended)
                    continue;

                if (p.Status == CallParticipantStatus.Invited && (!p.RingExpireAt.HasValue || p.RingExpireAt.Value > clock.Now))
                {
                    snapshot.IncomingCalls ??= [];
                    snapshot.IncomingCalls.Add(new IncomingCallDto
                    {
                        CallId = record.Id.ToString("D"),
                        SessionId = record.SessionId,
                        CallMode = (int)record.CallMode,
                        MediaType = (int)record.MediaType,
                        StateVersion = record.StateVersion.ToString(),
                        InviteRound = p.InviteRound
                    });
                }
                else if (p.Status is CallParticipantStatus.Accepted
                    or CallParticipantStatus.Connecting
                    or CallParticipantStatus.Connected
                    or CallParticipantStatus.Reconnecting)
                {
                    snapshot.CurrentCall ??= new CurrentCallDto
                    {
                        CallId = record.Id.ToString("D"),
                        SessionId = record.SessionId,
                        CallMode = (int)record.CallMode,
                        MediaType = (int)record.MediaType,
                        StateVersion = record.StateVersion.ToString(),
                        RoomEpoch = record.RoomEpoch
                    };
                }
            }
        }

        if (sessionId.HasValue)
        {
            var activeGroup = await callRecordRepository.FindAsync(
                x => x.SessionId == sessionId.Value && x.CallMode == CallMode.Group && x.State != CallSessionState.Ended,
                cancellationToken: cancellationToken);
            if (activeGroup != null)
            {
                var activeCount = await participantRepository.CountAsync(
                    x => x.CallId == activeGroup.Id && x.IsActiveBusy,
                    cancellationToken: cancellationToken);
                snapshot.SessionGroupCall = new SessionGroupCallDto
                {
                    CallId = activeGroup.Id.ToString("D"),
                    SessionId = activeGroup.SessionId,
                    StateVersion = activeGroup.StateVersion.ToString(),
                    MediaType = (int)activeGroup.MediaType,
                    ActiveCount = activeCount
                };
            }
        }

        return snapshot;
    }

    public async Task HangupAsync(
        CallActionInput input,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        bool settleAll = false;
        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = await callRecordRepository.GetAsync(input.CallId, cancellationToken: cancellationToken);
            if (record.State == CallSessionState.Ended) return;

            var participant = await participantRepository.FindAsync(
                x => x.CallId == input.CallId && x.SessionUnitId == input.SessionUnitId,
                cancellationToken: cancellationToken);

            if (participant != null)
            {
                participant.Status = CallParticipantStatus.Left;
                participant.IsActiveBusy = false;
                participant.LeftAt = clock.Now;
            }

            if (record.CallMode == CallMode.Direct)
            {
                settleAll = true;
            }
            else
            {
                var remainingBusy = await participantRepository.CountAsync(
                    x => x.CallId == input.CallId && x.IsActiveBusy && x.SessionUnitId != input.SessionUnitId,
                    cancellationToken: cancellationToken);
                if (remainingBusy == 0) settleAll = true;
                else record.StateVersion += 1;
            }
            await uow.CompleteAsync(cancellationToken);
        }

        if (settleAll)
            await SettleCallEndedInTransactionAsync(input.CallId, CallEndReason.Normal, cancellationToken);
    }

    public async Task<CallHeartbeatResultDto> HeartbeatAsync(
        CallHeartbeatInput input,
        string deviceId,
        string deviceSessionId,
        CancellationToken cancellationToken = default)
    {
        using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var record = await callRecordRepository.GetAsync(input.CallId, cancellationToken: cancellationToken);
        var participant = await participantRepository.FindAsync(
            x => x.CallId == input.CallId && x.SessionUnitId == input.SessionUnitId,
            cancellationToken: cancellationToken);

        if (participant == null || participant.DeviceId != deviceId || participant.DeviceSessionId != deviceSessionId)
            throw new BusinessException(CallErrorCodes.DeviceNotAuthorized);

        var now = clock.Now;
        participant.LastHeartbeatAt = now;
        record.LastActiveAt = now;
        await uow.CompleteAsync(cancellationToken);

        return new CallHeartbeatResultDto
        {
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            StateVersion = record.StateVersion.ToString(),
            RoomEpoch = record.RoomEpoch,
            State = (int)record.State
        };
    }

    public async Task<CallJoinResultDto> ReissueJoinResultAsync(
        Guid callId,
        Guid sessionUnitId,
        string deviceId,
        string deviceSessionId,
        CancellationToken cancellationToken = default)
    {
        var record = await callRecordRepository.GetAsync(callId, cancellationToken: cancellationToken);
        if (record.State == CallSessionState.Ended)
            throw new BusinessException(CallErrorCodes.CallEnded);

        var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == callId && x.RoomEpoch == record.RoomEpoch, cancellationToken: cancellationToken);
        if (lc == null || lc.ActionState != 2)
            throw new BusinessException(CallErrorCodes.RoomNotReady);

        var participant = await participantRepository.FindAsync(
            x => x.CallId == callId && x.SessionUnitId == sessionUnitId,
            cancellationToken: cancellationToken);

        if (participant == null
            || participant.DeviceId != deviceId
            || participant.DeviceSessionId != deviceSessionId
            || participant.Status is not (CallParticipantStatus.Accepted or CallParticipantStatus.Connecting or CallParticipantStatus.Connected or CallParticipantStatus.Reconnecting))
        {
            throw new BusinessException(CallErrorCodes.DeviceNotAuthorized);
        }

        var unit = await sessionUnitManager.GetCacheAsync(sessionUnitId);
        var identity = $"{participant.OwnerId}:{deviceId}";
        var metadata = JsonSerializer.Serialize(new
        {
            sessionUnitId = participant.SessionUnitId,
            chatObjectId = participant.OwnerId.ToString(),
            deviceId,
            displayName = participant.OwnerId.ToString()
        });

        var token = liveKitClient.GenerateParticipantToken(record.RoomName, identity, metadata, CallConsts.DefaultTokenTtlSeconds);
        return new CallJoinResultDto
        {
            CallId = record.Id.ToString("D"),
            SessionId = record.SessionId,
            StateVersion = record.StateVersion.ToString(),
            CallMode = (int)record.CallMode,
            MediaType = (int)record.MediaType,
            RoomName = record.RoomName,
            RoomEpoch = record.RoomEpoch,
            RtcUrl = Options.PublicWsUrl,
            Token = token,
            IceTransportPolicy = Options.IceTransportPolicy,
            ExpireAt = new DateTimeOffset(record.RingExpireAt, TimeSpan.Zero).ToUnixTimeMilliseconds()
        };
    }

    public async Task TryMarkParticipantConnectedAsync(
        Guid callId,
        int roomEpoch,
        string roomSid,
        Guid sessionUnitId,
        string deviceId,
        string participantSid,
        DateTime connectedAtUtc,
        CancellationToken cancellationToken = default)
    {
        using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var record = await callRecordRepository.GetAsync(callId, cancellationToken: cancellationToken);
        if (record.State == CallSessionState.Ended || record.RoomEpoch != roomEpoch || record.LiveKitRoomSid != roomSid)
            return;

        var participant = await participantRepository.FindAsync(
            x => x.CallId == callId && x.SessionUnitId == sessionUnitId,
            cancellationToken: cancellationToken);
        if (participant == null
            || participant.DeviceId != deviceId
            || participant.Status is not (CallParticipantStatus.Accepted or CallParticipantStatus.Connecting or CallParticipantStatus.Connected or CallParticipantStatus.Reconnecting))
            return;

        participant.Status = CallParticipantStatus.Connected;
        participant.IsActiveBusy = true;
        participant.LiveKitParticipantSid = participantSid;
        participant.ConnectExpireAt = null;
        participant.DisconnectDeadlineAt = null;
        participant.ConnectedAt ??= connectedAtUtc;

        var connectedCount = await participantRepository.CountAsync(
            x => x.CallId == callId && x.Status == CallParticipantStatus.Connected,
            cancellationToken: cancellationToken);
        if (connectedCount > record.MaxParticipants)
            record.MaxParticipants = connectedCount;

        if (record.State != CallSessionState.Connected)
        {
            if (record.CallMode == CallMode.Direct && connectedCount >= 2)
            {
                record.State = CallSessionState.Connected;
                record.ConnectedAt ??= connectedAtUtc;
            }
            else if (record.CallMode == CallMode.Group && connectedCount >= 2)
            {
                record.State = CallSessionState.Connected;
                record.ConnectedAt ??= connectedAtUtc;
            }
        }

        await uow.CompleteAsync(cancellationToken);
    }

    public async Task DispatchPendingInvitesAndBannerAsync(Guid callId, CancellationToken cancellationToken = default)
    {
        var record = await callRecordRepository.FindAsync(callId, cancellationToken: cancellationToken);
        if (record == null || record.State == CallSessionState.Ended) return;

        var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == callId && x.RoomEpoch == record.RoomEpoch, cancellationToken: cancellationToken);
        if (lc == null || lc.ActionState != 2) return;

        var pendingInvites = await participantRepository.GetListAsync(
            x => x.CallId == callId && x.Status == CallParticipantStatus.Invited && x.InviteNotifyState == 1,
            cancellationToken: cancellationToken);

        foreach (var group in pendingInvites.GroupBy(x => x.InviteRound))
        {
            await distributedEventBus.PublishAsync(new SendCallSignalToClientDistributedEto
            {
                Command = CommandConsts.CallInvited,
                SessionId = record.SessionId,
                TargetSessionUnitIds = group.Select(x => x.SessionUnitId).ToList(),
                ExcludeDeviceIds = string.IsNullOrWhiteSpace(record.InitiatorDeviceId) ? [] : [record.InitiatorDeviceId],
                Payload = new CallSignalPayload
                {
                    CallId = record.Id.ToString("D"),
                    SessionId = record.SessionId,
                    StateVersion = record.StateVersion.ToString(),
                    CallMode = (int)record.CallMode,
                    MediaType = (int)record.MediaType,
                    State = (int)record.State,
                    RoomEpoch = record.RoomEpoch,
                    InviteRound = group.Key,
                    InitiatorUnitId = record.InitiatorUnitId,
                    TargetUnitIds = group.Select(x => x.SessionUnitId).ToList(),
                    ExpireAt = new DateTimeOffset(record.RingExpireAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            });
        }

        if (pendingInvites.Count > 0 || record.BannerNotifyState == 1)
        {
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            foreach (var p in pendingInvites) p.InviteNotifyState = 2;
            record.BannerNotifyState = 2;
            await uow.CompleteAsync(cancellationToken);
        }
    }

    public async Task SettleCallEndedInTransactionAsync(Guid callId, CallEndReason reason, CancellationToken cancellationToken = default)
    {
        var now = clock.Now;
        string roomToClose = null;

        using (var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            var record = await callRecordRepository.GetAsync(callId, cancellationToken: cancellationToken);
            if (record.State == CallSessionState.Ended) return;

            record.State = CallSessionState.Ended;
            record.EndReason = reason;
            record.EndedAt = now;
            record.StateVersion += 1;
            record.DurationSeconds = record.ConnectedAt.HasValue
                ? Math.Max(0, (int)(now - record.ConnectedAt.Value).TotalSeconds)
                : 0;
            record.SummaryDispatchStatus = 1; // Pending Outbox

            var participants = await participantRepository.GetListAsync(
                x => x.CallId == callId && x.IsActiveBusy,
                cancellationToken: cancellationToken);
            foreach (var p in participants)
            {
                p.IsActiveBusy = false;
                p.LeftAt = now;
                if (p.Status == CallParticipantStatus.Invited)
                    p.Status = reason == CallEndReason.Timeout ? CallParticipantStatus.Timeout : CallParticipantStatus.Left;
                else
                    p.Status = CallParticipantStatus.Left;
            }

            var lifecycles = await roomLifecycleRepository.GetListAsync(
                x => x.CallId == callId && (x.ActionState == 1 || x.ActionState == 2),
                cancellationToken: cancellationToken);
            foreach (var lc in lifecycles)
            {
                lc.ActionState = 3; // Closing
                lc.CloseReason = "CallEnded";
                lc.NextRetryAt = now;
                roomToClose ??= lc.RoomName;
            }

            await uow.CompleteAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(roomToClose))
        {
            try
            {
                await liveKitClient.DeleteRoomAsync(roomToClose, cancellationToken);
                using var uow2 = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
                var lc = await roomLifecycleRepository.FindAsync(x => x.CallId == callId && x.RoomName == roomToClose, cancellationToken: cancellationToken);
                if (lc != null) { lc.ActionState = 4; lc.ClosedAt = clock.Now; }
                await uow2.CompleteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Deferred DeleteRoom for call {CallId}, Worker will retry", callId);
            }
        }
    }
}
