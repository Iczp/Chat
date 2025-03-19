using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 消息命令
/// </summary>
[Description("性别")]
public enum Command
{
    /// <summary>
    /// 新消息
    /// </summary>
    [Description("新消息")]
    Created = 0,

    /// <summary>
    /// 更新消息
    /// </summary>
    [Description("更新消息")]
    Updated = 1,

    /// <summary>
    /// 转发消息
    /// </summary>
    [Description("转发消息")]
    Forward = 2,

    /// <summary>
    /// 撤回消息
    /// </summary>
    [Description("撤回消息")]
    Rollback = 3,

    /// <summary>
    /// 删除消息
    /// </summary>
    [Description("删除消息")]
    Deleted = 4,

    /// <summary>
    /// 更新角标
    /// </summary>
    [Description("更新角标")]
    UpdateBadge = 5,
}
