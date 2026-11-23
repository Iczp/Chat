using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.Announcements;

public interface ISessionAnnouncementRepository : IRepository<SessionAnnouncement, Guid>
{
    Task<SessionAnnouncement> GetPinnedAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task UnpinOthersAsync(Guid sessionId, Guid excludeAnnouncementId, CancellationToken cancellationToken = default);

    Task IncrementReceiptCountsAsync(Guid announcementId, int readCount, int confirmedCount, CancellationToken cancellationToken = default);
}
