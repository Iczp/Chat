using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 二维码场景类型
/// </summary>
[Description("二维码场景类型")]
public enum QrCodeScenes
{
    /// <summary>
    /// 聊天对象名片（个人/掌柜/店小二/服务号/订阅号/广场/机器人等）
    /// </summary>
    [Description("聊天对象名片")]
    ChatObjectCard = 1,

    /// <summary>
    /// 群聊邀请二维码
    /// </summary>
    [Description("群聊邀请")]
    GroupInvite = 2,

    /// <summary>
    /// 工作台应用分享码
    /// </summary>
    [Description("工作台应用")]
    WorkbenchApp = 3,

    /// <summary>
    /// 通用自定义业务载荷
    /// </summary>
    [Description("自定义业务")]
    CustomPayload = 99,
}
