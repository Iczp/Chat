using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class ReviewSessionAnnouncementInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// true=审核通过，false=驳回
    /// </summary>
    public virtual bool Approved { get; set; }

    /// <summary>
    /// 审核意见或驳回原因
    /// </summary>
    [StringLength(500)]
    public virtual string Remark { get; set; }

    /// <summary>
    /// 审核通过并立即发布时是否在群内发送通知消息
    /// </summary>
    public virtual bool SendMessageNotification { get; set; } = true;
}
