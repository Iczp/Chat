using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Calls;

[Serializable]
public class CallContentInfo
{
    public string CallId { get; set; }
    public int CallMode { get; set; }
    public int MediaType { get; set; }
    public int EndReason { get; set; }
    public int DurationSeconds { get; set; }
    public Guid InitiatorUnitId { get; set; }
    public List<Guid> ParticipantUnitIds { get; set; } = [];
    public int MaxParticipants { get; set; }
    public bool HadScreenShare { get; set; }
    public long StartedAt { get; set; }
    public long EndedAt { get; set; }
}

[Serializable]
public class CallSignalPayload
{
    public string CallId { get; set; }
    public Guid SessionId { get; set; }
    public string StateVersion { get; set; }
    public int CallMode { get; set; }
    public int MediaType { get; set; }
    public int State { get; set; }
    public int RoomEpoch { get; set; }
    public int InviteRound { get; set; }
    public Guid InitiatorUnitId { get; set; }
    public string InitiatorName { get; set; }
    public string InitiatorAvatar { get; set; }
    public Guid? ActorUnitId { get; set; }
    public List<Guid> TargetUnitIds { get; set; } = [];
    public int ActiveCount { get; set; }
    public int? EndReason { get; set; }
    public int DurationSeconds { get; set; }
    public long? ExpireAt { get; set; }
    public long Timestamp { get; set; }
}

[Serializable]
public class SendCallSignalToClientDistributedEto
{
    public string Command { get; set; }
    public Guid SessionId { get; set; }
    public List<Guid> TargetSessionUnitIds { get; set; } = [];
    public List<long> TargetOwnerIds { get; set; } = [];
    public List<string> ExcludeDeviceIds { get; set; } = [];
    public CallSignalPayload Payload { get; set; }
}

public class CallInviteInput
{
    [Required]
    public string RequestId { get; set; }
    public Guid SessionId { get; set; }
    public Guid SenderSessionUnitId { get; set; }
    public CallMode CallMode { get; set; }
    public CallMediaType MediaType { get; set; }
    public List<Guid> TargetSessionUnitIds { get; set; } = [];
    public Guid? CallId { get; set; }
}

public class CallAcceptInput
{
    [Required]
    public string RequestId { get; set; }
    public Guid CallId { get; set; }
    public Guid SessionUnitId { get; set; }
    public int InviteRound { get; set; }
}

public class CallRejectInput
{
    [Required]
    public string RequestId { get; set; }
    public Guid CallId { get; set; }
    public Guid SessionUnitId { get; set; }
    public int InviteRound { get; set; }
    public string Reason { get; set; }
}

public class CallActionInput
{
    [Required]
    public string RequestId { get; set; }
    public Guid CallId { get; set; }
    public Guid SessionUnitId { get; set; }
    public string Reason { get; set; }
}

public class CallHeartbeatInput
{
    public Guid CallId { get; set; }
    public Guid SessionUnitId { get; set; }
}

public class CallJoinResultDto
{
    public string CallId { get; set; }
    public Guid SessionId { get; set; }
    public string StateVersion { get; set; }
    public int CallMode { get; set; }
    public int MediaType { get; set; }
    public string RoomName { get; set; }
    public int RoomEpoch { get; set; }
    public string RtcUrl { get; set; }
    public string Token { get; set; }
    public string IceTransportPolicy { get; set; }
    public long? ExpireAt { get; set; }
}

public class CallHeartbeatResultDto
{
    public long ServerTime { get; set; }
    public string StateVersion { get; set; }
    public int RoomEpoch { get; set; }
    public int State { get; set; }
}

public class ActiveCallSnapshotDto
{
    public long ServerTime { get; set; }
    public List<IncomingCallDto> IncomingCalls { get; set; } = [];
    public CurrentCallDto CurrentCall { get; set; }
    public SessionGroupCallDto SessionGroupCall { get; set; }
}

public class IncomingCallDto
{
    public string CallId { get; set; }
    public Guid SessionId { get; set; }
    public string StateVersion { get; set; }
    public int InviteRound { get; set; }
    public int CallMode { get; set; }
    public int MediaType { get; set; }
    public int State { get; set; }
    public Guid InitiatorUnitId { get; set; }
    public string InitiatorName { get; set; }
    public string InitiatorAvatar { get; set; }
    public Guid MySessionUnitId { get; set; }
    public long? ExpireAt { get; set; }
}

public class CurrentCallDto
{
    public string CallId { get; set; }
    public Guid SessionId { get; set; }
    public string StateVersion { get; set; }
    public int CallMode { get; set; }
    public int MediaType { get; set; }
    public int State { get; set; }
    public int RoomEpoch { get; set; }
    public Guid MySessionUnitId { get; set; }
    public string BoundDeviceId { get; set; }
    public int ActiveCount { get; set; }
}

public class SessionGroupCallDto
{
    public string CallId { get; set; }
    public Guid SessionId { get; set; }
    public string StateVersion { get; set; }
    public int MediaType { get; set; }
    public int ActiveCount { get; set; }
    public string InitiatorName { get; set; }
    public List<CallParticipantPreviewDto> ParticipantPreviews { get; set; } = [];
}

public class CallParticipantPreviewDto
{
    public Guid SessionUnitId { get; set; }
    public string DisplayName { get; set; }
    public string Avatar { get; set; }
}
