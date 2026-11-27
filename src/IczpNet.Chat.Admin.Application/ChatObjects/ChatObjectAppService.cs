using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ChatObjects.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjects;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Admin.ChatObjects;

/// <summary>
/// 聊天对象管理服务
/// </summary>
[Authorize(ChatAdminPermissions.ChatObjects.Default)]
[ExposeServices(typeof(ChatObjectAppService), typeof(IChatObjectAdminAppService))]
public class ChatObjectAppService(IChatObjectManager chatObjectManager)
    : ApplicationService, IChatObjectAdminAppService
{
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;

    /// <summary>
    /// 分页查询聊天对象列表
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.GetList)]
    public virtual async Task<PagedResultDto<ChatObjectAdminListItemDto>> GetListAsync(
        ChatObjectAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new ChatObjectAdminGetListInput();

        var filter = new ChatObjectAdminQueryFilter
        {
            Keyword = input.Keyword,
            Code = input.Code,
            AppUserId = input.AppUserId,
            ChatObjectTypeId = input.ChatObjectTypeId,
            ObjectType = input.ObjectType,
            ObjectTypes = input.ObjectTypes,
            CategoryIdList = input.CategoryIdList,
            IncludeChildCategories = input.IncludeChildCategories,
            IsEnabled = input.IsEnabled,
            IsDefault = input.IsDefault,
            IsPublic = input.IsPublic,
            IsStatic = input.IsStatic,
            IsActive = input.IsActive,
            IsDeveloper = input.IsDeveloper,
            Sorting = input.Sorting,
            SkipCount = input.SkipCount,
            MaxResultCount = input.MaxResultCount
        };

        var result = await ChatObjectManager.GetAdminListAsync(filter, cancellationToken);

        return new PagedResultDto<ChatObjectAdminListItemDto>(
            result.TotalCount,
            result.Items.Select(MapToListItemDto).ToList());
    }

    /// <summary>
    /// 获取聊天对象详情
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.GetDetail)]
    public virtual async Task<ChatObjectAdminDetailDto> GetAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await ChatObjectManager.GetAdminDetailAsync(id, cancellationToken);
        return MapToDetailDto(result);
    }

    /// <summary>
    /// 更新聊天对象基础资料
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.Update)]
    public virtual async Task<ChatObjectAdminDetailDto> UpdateAsync(
        long id,
        ChatObjectAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await ChatObjectManager.UpdateAdminAsync(
            id,
            input.Name,
            input.Code,
            input.Gender,
            input.Description,
            input.Portrait,
            input.Thumbnail,
            input.IsPublic,
            cancellationToken);
        return MapToDetailDto(result);
    }

    /// <summary>
    /// 创建聊天对象
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.Create)]
    public virtual async Task<ChatObjectAdminDetailDto> CreateAsync(
        ChatObjectAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await ChatObjectManager.CreateAdminAsync(
            new ChatObjectAdminCreateArgs
            {
                Name = input.Name,
                Code = input.Code,
                Description = input.Description,
                ChatObjectTypeId = input.ChatObjectTypeId,
                ObjectType = input.ObjectType,
                Gender = input.Gender,
                IsEnabled = input.IsEnabled,
                IsPublic = input.IsPublic,
                Portrait = input.Portrait,
                Thumbnail = input.Thumbnail,
                VerificationMethod = input.VerificationMethod,
                CategoryIds = input.CategoryIds
            },
            cancellationToken);
        return MapToDetailDto(result);
    }

    /// <summary>
    /// 删除聊天对象
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.Delete)]
    public virtual Task DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return ChatObjectManager.DeleteAdminAsync(id, cancellationToken);
    }

    /// <summary>
    /// 设置聊天对象验证方式
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.SetVerificationMethod)]
    public virtual async Task<ChatObjectAdminDetailDto> SetVerificationMethodAsync(
        long id,
        ChatObjectAdminSetVerificationMethodInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await ChatObjectManager.SetVerificationMethodAdminAsync(
            id,
            input.VerificationMethod,
            cancellationToken);
        return MapToDetailDto(result);
    }

    /// <summary>
    /// 设置聊天对象启用/停用状态
    /// </summary>
    [Authorize(ChatAdminPermissions.ChatObjects.SetIsEnabled)]
    public virtual async Task<ChatObjectAdminDetailDto> SetIsEnabledAsync(
        long id,
        ChatObjectAdminSetIsEnabledInput input,
        CancellationToken cancellationToken = default)
    {
        var result = await ChatObjectManager.SetIsEnabledAdminAsync(
            id,
            input.IsEnabled,
            cancellationToken);
        return MapToDetailDto(result);
    }

    private static ChatObjectAdminListItemDto MapToListItemDto(ChatObject entity)
    {
        return new ChatObjectAdminListItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Code = entity.Code,
            AppUserId = entity.AppUserId,
            ChatObjectTypeId = entity.ChatObjectTypeId,
            ObjectType = entity.ObjectType,
            ObjectTypeDescription = entity.ObjectTypeDescription,
            Gender = entity.Gender,
            VerificationMethod = entity.VerificationMethod,
            Thumbnail = entity.Thumbnail,
            Portrait = entity.Portrait,
            Description = entity.Description,
            ParentId = entity.ParentId,
            Depth = entity.Depth,
            FullPath = entity.FullPath,
            FullPathName = entity.FullPathName,
            IsEnabled = entity.IsEnabled,
            IsDefault = entity.IsDefault,
            IsPublic = entity.IsPublic,
            IsStatic = entity.IsStatic,
            IsActive = entity.IsActive,
            IsDeveloper = entity.IsDeveloper,
            CreationTime = entity.CreationTime
        };
    }

    private static ChatObjectAdminDetailDto MapToDetailDto(ChatObjectAdminDetailResult result)
    {
        var entity = result.Entity;

        return new ChatObjectAdminDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Code = entity.Code,
            AppUserId = entity.AppUserId,
            ChatObjectTypeId = entity.ChatObjectTypeId,
            ObjectType = entity.ObjectType,
            ObjectTypeDescription = entity.ObjectTypeDescription,
            Gender = entity.Gender,
            VerificationMethod = entity.VerificationMethod,
            Thumbnail = entity.Thumbnail,
            Portrait = entity.Portrait,
            Description = entity.Description,
            ParentId = entity.ParentId,
            Depth = entity.Depth,
            FullPath = entity.FullPath,
            FullPathName = entity.FullPathName,
            IsEnabled = entity.IsEnabled,
            IsDefault = entity.IsDefault,
            IsPublic = entity.IsPublic,
            IsStatic = entity.IsStatic,
            IsActive = entity.IsActive,
            IsDeveloper = entity.IsDeveloper,
            CreationTime = entity.CreationTime,
            NameSpelling = entity.NameSpelling,
            NameSpellingAbbreviation = entity.NameSpellingAbbreviation,
            MaxMessageAutoId = entity.MaxMessageAutoId,
            LastModificationTime = entity.LastModificationTime,
            CategoryIds = result.CategoryIds,
            CategoryNames = result.CategoryNames,
            EntryValues = result.EntryValues.Select(x => new ChatObjectAdminEntryValueDto
            {
                EntryNameId = x.EntryNameId,
                EntryName = x.EntryName,
                EntryValueId = x.EntryValueId,
                Value = x.Value
            }).ToList(),
            SameUserChatObjectCount = result.SameUserChatObjectCount
        };
    }
}
