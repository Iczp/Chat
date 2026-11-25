using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementDto : EntityDto<Guid>
{
    public virtual Guid? SessionId { get; set; }
    public virtual string SessionName { get; set; }
    public virtual long ChatObjectId { get; set; }
    public virtual Guid PublisherSessionUnitId { get; set; }
    public virtual long PublisherChatObjectId { get; set; }
    public virtual string PublisherName { get; set; }
    public virtual string PublisherPortrait { get; set; }
    public virtual string Title { get; set; }
    public virtual EditorTypes EditorType { get; set; }
    public virtual AnnouncementStatus Status { get; set; }
    public virtual bool IsPinned { get; set; }
    public virtual double Sorting { get; set; }
    public virtual DateTime? PinnedTime { get; set; }
    public virtual DateTime? ExpireTime { get; set; }
    public virtual DateTime? PublishTime { get; set; }
    public virtual bool ApplyToNewMembers { get; set; }
    public virtual bool IsExpired { get; set; }
    public virtual bool IsEffectiveForCurrentUser { get; set; } = true;
    public virtual bool IsPopup { get; set; }
    public virtual bool RequireConfirm { get; set; }
    public virtual int ConfirmedCount { get; set; }
    public virtual int ReadCount { get; set; }
    public virtual DateTime CreationTime { get; set; }
    public virtual bool IsConfirmedByCurrentUser { get; set; }
    public virtual int Version { get; set; }
    public virtual bool IsApproved { get; set; }
    public virtual Guid? ReviewerSessionUnitId { get; set; }
    public virtual long? ReviewerChatObjectId { get; set; }
    public virtual string ReviewerName { get; set; }
    public virtual string ReviewerPortrait { get; set; }
    public virtual DateTime? ReviewedTime { get; set; }
    public virtual string ReviewRemark { get; set; }
    public virtual int ConfirmedVersion { get; set; }
    public virtual bool CanEdit { get; set; }
    public virtual bool CanDelete { get; set; }
    public virtual bool CanPublish { get; set; }
    public virtual bool CanSubmitReview { get; set; }
    public virtual bool CanApprove { get; set; }
    public virtual bool CanRevoke { get; set; }
    public virtual bool CanPin { get; set; }
    public virtual bool CanRemind { get; set; }
    public virtual bool CanViewStat { get; set; }
}
