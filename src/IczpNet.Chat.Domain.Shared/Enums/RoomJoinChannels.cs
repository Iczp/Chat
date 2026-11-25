using System;
using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 允许的入群渠道（位掩码）
/// </summary>
[Flags]
[Description("允许的入群渠道")]
public enum RoomJoinChannels
{
    [Description("禁止入群")]
    None = 0,

    [Description("成员邀请")]
    Invitation = 1,

    [Description("扫描二维码")]
    QrCode = 2,

    [Description("搜索群号")]
    Search = 4,

    [Description("全部方式")]
    All = Invitation | QrCode | Search
}
