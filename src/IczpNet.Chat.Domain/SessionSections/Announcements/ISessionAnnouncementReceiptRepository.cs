using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.Announcements;

public interface ISessionAnnouncementReceiptRepository : IRepository<SessionAnnouncementReceipt, long>
{
    Task<SessionAnnouncementReceipt> FindByUnitAsync(Guid announcementId, Guid sessionUnitId, CancellationToken cancellationToken = default);
}
