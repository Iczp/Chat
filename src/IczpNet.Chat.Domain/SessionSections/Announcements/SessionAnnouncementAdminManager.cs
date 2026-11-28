using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.Announcements;

public class SessionAnnouncementAdminManager(
    ISessionAnnouncementRepository repository,
    IAbpDistributedLock distributedLock)
    : DomainService, ISessionAnnouncementAdminManager, ITransientDependency
{
    protected ISessionAnnouncementRepository Repository { get; } = repository;
    protected IAbpDistributedLock DistributedLock { get; } = distributedLock;

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
        SessionAnnouncementAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        if (scope != null && scope.TenantId != CurrentTenant.Id)
        {
            throw new AbpAuthorizationException("跨租户操作公告被拒绝。");
        }

        var entity = await GetAdminDetailAsync(id, cancellationToken);
        if (scope != null && !scope.HasManageAll &&
            (!entity.SessionId.HasValue || !scope.AllowedSessionIds.Contains(entity.SessionId.Value)))
        {
            throw new AbpAuthorizationException("无权置顶该会话的公告。");
        }

        if (entity.SessionId.HasValue)
        {
            await using var handle = await DistributedLock.TryAcquireAsync(
                $"Chat:Announcement:Session:{entity.SessionId.Value:N}",
                TimeSpan.FromSeconds(10),
                cancellationToken);
            if (handle == null)
            {
                throw new UserFriendlyException("公告正在更新，请重试。");
            }

            entity.SetPinned(isPinned, Clock.Now);
            await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
            return entity;
        }

        entity.SetPinned(isPinned, Clock.Now);
        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }
}
