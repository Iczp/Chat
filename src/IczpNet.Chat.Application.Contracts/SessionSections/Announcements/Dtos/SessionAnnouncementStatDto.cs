using System;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementStatDto
{
    public virtual Guid AnnouncementId { get; set; }

    public virtual int TotalMembersCount { get; set; }

    public virtual int ReadCount { get; set; }

    public virtual int UnreadCount { get; set; }

    public virtual int ConfirmedCount { get; set; }

    public virtual int UnconfirmedCount { get; set; }

    public virtual double ReadRate => TotalMembersCount > 0 ? Math.Round((double)ReadCount / TotalMembersCount, 4) : 0;

    public virtual double ConfirmedRate => TotalMembersCount > 0 ? Math.Round((double)ConfirmedCount / TotalMembersCount, 4) : 0;
}
