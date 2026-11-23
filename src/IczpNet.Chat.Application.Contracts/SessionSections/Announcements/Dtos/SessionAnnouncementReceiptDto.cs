using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementReceiptDto : EntityDto<long>
{
    public virtual Guid AnnouncementId { get; set; }
    public virtual Guid SessionUnitId { get; set; }
    public virtual long ChatObjectId { get; set; }
    public virtual string MemberName { get; set; }
    public virtual string MemberPortrait { get; set; }
    public virtual DateTime FirstReadTime { get; set; }
    public virtual bool IsConfirmed { get; set; }
    public virtual DateTime? ConfirmedTime { get; set; }
}
