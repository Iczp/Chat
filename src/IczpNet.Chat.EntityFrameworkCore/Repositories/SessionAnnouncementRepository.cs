using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.Announcements;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        var now = DateTime.Now;
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId
                && (x.Sorting > 0 || x.IsPinned)
                && x.Status == AnnouncementStatus.Published
                && (x.ExpireTime == null || x.ExpireTime > now))
            .OrderByDescending(x => x.Sorting)
            .ThenByDescending(x => x.PinnedTime ?? x.CreationTime)
            .ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<SessionAnnouncement>> GetPinnedListAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId
                && (x.Sorting > 0 || x.IsPinned)
                && x.Status == AnnouncementStatus.Published
                && (x.ExpireTime == null || x.ExpireTime > now))
            .OrderByDescending(x => x.Sorting)
            .ThenByDescending(x => x.PinnedTime ?? x.CreationTime)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<SessionAnnouncement>> GetDueScheduledAsync(DateTime now, int maxCount = 50, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Status == AnnouncementStatus.Draft
                && x.IsApproved
                && x.PublishTime != null
                && x.PublishTime <= now)
            .OrderBy(x => x.PublishTime)
            .Take(maxCount)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task IncrementReceiptCountsAsync(Guid announcementId, int readCount, int confirmedCount, CancellationToken cancellationToken = default)
    {
        // Counters are independent of content edits and must not use tracked read/modify/write.
        await (await GetDbSetAsync()).Where(x => x.Id == announcementId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.ReadCount, x => x.ReadCount + readCount)
                .SetProperty(x => x.ConfirmedCount, x => x.ConfirmedCount + confirmedCount), cancellationToken);
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
