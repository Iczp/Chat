using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.ChatObjectCategories;

public class ChatObjectCategoryAdminListResult
{
    public long TotalCount { get; set; }

    public List<ChatObjectCategory> Items { get; set; } = new();
}

public interface IChatObjectCategoryManager : ITreeManager<ChatObjectCategory, Guid, ChatObjectCategoryInfo>
{
    Task<ChatObjectCategoryAdminListResult> GetAdminListAsync(
        string keyword = null,
        string chatObjectTypeId = null,
        Guid? parentId = null,
        bool filterByParentId = false,
        int skipCount = 0,
        int maxResultCount = 200,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<ChatObjectCategory> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ChatObjectCategory> CreateAdminAsync(
        string name,
        string chatObjectTypeId,
        Guid? parentId,
        string description,
        double sorting,
        CancellationToken cancellationToken = default);

    Task<ChatObjectCategory> UpdateAdminAsync(
        Guid id,
        string name,
        string chatObjectTypeId,
        Guid? parentId,
        string description,
        double sorting,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
