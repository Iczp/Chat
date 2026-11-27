using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjectCategories.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjectCategories;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.ChatObjectCategories;

/// <summary>
/// 聊天对象分类管理服务
/// </summary>
[Authorize(ChatAdminPermissions.ChatObjectCategories.Default)]
[ExposeServices(typeof(ChatObjectCategoryAppService), typeof(IChatObjectCategoryAdminAppService))]
public class ChatObjectCategoryAppService(IChatObjectCategoryManager categoryManager)
    : ApplicationService, IChatObjectCategoryAdminAppService
{
    protected IChatObjectCategoryManager CategoryManager { get; } = categoryManager;

    [Authorize(ChatAdminPermissions.ChatObjectCategories.GetList)]
    public virtual async Task<PagedResultDto<ChatObjectCategoryAdminDto>> GetListAsync(
        ChatObjectCategoryAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new ChatObjectCategoryAdminGetListInput();

        var result = await CategoryManager.GetAdminListAsync(
            input.Keyword,
            input.ChatObjectTypeId,
            input.ParentId,
            input.FilterByParentId,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<ChatObjectCategoryAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.ChatObjectCategories.GetDetail)]
    public virtual async Task<ChatObjectCategoryAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await CategoryManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.ChatObjectCategories.Create)]
    public virtual async Task<ChatObjectCategoryAdminDto> CreateAsync(
        ChatObjectCategoryAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await CategoryManager.CreateAdminAsync(
            input.Name,
            input.ChatObjectTypeId,
            input.ParentId,
            input.Description,
            input.Sorting,
            cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.ChatObjectCategories.Update)]
    public virtual async Task<ChatObjectCategoryAdminDto> UpdateAsync(
        Guid id,
        ChatObjectCategoryAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await CategoryManager.UpdateAdminAsync(
            id,
            input.Name,
            input.ChatObjectTypeId,
            input.ParentId,
            input.Description,
            input.Sorting,
            cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.ChatObjectCategories.Delete)]
    public virtual Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return CategoryManager.DeleteAdminAsync(id, cancellationToken);
    }

    private static ChatObjectCategoryAdminDto MapToDto(ChatObjectCategory entity)
    {
        return new ChatObjectCategoryAdminDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ChatObjectTypeId = entity.ChatObjectTypeId,
            ParentId = entity.ParentId,
            Depth = entity.Depth,
            FullPath = entity.FullPath,
            FullPathName = entity.FullPathName,
            ChildrenCount = entity.ChildrenCount,
            Sorting = entity.Sorting,
            Description = entity.Description,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
