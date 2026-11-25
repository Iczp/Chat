using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 群聊邀请策略
/// </summary>
[Description("群邀请策略")]
public enum RoomInvitePolicies
{
    /// <summary>
    /// 所有成员均可直接邀请
    /// </summary>
    [Description("所有人可邀请")]
    AllMembers = 0,

    /// <summary>
    /// 仅群主和管理员可邀请
    /// </summary>
    [Description("仅管理员可邀请")]
    AdminsOnly = 1,

    /// <summary>
    /// 成员邀请需管理员确认
    /// </summary>
    [Description("邀请需管理确认")]
    ApprovalRequired = 2
}
