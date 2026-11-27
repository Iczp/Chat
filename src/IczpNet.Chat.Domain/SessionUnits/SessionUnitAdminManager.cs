using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public class SessionUnitAdminManager(
    ISessionUnitRepository sessionUnitRepository,
    ISessionUnitManager sessionUnitManager) : DomainService, ISessionUnitAdminManager, ITransientDependency
{
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;

    public virtual async Task<SessionUnitAdminListResult> GetAdminListAsync(
        long? ownerId,
        long? destinationId,
        ChatObjectTypeEnums? destinationObjectType,
        bool? isKilled,
        string keyword,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Destination)
            .Include(x => x.Setting)
            .WhereIf(ownerId.HasValue, x => x.OwnerId == ownerId.Value)
            .WhereIf(destinationId.HasValue, x => x.DestinationId == destinationId.Value)
            .WhereIf(destinationObjectType.HasValue, x => x.DestinationObjectType == destinationObjectType.Value)
            .WhereIf(isKilled.HasValue, x => x.Setting != null && x.Setting.IsKilled == isKilled.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                x.Destination != null &&
                (x.Destination.Name.Contains(kw) ||
                 (x.Destination.Code != null && x.Destination.Code.Contains(kw))));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new SessionUnitAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.Sorting)
                .ThenByDescending(x => x.Ticks)
                .ThenByDescending(x => x.CreationTime)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 50, 1, 200)),
            cancellationToken);

        return new SessionUnitAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<SessionUnit> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Include(x => x.Destination)
            .Include(x => x.Setting)
            .Where(x => x.Id == id);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SessionUnit), id);
        }

        return entity;
    }

    public virtual async Task<Guid> FindIdAdminAsync(
        long ownerId,
        long destinationId,
        CancellationToken cancellationToken = default)
    {
        var cached = await SessionUnitManager.FindCacheAsync(ownerId, destinationId);
        if (cached != null)
        {
            return cached.Id;
        }

        var query = (await SessionUnitRepository.GetQueryableAsync())
            .Where(x => x.OwnerId == ownerId && x.DestinationId == destinationId)
            .Select(x => (Guid?)x.Id);
        var id = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (!id.HasValue)
        {
            throw new EntityNotFoundException(typeof(SessionUnit), $"{ownerId}:{destinationId}");
        }

        return id.Value;
    }
}
