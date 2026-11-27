using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjectCategories;

public class ChatObjectCategoryManager : TreeManager<ChatObjectCategory, Guid, ChatObjectCategoryInfo>, IChatObjectCategoryManager
{
    protected IChatObjectRepository ChatObjectRepository =>
        LazyServiceProvider.LazyGetRequiredService<IChatObjectRepository>();

    public ChatObjectCategoryManager(IRepository<ChatObjectCategory, Guid> repository) : base(repository)
    {
    }

    public virtual async Task<ChatObjectCategoryAdminListResult> GetAdminListAsync(
        string keyword = null,
        string chatObjectTypeId = null,
        Guid? parentId = null,
        bool filterByParentId = false,
        int skipCount = 0,
        int maxResultCount = 200,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await Repository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x => x.Name.Contains(keyword.Trim()))
            .WhereIf(!string.IsNullOrWhiteSpace(chatObjectTypeId), x => x.ChatObjectTypeId == chatObjectTypeId.Trim())
            .WhereIf(filterByParentId, x => x.ParentId == parentId);

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new ChatObjectCategoryAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 1000)),
            cancellationToken);

        return new ChatObjectCategoryAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<ChatObjectCategory> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ChatObjectCategory), id);
        }

        return entity;
    }

    public virtual async Task<ChatObjectCategory> CreateAdminAsync(
        string name,
        string chatObjectTypeId,
        Guid? parentId,
        string description,
        double sorting,
        CancellationToken cancellationToken = default)
    {
        var normalizedTypeId = string.IsNullOrWhiteSpace(chatObjectTypeId) ? null : chatObjectTypeId.Trim();

        if (parentId.HasValue)
        {
            var parent = await Repository.FindAsync(parentId.Value, cancellationToken: cancellationToken)
                ?? throw new EntityNotFoundException(typeof(ChatObjectCategory), parentId.Value);

            if (!string.IsNullOrWhiteSpace(normalizedTypeId) &&
                !string.IsNullOrWhiteSpace(parent.ChatObjectTypeId) &&
                parent.ChatObjectTypeId != normalizedTypeId)
            {
                throw new BusinessException("ChatAdmin:ChatObjectCategoryParentTypeMismatch", "子分类的对象类型必须与父分类一致。");
            }

            normalizedTypeId ??= parent.ChatObjectTypeId;
        }

        var entity = new ChatObjectCategory
        {
            ParentId = parentId,
            ChatObjectTypeId = normalizedTypeId,
            Description = description,
            Sorting = sorting
        };
        entity.SetName(name.Trim());

        return await base.CreateAsync(entity, isUnique: true);
    }

    public virtual async Task<ChatObjectCategory> UpdateAdminAsync(
        Guid id,
        string name,
        string chatObjectTypeId,
        Guid? parentId,
        string description,
        double sorting,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);

        if (parentId == id)
        {
            throw new BusinessException("ChatAdmin:ChatObjectCategorySelfParent", "分类不能将自身设为上级分类。");
        }

        var normalizedTypeId = string.IsNullOrWhiteSpace(chatObjectTypeId)
            ? entity.ChatObjectTypeId
            : chatObjectTypeId.Trim();

        if (parentId.HasValue)
        {
            var parent = await Repository.FindAsync(parentId.Value, cancellationToken: cancellationToken)
                ?? throw new EntityNotFoundException(typeof(ChatObjectCategory), parentId.Value);

            if (!string.IsNullOrWhiteSpace(normalizedTypeId) &&
                !string.IsNullOrWhiteSpace(parent.ChatObjectTypeId) &&
                parent.ChatObjectTypeId != normalizedTypeId)
            {
                throw new BusinessException("ChatAdmin:ChatObjectCategoryParentTypeMismatch", "子分类的对象类型必须与父分类一致。");
            }
        }

        entity.SetName(name.Trim());
        entity.ChatObjectTypeId = normalizedTypeId;
        entity.Description = description;
        entity.Sorting = sorting;

        return await base.UpdateAsync(entity, parentId, isUnique: true);
    }

    public virtual async Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ChatObjectCategory), id);
        }

        if (await Repository.AnyAsync(x => x.ParentId == id, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:ChatObjectCategoryHasChildren", "该分类存在子分类，请先删除或移动子分类。");
        }

        var isInUse = await AsyncExecuter.AnyAsync(
            (await ChatObjectRepository.GetQueryableAsync())
                .Where(x => x.ChatObjectCategoryUnitList.Any(u => u.CategoryId == id)),
            cancellationToken);
        if (isInUse)
        {
            throw new BusinessException("ChatAdmin:ChatObjectCategoryInUse", "该分类已被聊天对象引用，无法删除。");
        }

        var parentId = entity.ParentId;
        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        if (parentId.HasValue)
        {
            await UpdateParentChildrenCountAsync([parentId.Value]);
        }
        await RemoveCacheAsync();
    }

    private static IQueryable<ChatObjectCategory> ApplySorting(IQueryable<ChatObjectCategory> queryable, string sorting)
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
