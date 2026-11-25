using System;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementPermissionsDto
{
    public virtual Guid SessionId { get; set; }
    public virtual Guid SessionUnitId { get; set; }
    public virtual bool IsCreator { get; set; }
    public virtual bool CanCreate { get; set; }
    public virtual bool CanPublish { get; set; }
    public virtual bool CanApprove { get; set; }
    public virtual bool CanUpdateAny { get; set; }
    public virtual bool CanDeleteAny { get; set; }
    public virtual bool CanPin { get; set; }
    public virtual bool CanRemind { get; set; }
    public virtual bool CanViewStat { get; set; }
    public virtual int PendingReviewCount { get; set; }
    public virtual int DraftCount { get; set; }
}
