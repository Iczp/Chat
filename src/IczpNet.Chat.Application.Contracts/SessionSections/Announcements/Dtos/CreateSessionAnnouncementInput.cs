using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class CreateSessionAnnouncementInput
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

    public virtual bool IsPinned { get; set; } = true;

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
    public virtual bool ApplyToNewMembers { get; set; } = false;

    /// <summary>
    /// 定时发布时间（null=立即发布）
    /// </summary>
    public virtual DateTime? PublishTime { get; set; }

    /// <summary>
    /// 是否保存为草稿（不立即发布）
    /// </summary>
    public virtual bool SaveAsDraft { get; set; } = false;

    /// <summary>
    /// 是否提交审核（无直接发布权时自动提交审核）
    /// </summary>
    public virtual bool SubmitForReview { get; set; } = false;

    public virtual bool IsPopup { get; set; } = false;

    public virtual bool RequireConfirm { get; set; } = false;

    /// <summary>
    /// 是否在聊天消息流中发送系统通知卡片
    /// </summary>
    public virtual bool SendMessageNotification { get; set; } = true;
}
