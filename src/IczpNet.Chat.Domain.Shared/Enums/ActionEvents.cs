using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 公众号事件
/// </summary>
[Description("公众号事件")]
public enum ActionEvents : int
{
    /// <summary>
    /// 自定义行为
    /// </summary>
    [Description("自定义行为")]
    Action = 0,
    /// <summary>
    /// 导航连接
    /// </summary>
    [Description("导航连接")]
    Navigation = 1,
    /// <summary>
    /// 本地页面
    /// </summary>
    [Description("本地页面")]
    LocalPage = 2,
    /// <summary>
    /// 应该标签
    /// </summary>
    [Description("应用标签")]
    SwitchTab = 3,
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    [Description("取消订阅事件")]
    Unsubscribe = 4,
    /// <summary>
    /// 订阅事件
    /// </summary>
    [Description("订阅事件")]
    Subscribe = 5,
    /// <summary>
    /// 扫码事件
    /// </summary>
    [Description("扫码事件")]
    Scan = 6,
    /// <summary>
    /// 进入应用
    /// </summary>
    [Description("进入应用")]
    EnterAgent = 7,
}
