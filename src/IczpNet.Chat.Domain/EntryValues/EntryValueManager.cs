using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.EntryNames;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.EntryValues;

public class EntryValueManager(
    IRepository<EntryValue, Guid> repository,
    IRepository<EntryName, Guid> entryNameRepository) : DomainService, IEntryValueManager
{
    protected IRepository<EntryValue, Guid> Repository { get; } = repository;
    protected IRepository<EntryName, Guid> EntryNameRepository { get; } = entryNameRepository;

    public virtual async Task<EntryValueAdminListResult> GetAdminListAsync(
        Guid? entryNameId = null,
        string keyword = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await Repository.WithDetailsAsync(x => x.EntryName))
            .WhereIf(entryNameId.HasValue, x => x.EntryNameId == entryNameId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x => x.Value.Contains(keyword.Trim()));

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new EntryValueAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        return new EntryValueAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<EntryValue> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var queryable = await Repository.WithDetailsAsync(x => x.EntryName);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id), cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(EntryValue), id);
        }

        return entity;
    }

    public virtual async Task<EntryValue> CreateAdminAsync(
        Guid entryNameId,
        string value,
        bool isOption,
        bool isPublic,
        CancellationToken cancellationToken = default)
    {
        var entryName = await EntryNameRepository.FindAsync(entryNameId, cancellationToken: cancellationToken)
            ?? throw new EntityNotFoundException(typeof(EntryName), entryNameId);

        var normalizedValue = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new BusinessException("ChatAdmin:EntryValueRequired", "字典项值不能为空。");
        }

        if (await Repository.AnyAsync(x => x.EntryNameId == entryNameId && x.Value == normalizedValue, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateEntryValue", $"该字段下已存在值 [{normalizedValue}]。");
        }

        var entity = new EntryValue(GuidGenerator.Create(), entryNameId, normalizedValue)
        {
            EntryName = entryName,
            IsOption = isOption,
            IsPublic = isPublic
        };

        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task<EntryValue> UpdateAdminAsync(
        Guid id,
        string value,
        bool isOption,
        bool isPublic,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetAdminDetailAsync(id, cancellationToken);

        var normalizedValue = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new BusinessException("ChatAdmin:EntryValueRequired", "字典项值不能为空。");
        }

        if (await Repository.AnyAsync(x => x.Id != id && x.EntryNameId == entity.EntryNameId && x.Value == normalizedValue, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateEntryValue", $"该字段下已存在值 [{normalizedValue}]。");
        }

        entity.Value = normalizedValue;
        entity.IsOption = isOption;
        entity.IsPublic = isPublic;

        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        return entity;
    }

    public virtual async Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(EntryValue), id);
        }

        if (entity.IsStatic)
        {
            throw new BusinessException("ChatAdmin:CannotDeleteStaticEntryValue", "系统固有字典项不允许删除。");
        }

        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }

    private static IQueryable<EntryValue> ApplySorting(IQueryable<EntryValue> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "value" => desc ? queryable.OrderByDescending(x => x.Value) : queryable.OrderBy(x => x.Value),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime)
        };
    }
}
