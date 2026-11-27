using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementAdminListResult
{
    public long TotalCount { get; set; }

    public List<SessionAnnouncement> Items { get; set; } = new();
}

public interface ISessionAnnouncementAdminManager
{
    Task<SessionAnnouncementAdminListResult> GetAdminListAsync(
        Guid? sessionId,
        string keyword,
        AnnouncementStatus? status,
        bool? isPinned,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<SessionAnnouncement> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
