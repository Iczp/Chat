using System.ComponentModel;

namespace IczpNet.Chat.Enums;

[Description("通话模式")]
public enum CallMode
{
    [Description("单聊通话")]
    Direct = 1,

    [Description("群组通话")]
    Group = 2,
}

[Description("通话媒体类型")]
public enum CallMediaType
{
    [Description("语音")]
    Audio = 1,

    [Description("视频")]
    Video = 2,
}

[Description("通话会话状态")]
public enum CallSessionState
{
    [Description("振铃中")]
    Ringing = 1,

    [Description("建连中")]
    Connecting = 2,

    [Description("通话中")]
    Connected = 3,

    [Description("已结束")]
    Ended = 4,
}

[Description("通话参与者状态")]
public enum CallParticipantStatus
{
    [Description("振铃中")]
    Invited = 1,

    [Description("已接听")]
    Accepted = 2,

    [Description("建连中")]
    Connecting = 3,

    [Description("已连接")]
    Connected = 4,

    [Description("重连中")]
    Reconnecting = 5,

    [Description("已拒绝")]
    Rejected = 6,

    [Description("未接超时")]
    Timeout = 7,

    [Description("已离开")]
    Left = 8,

    [Description("已撤销")]
    Revoked = 9,
}

[Description("通话结束原因")]
public enum CallEndReason
{
    [Description("正常挂断")]
    Normal = 1,

    [Description("主叫取消")]
    Canceled = 2,

    [Description("对方拒绝")]
    Rejected = 3,

    [Description("对方忙线")]
    Busy = 4,

    [Description("无人接听")]
    Timeout = 5,

    [Description("网络异常")]
    NetworkError = 6,

    [Description("权限撤销")]
    Revoked = 7,
}
