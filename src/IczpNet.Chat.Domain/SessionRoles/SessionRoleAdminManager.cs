using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionRoles;

public class SessionRoleAdminManager(IRepository<SessionRole, Guid> repository)
    : DomainService, ISessionRoleAdminManager, ITransientDependency
{
    protected IRepository<SessionRole, Guid> Repository { get; } = repository;

    public virtual async Task<SessionRoleAdminListResult> GetAdminListAsync(
        Guid? sessionId,
        string keyword,
        bool? isDefault,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Session)
            .WhereIf(sessionId.HasValue, x => x.SessionId == sessionId.Value)
            .WhereIf(isDefault.HasValue, x => x.IsDefault == isDefault.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                (x.Name != null && x.Name.Contains(kw)) ||
                (x.Description != null && x.Description.Contains(kw)));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new SessionRoleAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.PermissionCount)
                .ThenByDescending(x => x.CreationTime)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 200)),
            cancellationToken);

        return new SessionRoleAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<SessionRole> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Session)
            .Where(x => x.Id == id);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SessionRole), id);
        }

        return entity;
    }
}
