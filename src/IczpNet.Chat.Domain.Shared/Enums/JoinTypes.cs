using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 共享位置
/// </summary>
[Description("共享位置")]
public enum JoinTypes : int
{
    /// <summary>
    /// 更新位置
    /// </summary>
    [Description("更新位置")]
    Update = 0,

    /// <summary>
    /// 发起位置共享
    /// </summary>
    [Description("发起位置共享")]
    Creator = 1,

    /// <summary>
    /// 加入位置共享
    /// </summary>
    [Description("加入位置共享")] 
    Join = 2,

    /// <summary>
    /// 退出位置共享
    /// </summary>
    [Description("退出位置共享")]
    Stop = 3
}
