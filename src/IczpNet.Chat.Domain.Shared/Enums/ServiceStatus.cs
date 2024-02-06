using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 客服状态
/// </summary>
[Description("客服状态")]
public enum ServiceStatus : int
{
    /// <summary>
    /// 常规
    /// </summary>
    [Description("常规")]
    Normal = 0,
    /// <summary>
    /// 在线
    /// </summary>
    [Description("在线")]
    Online = 1,
    /// <summary>
    /// 挂起
    /// </summary>
    [Description("挂起")]
    Pending = 2,
    /// <summary>
    /// 隐身
    /// </summary>
    [Description("隐身")]
    Stealth = 3,
    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")]
    Offline = 4,
}
