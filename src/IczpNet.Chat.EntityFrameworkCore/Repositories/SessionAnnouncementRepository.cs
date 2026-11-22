using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.Announcements;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class SessionAnnouncementRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
    : ChatRepositoryBase<SessionAnnouncement, Guid>(dbContextProvider), ISessionAnnouncementRepository
{
    public virtual async Task<SessionAnnouncement> GetPinnedAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId && x.IsPinned)
            .OrderByDescending(x => x.PinnedTime ?? x.CreationTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task UnpinOthersAsync(Guid sessionId, Guid excludeAnnouncementId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        await dbSet
            .Where(x => x.SessionId == sessionId && x.IsPinned && x.Id != excludeAnnouncementId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsPinned, false)
                .SetProperty(b => b.PinnedTime, (DateTime?)null),
                cancellationToken: cancellationToken);
    }
}
