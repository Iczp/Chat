using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Calls;

public interface ICallAppService : IApplicationService
{
    Task<CallJoinResultDto> InviteAsync(CallInviteInput input);
    Task<CallJoinResultDto> AcceptAsync(CallAcceptInput input);
    Task<CallJoinResultDto> JoinAsync(CallActionInput input);
    Task RejectAsync(CallRejectInput input);
    Task CancelAsync(CallActionInput input);
    Task HangupAsync(CallActionInput input);
    Task<CallHeartbeatResultDto> HeartbeatAsync(CallHeartbeatInput input);
    Task<CallJoinResultDto> RefreshTokenAsync(CallHeartbeatInput input);
    Task<ActiveCallSnapshotDto> GetActiveAsync(Guid? sessionId = null);
}
