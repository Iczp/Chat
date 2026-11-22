using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementDto : EntityDto<Guid>
{
    public virtual Guid? SessionId { get; set; }
    public virtual long ChatObjectId { get; set; }
    public virtual Guid PublisherSessionUnitId { get; set; }
    public virtual long PublisherChatObjectId { get; set; }
    public virtual string PublisherName { get; set; }
    public virtual string PublisherPortrait { get; set; }
    public virtual string Title { get; set; }
    public virtual EditorTypes EditorType { get; set; }
    public virtual AnnouncementStatus Status { get; set; }
    public virtual bool IsPinned { get; set; }
    public virtual DateTime? PinnedTime { get; set; }
    public virtual bool IsPopup { get; set; }
    public virtual bool RequireConfirm { get; set; }
    public virtual int ConfirmedCount { get; set; }
    public virtual int ReadCount { get; set; }
    public virtual DateTime CreationTime { get; set; }
    public virtual bool IsConfirmedByCurrentUser { get; set; }
}
