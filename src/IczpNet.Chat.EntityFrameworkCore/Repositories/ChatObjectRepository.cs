using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class ChatObjectRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
    : ChatRepositoryBase<ChatObject, long>(dbContextProvider), IChatObjectRepository
{
    public virtual async Task<ChatObjectAdminListResult> GetAdminPagedListAsync(
        ChatObjectAdminQueryFilter filter,
        List<Guid> resolvedCategoryIds,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await GetQueryableAsync()).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword;
            queryable = queryable.Where(x =>
                x.Name.Contains(keyword) ||
                x.Code.Contains(keyword) ||
                x.NameSpellingAbbreviation.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(filter.Code))
        {
            queryable = queryable.Where(x => x.Code == filter.Code);
        }

        if (filter.AppUserId.HasValue)
        {
            queryable = queryable.Where(x => x.AppUserId == filter.AppUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.ChatObjectTypeId))
        {
            queryable = queryable.Where(x => x.ChatObjectTypeId == filter.ChatObjectTypeId);
        }

        if (filter.ObjectType.HasValue)
        {
            queryable = queryable.Where(x => x.ObjectType == filter.ObjectType.Value);
        }

        if (filter.ObjectTypes != null && filter.ObjectTypes.Count > 0)
        {
            var types = filter.ObjectTypes.Select(t => (ChatObjectTypeEnums?)t).ToList();
            queryable = queryable.Where(x => types.Contains(x.ObjectType));
        }

        if (filter.CategoryIdList != null && filter.CategoryIdList.Count > 0)
        {
            if (resolvedCategoryIds == null || resolvedCategoryIds.Count == 0)
            {
                return new ChatObjectAdminListResult { TotalCount = 0, Items = [] };
            }

            queryable = queryable.Where(x =>
                x.ChatObjectCategoryUnitList.Any(u => resolvedCategoryIds.Contains(u.CategoryId)));
        }

        if (filter.IsEnabled.HasValue)
        {
            queryable = queryable.Where(x => x.IsEnabled == filter.IsEnabled.Value);
        }

        if (filter.IsDefault.HasValue)
        {
            queryable = queryable.Where(x => x.IsDefault == filter.IsDefault.Value);
        }

        if (filter.IsPublic.HasValue)
        {
            queryable = queryable.Where(x => x.IsPublic == filter.IsPublic.Value);
        }

        if (filter.IsStatic.HasValue)
        {
            queryable = queryable.Where(x => x.IsStatic == filter.IsStatic.Value);
        }

        if (filter.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == filter.IsActive.Value);
        }

        if (filter.IsDeveloper.HasValue)
        {
            queryable = queryable.Where(x => x.IsDeveloper == filter.IsDeveloper.Value);
        }

        var totalCount = await queryable.LongCountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return new ChatObjectAdminListResult
            {
                TotalCount = 0,
                Items = []
            };
        }

        queryable = ApplySorting(queryable, filter.Sorting);

        var items = await queryable
            .Skip(filter.SkipCount)
            .Take(filter.MaxResultCount)
            .ToListAsync(cancellationToken);

        return new ChatObjectAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<ChatObject> FindAdminDetailAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.ChatObjectCategoryUnitList)
                .ThenInclude(x => x.Category)
            .Include(x => x.Entries)
                .ThenInclude(x => x.EntryValue)
                    .ThenInclude(x => x.EntryName)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task<int> GetCountByAppUserIdAsync(
        Guid appUserId,
        CancellationToken cancellationToken = default)
    {
        var queryable = await GetQueryableAsync();
        return await queryable.CountAsync(x => x.AppUserId == appUserId, cancellationToken);
    }

    private static IQueryable<ChatObject> ApplySorting(IQueryable<ChatObject> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.Id);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "id" => desc ? queryable.OrderByDescending(x => x.Id) : queryable.OrderBy(x => x.Id),
            "name" => desc ? queryable.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.Name).ThenBy(x => x.Id),
            "code" => desc ? queryable.OrderByDescending(x => x.Code).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.Code).ThenBy(x => x.Id),
            "objecttype" => desc ? queryable.OrderByDescending(x => x.ObjectType).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.ObjectType).ThenBy(x => x.Id),
            "isenabled" => desc ? queryable.OrderByDescending(x => x.IsEnabled).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.IsEnabled).ThenBy(x => x.Id),
            "isactive" => desc ? queryable.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.CreationTime).ThenBy(x => x.Id),
            _ => queryable.OrderByDescending(x => x.CreationTime).ThenByDescending(x => x.Id)
        };
    }
}
