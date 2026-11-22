using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class CreateSessionAnnouncementInput
{
    [Required]
    public virtual Guid SessionUnitId { get; set; }

    public virtual Guid? SessionId { get; set; }

    [Required]
    [StringLength(128)]
    public virtual string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public virtual string Content { get; set; }

    public virtual EditorTypes EditorType { get; set; } = EditorTypes.None;

    public virtual bool IsPinned { get; set; } = true;

    public virtual bool IsPopup { get; set; } = false;

    public virtual bool RequireConfirm { get; set; } = false;

    /// <summary>
    /// 是否在聊天消息流中发送系统通知卡片
    /// </summary>
    public virtual bool SendMessageNotification { get; set; } = true;
}
