using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 入群邀请方式（权限）  0:群成员可以邀请/扫码, 1:群管理员, 2:群主
/// </summary>
[Description("入群邀请方式")]
public enum InvitationMethods
{
    /// <summary>
    /// 群成员可以邀请/扫码
    /// </summary>
    [Description("群成员可以邀请/扫码")]
    Member = 0,
    /// <summary>
    /// 群管理员
    /// </summary>
    [Description("群管理员")]
    Manager = 1,
    /// <summary>
    /// 群主
    /// </summary>
    [Description("群主")]
    Creator = 2,
}
