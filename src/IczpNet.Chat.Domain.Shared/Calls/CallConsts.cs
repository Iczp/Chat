namespace IczpNet.Chat.Calls;

public static class CallConsts
{
    public const int MaxGroupParticipants = 16;
    public const int DefaultRingTimeoutSeconds = 45;
    public const int DefaultConnectTimeoutSeconds = 20;
    public const int DefaultDisconnectGraceSeconds = 20;
    public const int DefaultCreateDeadlineSeconds = 15;
    public const int DefaultTokenTtlSeconds = 180;
    public const int DefaultHeartbeatTimeoutSeconds = 90;
}

public static class CallErrorCodes
{
    public const string DeviceNotAuthorized = "Call:DeviceNotAuthorized";
    public const string NotSessionMember = "Call:NotSessionMember";
    public const string AlreadyBusy = "Call:AlreadyBusy";
    public const string GroupCallAlreadyActive = "Call:GroupCallAlreadyActive";
    public const string RoomFull = "Call:RoomFull";
    public const string AlreadyHandled = "Call:AlreadyHandled";
    public const string CallEnded = "Call:CallEnded";
    public const string RoomNotReady = "Call:RoomNotReady";
    public const string InvalidStateTransition = "Call:InvalidStateTransition";
    public const string RoomCreateFailed = "Call:RoomCreateFailed";
}
