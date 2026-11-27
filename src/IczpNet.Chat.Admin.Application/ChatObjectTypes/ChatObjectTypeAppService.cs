using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjectTypes.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjectTypes;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.ChatObjectTypes;

/// <summary>
/// 聊天对象类型管理服务
/// </summary>
[Authorize(ChatAdminPermissions.ChatObjectTypes.Default)]
[ExposeServices(typeof(ChatObjectTypeAppService), typeof(IChatObjectTypeAdminAppService))]
public class ChatObjectTypeAppService(IChatObjectTypeManager typeManager)
    : ApplicationService, IChatObjectTypeAdminAppService
{
    protected IChatObjectTypeManager TypeManager { get; } = typeManager;

    [Authorize(ChatAdminPermissions.ChatObjectTypes.GetList)]
    public virtual async Task<PagedResultDto<ChatObjectTypeAdminDto>> GetListAsync(
        ChatObjectTypeAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new ChatObjectTypeAdminGetListInput();

        var result = await TypeManager.GetAdminListAsync(
            input.Keyword,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<ChatObjectTypeAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.ChatObjectTypes.GetDetail)]
    public virtual async Task<ChatObjectTypeAdminDto> GetAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await TypeManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(item);
    }

    [Authorize(ChatAdminPermissions.ChatObjectTypes.Create)]
    public virtual async Task<ChatObjectTypeAdminDto> CreateAsync(
        ChatObjectTypeAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var item = await TypeManager.CreateAdminAsync(
            input.Id,
            input.Name,
            input.ObjectType,
            input.Description,
            input.MaxDepth,
            input.IsHasChild,
            cancellationToken);
        return MapToDto(item);
    }

    [Authorize(ChatAdminPermissions.ChatObjectTypes.Update)]
    public virtual async Task<ChatObjectTypeAdminDto> UpdateAsync(
        string id,
        ChatObjectTypeAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var item = await TypeManager.UpdateAdminAsync(
            id,
            input.Name,
            input.ObjectType,
            input.Description,
            input.MaxDepth,
            input.IsHasChild,
            cancellationToken);
        return MapToDto(item);
    }

    [Authorize(ChatAdminPermissions.ChatObjectTypes.Delete)]
    public virtual Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return TypeManager.DeleteAdminAsync(id, cancellationToken);
    }

    private static ChatObjectTypeAdminDto MapToDto(ChatObjectTypeAdminItemResult item)
    {
        var entity = item.Entity;
        return new ChatObjectTypeAdminDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ObjectType = entity.ObjectType,
            Description = entity.Description,
            MaxDepth = entity.MaxDepth,
            IsHasChild = entity.IsHasChild,
            IsStatic = entity.IsStatic,
            ChatObjectCount = item.ChatObjectCount,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
