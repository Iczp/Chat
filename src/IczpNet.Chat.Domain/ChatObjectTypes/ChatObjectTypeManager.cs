using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjectTypes;

public class ChatObjectTypeManager : DomainService, IChatObjectTypeManager
{
    protected IRepository<ChatObjectType, string> Repository { get; }

    protected IDistributedCache<ChatObjectTypeInfo, Guid> ChatObjectTypeCache { get; }

    protected IChatObjectRepository ChatObjectRepository =>
        LazyServiceProvider.LazyGetRequiredService<IChatObjectRepository>();

    protected IRepository<ChatObjectCategory, Guid> CategoryRepository =>
        LazyServiceProvider.LazyGetRequiredService<IRepository<ChatObjectCategory, Guid>>();

    public ChatObjectTypeManager(
        IRepository<ChatObjectType, string> repository)
    {
        Repository = repository;
    }

    public virtual Task<ChatObjectType> GetAsync(string id)
    {
        return Repository.GetAsync(id);
    }

    public virtual Task<ChatObjectType> GetAsync(ChatObjectTypeEnums chatObjectTypeEnum)
    {
        return GetAsync(chatObjectTypeEnum.ToString());
    }

    public virtual async Task<ChatObjectTypeAdminListResult> GetAdminListAsync(
        string keyword = null,
        int skipCount = 0,
        int maxResultCount = 50,
        string sorting = null,
        CancellationToken cancellationToken = default)
    {
        var queryable = (await Repository.GetQueryableAsync())
            .WhereIf(!string.IsNullOrWhiteSpace(keyword), x =>
                x.Id.Contains(keyword.Trim()) || x.Name.Contains(keyword.Trim()));

        var totalCount = await AsyncExecuter.LongCountAsync(queryable, cancellationToken);
        if (totalCount == 0)
        {
            return new ChatObjectTypeAdminListResult();
        }

        queryable = ApplySorting(queryable, sorting);

        var entities = await AsyncExecuter.ToListAsync(
            queryable
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount, 1, 200)),
            cancellationToken);

        var ids = entities.Select(x => x.Id).ToList();
        var countList = await AsyncExecuter.ToListAsync(
            (await ChatObjectRepository.GetQueryableAsync())
                .Where(x => ids.Contains(x.ChatObjectTypeId))
                .GroupBy(x => x.ChatObjectTypeId)
                .Select(g => new { TypeId = g.Key, Count = g.Count() }),
            cancellationToken);

        var counts = countList.ToDictionary(x => x.TypeId, x => x.Count);

        return new ChatObjectTypeAdminListResult
        {
            TotalCount = totalCount,
            Items = entities.Select(e => new ChatObjectTypeAdminItemResult
            {
                Entity = e,
                ChatObjectCount = counts.TryGetValue(e.Id, out var c) ? c : 0
            }).ToList()
        };
    }

    public virtual async Task<ChatObjectTypeAdminItemResult> GetAdminDetailAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ChatObjectType), id);
        }

        var count = await ChatObjectRepository.CountAsync(x => x.ChatObjectTypeId == id, cancellationToken);

        return new ChatObjectTypeAdminItemResult
        {
            Entity = entity,
            ChatObjectCount = count
        };
    }

    public virtual async Task<ChatObjectTypeAdminItemResult> CreateAdminAsync(
        string id,
        string name,
        ChatObjectTypeEnums? objectType,
        string description,
        int maxDepth,
        bool isHasChild,
        CancellationToken cancellationToken = default)
    {
        var normalizedId = id?.Trim();
        var normalizedName = name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedId))
        {
            throw new BusinessException("ChatAdmin:ChatObjectTypeIdRequired", "聊天对象类型标识不能为空。");
        }

        if (await Repository.AnyAsync(x => x.Id == normalizedId, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateChatObjectTypeId", $"聊天对象类型标识 [{normalizedId}] 已存在。");
        }

        if (await Repository.AnyAsync(x => x.Name == normalizedName, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateChatObjectTypeName", $"聊天对象类型名称 [{normalizedName}] 已存在。");
        }

        var resolvedObjectType = objectType ?? (
            Enum.TryParse<ChatObjectTypeEnums>(normalizedId, true, out var parsed)
                ? parsed
                : ChatObjectTypeEnums.Personal);

        var entity = new ChatObjectType(normalizedId)
        {
            Name = normalizedName,
            ObjectType = resolvedObjectType,
            Description = description,
            MaxDepth = Math.Max(0, maxDepth),
            IsHasChild = isHasChild
        };

        await Repository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);

        return new ChatObjectTypeAdminItemResult
        {
            Entity = entity,
            ChatObjectCount = 0
        };
    }

    public virtual async Task<ChatObjectTypeAdminItemResult> UpdateAdminAsync(
        string id,
        string name,
        ChatObjectTypeEnums? objectType,
        string description,
        int maxDepth,
        bool isHasChild,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetAsync(id, cancellationToken: cancellationToken);
        var normalizedName = name?.Trim();

        if (await Repository.AnyAsync(x => x.Id != id && x.Name == normalizedName, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:DuplicateChatObjectTypeName", $"聊天对象类型名称 [{normalizedName}] 已存在。");
        }

        entity.Name = normalizedName;
        if (objectType.HasValue)
        {
            entity.ObjectType = objectType.Value;
        }
        entity.Description = description;
        entity.MaxDepth = Math.Max(0, maxDepth);
        entity.IsHasChild = isHasChild;

        await Repository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);

        var count = await ChatObjectRepository.CountAsync(x => x.ChatObjectTypeId == id, cancellationToken);

        return new ChatObjectTypeAdminItemResult
        {
            Entity = entity,
            ChatObjectCount = count
        };
    }

    public virtual async Task DeleteAdminAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ChatObjectType), id);
        }

        if (entity.IsStatic)
        {
            throw new BusinessException("ChatAdmin:CannotDeleteStaticChatObjectType", "系统固有对象类型不允许删除。");
        }

        if (await ChatObjectRepository.AnyAsync(x => x.ChatObjectTypeId == id, cancellationToken) ||
            await CategoryRepository.AnyAsync(x => x.ChatObjectTypeId == id, cancellationToken))
        {
            throw new BusinessException("ChatAdmin:ChatObjectTypeInUse", "该对象类型已被聊天对象或分类引用，无法删除。");
        }

        await Repository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
    }

    private static IQueryable<ChatObjectType> ApplySorting(IQueryable<ChatObjectType> queryable, string sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return queryable.OrderByDescending(x => x.CreationTime).ThenBy(x => x.Id);
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var field = parts[0].ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "id" => desc ? queryable.OrderByDescending(x => x.Id) : queryable.OrderBy(x => x.Id),
            "name" => desc ? queryable.OrderByDescending(x => x.Name) : queryable.OrderBy(x => x.Name),
            "creationtime" => desc ? queryable.OrderByDescending(x => x.CreationTime) : queryable.OrderBy(x => x.CreationTime),
            _ => queryable.OrderByDescending(x => x.CreationTime).ThenBy(x => x.Id)
        };
    }
}
