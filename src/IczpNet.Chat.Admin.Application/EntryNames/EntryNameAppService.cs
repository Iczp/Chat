using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.EntryNames.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.EntryNames;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.EntryNames;

/// <summary>
/// 属性字典字段管理服务
/// </summary>
[Authorize(ChatAdminPermissions.EntryNames.Default)]
[ExposeServices(typeof(EntryNameAppService), typeof(IEntryNameAdminAppService))]
public class EntryNameAppService(IEntryNameManager entryNameManager)
    : ApplicationService, IEntryNameAdminAppService
{
    protected IEntryNameManager EntryNameManager { get; } = entryNameManager;

    [Authorize(ChatAdminPermissions.EntryNames.GetList)]
    public virtual async Task<PagedResultDto<EntryNameAdminDto>> GetListAsync(
        EntryNameAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new EntryNameAdminGetListInput();

        var result = await EntryNameManager.GetAdminListAsync(
            input.Keyword,
            input.ParentId,
            input.FilterByParentId,
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            cancellationToken);

        return new PagedResultDto<EntryNameAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.EntryNames.GetDetail)]
    public virtual async Task<EntryNameAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntryNameManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.EntryNames.Create)]
    public virtual async Task<EntryNameAdminDto> CreateAsync(
        EntryNameAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntryNameManager.CreateAdminAsync(MapToModel(input), cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.EntryNames.Update)]
    public virtual async Task<EntryNameAdminDto> UpdateAsync(
        Guid id,
        EntryNameAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await EntryNameManager.UpdateAdminAsync(id, MapToModel(input), cancellationToken);
        return MapToDto(entity);
    }

    [Authorize(ChatAdminPermissions.EntryNames.Delete)]
    public virtual Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return EntryNameManager.DeleteAdminAsync(id, cancellationToken);
    }

    private static EntryNameAdminInputModel MapToModel(EntryNameAdminCreateInput input)
    {
        return new EntryNameAdminInputModel
        {
            Name = input.Name,
            ParentId = input.ParentId,
            Code = input.Code,
            InputType = input.InputType,
            Help = input.Help,
            DefaultValue = input.DefaultValue,
            Regex = input.Regex,
            MaxLength = input.MaxLength,
            MinLength = input.MinLength,
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
    }

    private static EntryNameAdminDto MapToDto(EntryName entity)
    {
        return new EntryNameAdminDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentId = entity.ParentId,
            Depth = entity.Depth,
            FullPath = entity.FullPath,
            FullPathName = entity.FullPathName,
            ChildrenCount = entity.ChildrenCount,
            Sorting = entity.Sorting,
            Description = entity.Description,
            Code = entity.Code,
            InputType = entity.InputType,
            Help = entity.Help,
            DefaultValue = entity.DefaultValue,
            Regex = entity.Regex,
            MaxLength = entity.MaxLenth,
            MinLength = entity.MinLenth,
            MaxCount = entity.MaxCount,
            MinCount = entity.MinCount,
            IsChoice = entity.IsChoice,
            IsUniqued = entity.IsUniqued,
            IsRequired = entity.IsRequired,
            IsStatic = entity.IsStatic,
            IsPublic = entity.IsPublic,
            ErrorMessage = entity.ErrorMessage,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
