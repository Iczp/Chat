using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.Calls;

public class CallRecord : Entity<Guid>
{
    public Guid SessionId { get; set; }
    public CallMode CallMode { get; set; }
    public CallMediaType MediaType { get; set; }
    public CallSessionState State { get; set; }
    public long StateVersion { get; set; }
    public Guid InitiatorUnitId { get; set; }
    public long InitiatorOwnerId { get; set; }
    [MaxLength(128)]
    public string InitiatorDeviceId { get; set; }
    public int RoomEpoch { get; set; } = 1;
    [MaxLength(128)]
    public string RoomName { get; set; }
    [MaxLength(64)]
    public string LiveKitRoomSid { get; set; }
    public byte BannerNotifyState { get; set; }
    public DateTime RingExpireAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public DateTime? NextCheckAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public CallEndReason? EndReason { get; set; }
    public int DurationSeconds { get; set; }
    public bool HadScreenShare { get; set; }
    public int MaxParticipants { get; set; }
    [Required, MaxLength(26)]
    public string SummaryClientMessageId { get; set; }
    public long? SummaryMessageId { get; set; }
    public byte SummaryDispatchStatus { get; set; }

    protected CallRecord() { }

    public CallRecord(Guid id) : base(id) { }
}

public class CallParticipant : Entity<Guid>
{
    public Guid CallId { get; set; }
    public Guid SessionUnitId { get; set; }
    public long OwnerId { get; set; }
    public bool IsActiveBusy { get; set; }
    public CallParticipantStatus Status { get; set; }
    public int InviteRound { get; set; }
    public byte InviteNotifyState { get; set; }
    [MaxLength(128)]
    public string DeviceId { get; set; }
    [MaxLength(128)]
    public string DeviceSessionId { get; set; }
    [MaxLength(64)]
    public string LiveKitParticipantSid { get; set; }
    public DateTime? RingExpireAt { get; set; }
    public DateTime? ConnectExpireAt { get; set; }
    public DateTime? DisconnectDeadlineAt { get; set; }
    public DateTime? LastHeartbeatAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LeftAt { get; set; }

    protected CallParticipant() { }

    public CallParticipant(Guid id) : base(id) { }
}

public class CallRoomLifecycle : Entity<Guid>
{
    public Guid CallId { get; set; }
    public int RoomEpoch { get; set; }
    [Required, MaxLength(128)]
    public string RoomName { get; set; }
    [MaxLength(64)]
    public string LiveKitRoomSid { get; set; }
    public byte ActionState { get; set; } // 1=Creating, 2=Active, 3=Closing, 4=Closed, 5=CreateFailed
    public byte RotationNotifyState { get; set; } // 0=None, 1=Pending, 2=Dispatched
    [MaxLength(64)]
    public string CloseReason { get; set; }
    public int RetryCount { get; set; }
    public DateTime? CreateDeadlineAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    protected CallRoomLifecycle() { }

    public CallRoomLifecycle(Guid id) : base(id) { }
}

public class CallIdempotencyRecord : Entity<Guid>
{
    public Guid SessionUnitId { get; set; }
    [Required, MaxLength(128)]
    public string DeviceId { get; set; }
    [Required, MaxLength(32)]
    public string Action { get; set; }
    [Required, MaxLength(128)]
    public string RequestId { get; set; }
    public Guid CallId { get; set; }
    [MaxLength(64)]
    public string ResultCode { get; set; }
    public DateTime CreatedAt { get; set; }

    protected CallIdempotencyRecord() { }

    public CallIdempotencyRecord(Guid id) : base(id) { }
}

public class CallWebhookEvent : Entity<string>
{
    [MaxLength(128)]
    public string RoomName { get; set; }
    [MaxLength(64)]
    public string RoomSid { get; set; }
    [MaxLength(64)]
    public string EventType { get; set; }
    public DateTime ProcessedAt { get; set; }

    protected CallWebhookEvent() { }

    public CallWebhookEvent(string id) : base(id) { }
}
