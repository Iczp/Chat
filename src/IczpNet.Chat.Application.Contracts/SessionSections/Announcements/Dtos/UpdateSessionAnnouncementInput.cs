using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class UpdateSessionAnnouncementInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    [Required]
    [StringLength(128)]
    public virtual string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public virtual string Content { get; set; }

    public virtual EditorTypes EditorType { get; set; } = EditorTypes.None;

    public virtual bool IsPinned { get; set; }

    /// <summary>
    /// 置顶排序权重（0=不置顶，数值越大越置顶）
    /// </summary>
    public virtual double? Sorting { get; set; }

    /// <summary>
    /// 公告有效期截止时间（null=长期有效）
    /// </summary>
    public virtual DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 是否对发布后新入群/重入群成员生效（弹窗/需确认）
    /// </summary>
    public virtual bool ApplyToNewMembers { get; set; }

    /// <summary>
    /// 定时发布时间（null=立即发布）
    /// </summary>
    public virtual DateTime? PublishTime { get; set; }

    /// <summary>
    /// 是否保存为草稿
    /// </summary>
    public virtual bool SaveAsDraft { get; set; } = false;

    /// <summary>
    /// 是否提交审核
    /// </summary>
    public virtual bool SubmitForReview { get; set; } = false;

    /// <summary>
    /// 是否递增版本号（已发布公告修订默认可升版）
    /// </summary>
    public virtual bool BumpVersion { get; set; } = false;

    /// <summary>
    /// 本次修订摘要
    /// </summary>
    [StringLength(300)]
    public virtual string ChangeSummary { get; set; }

    /// <summary>
    /// 乐观并发校验的期望版本号
    /// </summary>
    public virtual int? ExpectedVersion { get; set; }

    /// <summary>
    /// 修改后是否重置已读/确认状态（要求成员重新确认）
    /// </summary>
    public virtual bool ResetReceipts { get; set; } = false;

    public virtual bool IsPopup { get; set; }

    public virtual bool RequireConfirm { get; set; }

    /// <summary>
    /// 是否再次在聊天消息流中发送系统通知卡片
    /// </summary>
    public virtual bool SendMessageNotification { get; set; } = false;
}
