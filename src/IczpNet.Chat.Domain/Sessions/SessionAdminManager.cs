using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Sessions;

public class SessionAdminManager(ISessionRepository sessionRepository)
    : DomainService, ISessionAdminManager, ITransientDependency
{
    protected ISessionRepository SessionRepository { get; } = sessionRepository;

    public virtual async Task<SessionAdminListResult> GetAdminListAsync(
        string keyword,
        long? ownerId,
        Channels? channel,
        bool? isEnabled,
        bool? isAllMuted,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var query = (await SessionRepository.GetQueryableAsync())
            .Include(x => x.Owner)
            .WhereIf(ownerId.HasValue, x => x.OwnerId == ownerId.Value)
            .WhereIf(channel.HasValue, x => x.Channel == channel.Value)
            .WhereIf(isEnabled.HasValue, x => x.IsEnabled == isEnabled.Value)
            .WhereIf(isAllMuted.HasValue, x => x.IsAllMuted == isAllMuted.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                (x.Title != null && x.Title.Contains(kw)) ||
                (x.SessionKey != null && x.SessionKey.Contains(kw)) ||
                (x.Description != null && x.Description.Contains(kw)) ||
                (x.Owner != null && x.Owner.Name.Contains(kw)));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new SessionAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .ThenByDescending(x => x.LastMessageId)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 200)),
            cancellationToken);

        return new SessionAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<Session> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = (await SessionRepository.GetQueryableAsync())
            .Include(x => x.Owner)
            .Where(x => x.Id == id);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(Session), id);
        }

        return entity;
    }

    public virtual async Task<Session> SetIsEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetAdminDetailAsync(id, cancellationToken);
        entity.SetIsEnabled(isEnabled);
        await SessionRepository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<Session> SetIsAllMutedAdminAsync(
        Guid id,
        bool isAllMuted,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetAdminDetailAsync(id, cancellationToken);
        if (entity.Channel != Channels.RoomChannel)
        {
            throw new BusinessException("ChatAdmin:OnlyRoomSessionCanMute", "仅群聊会话支持全员禁言。");
        }

        entity.SetRoomPolicies(isAllMuted: isAllMuted);
        await SessionRepository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }
}
