using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.EntryValues.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.EntryValues;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.EntryValues;

/// <summary>
/// 属性字典项管理服务
/// </summary>
[Authorize(ChatAdminPermissions.EntryValues.Default)]
[ExposeServices(typeof(EntryValueAppService), typeof(IEntryValueAdminAppService))]
public class EntryValueAppService(IEntryValueManager entryValueManager)
    : ApplicationService, IEntryValueAdminAppService
{
    protected IEntryValueManager EntryValueManager { get; } = entryValueManager;

    [Authorize(ChatAdminPermissions.EntryValues.GetList)]
    public virtual async Task<PagedResultDto<EntryValueAdminDto>> GetListAsync(
        EntryValueAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new EntryValueAdminGetListInput();

        var result = await EntryValueManager.GetAdminListAsync(
            input.EntryNameId,
            input.Keyword,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<EntryValueAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.EntryValues.GetDetail)]
    public virtual async Task<EntryValueAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntryValueManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.EntryValues.Create)]
    public virtual async Task<EntryValueAdminDto> CreateAsync(
        EntryValueAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntryValueManager.CreateAdminAsync(
            input.EntryNameId,
            input.Value,
            input.IsOption,
            input.IsPublic,
            cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.EntryValues.Update)]
    public virtual async Task<EntryValueAdminDto> UpdateAsync(
        Guid id,
        EntryValueAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntryValueManager.UpdateAdminAsync(
            id,
            input.Value,
            input.IsOption,
            input.IsPublic,
            cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.EntryValues.Delete)]
    public virtual Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return EntryValueManager.DeleteAdminAsync(id, cancellationToken);
    }

    private static EntryValueAdminDto MapToDto(EntryValue entity)
    {
        return new EntryValueAdminDto
        {
            Id = entity.Id,
            EntryNameId = entity.EntryNameId,
            Name = entity.EntryName?.Name,
            Value = entity.Value,
            IsStatic = entity.IsStatic,
            IsPublic = entity.IsPublic,
            IsOption = entity.IsOption,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
