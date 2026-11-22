using System;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementDetailDto : SessionAnnouncementDto
{
    public virtual string Content { get; set; }
    public virtual long? LinkedMessageId { get; set; }
    public virtual DateTime? ConfirmedTime { get; set; }
}
