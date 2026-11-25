using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 公告状态
/// </summary>
[Description("公告状态")]
public enum AnnouncementStatus : int
{
    /// <summary>
    /// 草稿
    /// </summary>
    [Description("草稿")]
    Draft = 0,

    /// <summary>
    /// 已发布
    /// </summary>
    [Description("已发布")]
    Published = 1,

    /// <summary>
    /// 已撤下/归档
    /// </summary>
    [Description("已撤下")]
    Revoked = 2,

    /// <summary>
    /// 待审核
    /// </summary>
    [Description("待审核")]
    PendingReview = 3,

    /// <summary>
    /// 已驳回
    /// </summary>
    [Description("已驳回")]
    Rejected = 4,
}
