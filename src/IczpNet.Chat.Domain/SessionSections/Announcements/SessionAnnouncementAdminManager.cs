using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementAdminManager(ISessionAnnouncementRepository repository)
    : DomainService, ISessionAnnouncementAdminManager, ITransientDependency
{
    protected ISessionAnnouncementRepository Repository { get; } = repository;

    public virtual async Task<SessionAnnouncementAdminListResult> GetAdminListAsync(
        Guid? sessionId,
        string keyword,
        AnnouncementStatus? status,
        bool? isPinned,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Session)
            .Include(x => x.PublisherChatObject)
            .WhereIf(sessionId.HasValue, x => x.SessionId == sessionId.Value)
            .WhereIf(status.HasValue, x => x.Status == status.Value)
            .WhereIf(isPinned.HasValue, x => x.IsPinned == isPinned.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                x.Title.Contains(kw) ||
                x.Content.Contains(kw));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new SessionAnnouncementAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.IsPinned)
                .ThenByDescending(x => x.Sorting)
                .ThenByDescending(x => x.CreationTime)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 200)),
            cancellationToken);

        return new SessionAnnouncementAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<SessionAnnouncement> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Session)
            .Include(x => x.PublisherChatObject)
            .Where(x => x.Id == id);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SessionAnnouncement), id);
        }

        return entity;
    }

    public virtual async Task<SessionAnnouncement> SetPinnedAdminAsync(
        Guid id,
        bool isPinned,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetAdminDetailAsync(id, cancellationToken);
        entity.SetPinned(isPinned, Clock.Now);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }
}
