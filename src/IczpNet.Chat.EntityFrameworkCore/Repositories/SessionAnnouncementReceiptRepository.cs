using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.Announcements;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class SessionAnnouncementReceiptRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
    : ChatRepositoryBase<SessionAnnouncementReceipt, long>(dbContextProvider), ISessionAnnouncementReceiptRepository
{
    public virtual async Task<SessionAnnouncementReceipt> FindByUnitAsync(Guid announcementId, Guid sessionUnitId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.AnnouncementId == announcementId && x.SessionUnitId == sessionUnitId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
