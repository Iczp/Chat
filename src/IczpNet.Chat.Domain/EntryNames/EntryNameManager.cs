using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.AbpTrees;
using IczpNet.Chat.EntryValues;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.EntryNames;

public class EntryNameManager(
    IRepository<EntryName, Guid> repository) : TreeManager<EntryName, Guid, TreeInfo<Guid>>(repository), IEntryNameManager
{
    protected IRepository<EntryValue, Guid> EntryValueRepository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<EntryValue, Guid>>();

    public virtual async Task<EntryNameAdminListResult> GetAdminListAsync(
        string keyword = null,
        Guid? parentId = null,
        bool filterByParentId = false,
        int skipCount = 0,
        int maxResultCount = 200,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await Repository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                x.Name.Contains(keyword.Trim()) ||
                (x.Code != null && x.Code.Contains(keyword.Trim())))
            .WhereIf(filterByParentId, x => x.ParentId == parentId);

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new EntryNameAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 1000)),
            cancellationToken);

        return new EntryNameAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<EntryName> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(EntryName), id);
        }

        return entity;
    }

    public virtual async Task<EntryName> CreateAdminAsync(
        EntryNameAdminInputModel input,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(input.Code) ? null : input.Code.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedCode) &&
            await Repository.AnyAsync(x => x.Code == normalizedCode, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateEntryNameCode", $"属性字段编码 [{normalizedCode}] 已存在。");
        }

        var entity = new EntryName(GuidGenerator.Create(), input.Name.Trim(), input.ParentId)
        {
            Code = normalizedCode,
            InputType = string.IsNullOrWhiteSpace(input.InputType) ? "Text" : input.InputType.Trim(),
            Help = input.Help,
            DefaultValue = input.DefaultValue,
            Regex = input.Regex,
            MaxLenth = input.MaxLength,
            MinLenth = input.MinLength,
            MaxCount = input.MaxCount,
            MinCount = input.MinCount,
            IsChoice = input.IsChoice,
            IsUniqued = input.IsUniqued,
            IsRequired = input.IsRequired,
            IsPublic = input.IsPublic,
            ErrorMessage = input.ErrorMessage,
            Sorting = input.Sorting,
            Description = input.Description
        };

        return await base.CreateAsync(entity, isUnique: true);
    }

    public virtual async Task<EntryName> UpdateAdminAsync(
        Guid id,
        EntryNameAdminInputModel input,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);

        if (input.ParentId == id)
        {
            throw new BusinessException("ChatAdmin:EntryNameSelfParent", "属性字段不能将自身设为上级节点。");
        }

        var normalizedCode = string.IsNullOrWhiteSpace(input.Code) ? null : input.Code.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedCode) &&
            await Repository.AnyAsync(x => x.Id != id && x.Code == normalizedCode, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateEntryNameCode", $"属性字段编码 [{normalizedCode}] 已存在。");
        }

        entity.SetName(input.Name.Trim());
        entity.Code = normalizedCode;
        entity.InputType = string.IsNullOrWhiteSpace(input.InputType) ? entity.InputType : input.InputType.Trim();
        entity.Help = input.Help;
        entity.DefaultValue = input.DefaultValue;
        entity.Regex = input.Regex;
        entity.MaxLenth = input.MaxLength;
        entity.MinLenth = input.MinLength;
        entity.MaxCount = input.MaxCount;
        entity.MinCount = input.MinCount;
        entity.IsChoice = input.IsChoice;
        entity.IsUniqued = input.IsUniqued;
        entity.IsRequired = input.IsRequired;
        entity.IsPublic = input.IsPublic;
        entity.ErrorMessage = input.ErrorMessage;
        entity.Sorting = input.Sorting;
        entity.Description = input.Description;

        return await base.UpdateAsync(entity, input.ParentId, isUnique: true);
    }

    public virtual async Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(EntryName), id);
        }

        if (entity.IsStatic)
        {
            throw new BusinessException("ChatAdmin:CannotDeleteStaticEntryName", "系统固有属性字段不允许删除。");
        }

        if (await Repository.AnyAsync(x => x.ParentId == id, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:EntryNameHasChildren", "该属性字段存在子节点，请先删除或移动子节点。");
        }

        if (await EntryValueRepository.AnyAsync(x => x.EntryNameId == id, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:EntryNameInUse", "该属性字段存在关联字典项，无法删除。");
        }

        var parentId = entity.ParentId;
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        if (parentId.HasValue)
        {
            await UpdateParentChildrenCountAsync([parentId.Value]);
        }
        await RemoveCacheAsync();
    }

    private static IQueryable<EntryName> ApplySorting(IQueryable<EntryName> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.Sorting).ThenBy(x => x.CreationTime);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "name" => desc ? queryable.OrderByDescending(x => x.Name) : queryable.OrderBy(x => x.Name),
            "sorting" => desc ? queryable.OrderByDescending(x => x.Sorting) : queryable.OrderBy(x => x.Sorting),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.Sorting).ThenBy(x => x.CreationTime)
        };
    }
}
