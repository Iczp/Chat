using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Calls;

public class CallCompensationWorker : AsyncPeriodicBackgroundWorkerBase
{
    public CallCompensationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory) : base(timer, serviceScopeFactory)
    {
        Timer.Period = 5000; // 5s 周期
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var callRecordRepo = workerContext.ServiceProvider.GetRequiredService<IRepository<CallRecord, Guid>>();
        var participantRepo = workerContext.ServiceProvider.GetRequiredService<IRepository<CallParticipant, Guid>>();
        var lifecycleRepo = workerContext.ServiceProvider.GetRequiredService<IRepository<CallRoomLifecycle, Guid>>();
        var liveKitClient = workerContext.ServiceProvider.GetRequiredService<ILiveKitClient>();
        var callManager = workerContext.ServiceProvider.GetRequiredService<CallSessionManager>();
        var uowManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        var utcNow = DateTime.UtcNow;

        // 1. 关房重试 (ActionState == Closing)
        var closingRooms = await lifecycleRepo.GetListAsync(x => x.ActionState == 3 && (x.NextRetryAt == null || x.NextRetryAt <= utcNow));
        foreach (var lc in closingRooms.Take(20))
        {
            try
            {
                await liveKitClient.DeleteRoomAsync(lc.RoomName);
                using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
                var target = await lifecycleRepo.GetAsync(lc.Id);
                target.ActionState = 4; // Closed
                target.ClosedAt = DateTime.UtcNow;
                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "CallCompensationWorker DeleteRoom retry failed for {RoomName}", lc.RoomName);
            }
        }

        // 2. 建连/断线宽限超时检查 (仅当 ParticipantInfo.State == 2 [ACTIVE] 时才补记 Connected)
        var activeCalls = await callRecordRepo.GetListAsync(x => x.State != Enums.CallSessionState.Ended && x.NextCheckAt != null && x.NextCheckAt <= utcNow);
        foreach (var call in activeCalls.Take(20))
        {
            var lkParticipants = await liveKitClient.ListParticipantsAsync(call.RoomName);
            var participants = await participantRepo.GetListAsync(x => x.CallId == call.Id && x.IsActiveBusy);

            foreach (var p in participants)
            {
                if (p.Status is Enums.CallParticipantStatus.Accepted or Enums.CallParticipantStatus.Connecting or Enums.CallParticipantStatus.Reconnecting)
                {
                    var matched = lkParticipants.FirstOrDefault(lp =>
                        !string.IsNullOrWhiteSpace(p.DeviceId)
                        && lp.Identity == $"{p.OwnerId}:{p.DeviceId}"
                        && lp.State == 2); // 2 = ACTIVE（严格排除 JOINING=0 / JOINED=1）

                    if (matched != null)
                    {
                        await callManager.TryMarkParticipantConnectedAsync(
                            call.Id, call.RoomEpoch, call.LiveKitRoomSid, p.SessionUnitId, p.DeviceId, matched.Sid, utcNow);
                        continue;
                    }
                }

                if ((p.Status == Enums.CallParticipantStatus.Invited && p.RingExpireAt <= utcNow)
                    || (p.Status is Enums.CallParticipantStatus.Accepted or Enums.CallParticipantStatus.Connecting && p.ConnectExpireAt <= utcNow)
                    || (p.Status == Enums.CallParticipantStatus.Reconnecting && p.DisconnectDeadlineAt <= utcNow))
                {
                    if (call.CallMode == Enums.CallMode.Direct)
                    {
                        var reason = p.Status == Enums.CallParticipantStatus.Invited
                            ? Enums.CallEndReason.Timeout
                            : Enums.CallEndReason.NetworkError;
                        await callManager.SettleCallEndedInTransactionAsync(call.Id, reason);
                        break;
                    }
                }
            }
        }
    }
}
