using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.MessageTemplates.Dtos;
using IczpNet.Chat.Admin.Permissions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageTemplates;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Admin.MessageTemplates;

/// <summary>
/// 自定义消息模板管理服务
/// </summary>
[Authorize(ChatAdminPermissions.MessageTemplates.Default)]
[ExposeServices(typeof(MessageTemplateAppService), typeof(IMessageTemplateAdminAppService))]
public class MessageTemplateAppService(
    IMessageTemplateAdminManager templateAdminManager,
    IChatObjectManager chatObjectManager)
    : ApplicationService, IMessageTemplateAdminAppService
{
    protected IMessageTemplateAdminManager TemplateAdminManager { get; } = templateAdminManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;

    private async Task<MessageTemplateAdminScope> BuildScopeAsync()
    {
        var hasManageAll = await AuthorizationService.IsGrantedAsync(ChatAdminPermissions.MessageTemplates.ManageAll);
        var canUse = await AuthorizationService.IsGrantedAsync(ChatAdminPermissions.MessageTemplates.Use);
        IReadOnlyCollection<long> allowedOwners = null;
        if (!hasManageAll && CurrentUser.Id.HasValue)
        {
            allowedOwners = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        }

        return new MessageTemplateAdminScope
        {
            TenantId = CurrentTenant.Id,
            HasManageAll = hasManageAll,
            CanUse = canUse,
            AllowedOwnerIds = allowedOwners
        };
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.GetList)]
    public virtual async Task<PagedResultDto<MessageTemplateAdminDto>> GetListAsync(
        MessageTemplateAdminGetListInput input,
        CancellationToken cancellationToken = default)
    {
        input ??= new MessageTemplateAdminGetListInput();
        var accessScope = await BuildScopeAsync();

        var result = await TemplateAdminManager.GetAdminListAsync(
            input.OwnerId,
            input.Keyword,
            input.Scope,
            input.IsEnabled,
            input.SkipCount,
            input.MaxResultCount,
            accessScope,
            cancellationToken);

        return new PagedResultDto<MessageTemplateAdminDto>(
            result.TotalCount,
            result.Items.Select(MapToDto).ToList());
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.GetDetail)]
    public virtual async Task<MessageTemplateAdminDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var accessScope = await BuildScopeAsync();
        var entity = await TemplateAdminManager.GetAdminDetailAsync(id, accessScope, allowUsableRead: false, cancellationToken);
        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.Create)]
    public virtual async Task<MessageTemplateAdminDetailDto> CreateAsync(
        MessageTemplateAdminCreateInput input,
        CancellationToken cancellationToken = default)
    {
        if (input.Visibility == MessageTemplateVisibility.TenantShared)
        {
            await AuthorizationService.CheckAsync(ChatAdminPermissions.MessageTemplates.Share);
        }

        var accessScope = await BuildScopeAsync();
        var entity = await TemplateAdminManager.CreateAdminAsync(
            input.OwnerId,
            input.Name,
            input.Description,
            input.Visibility,
            input.Definition,
            CurrentTenant.Id,
            accessScope,
            cancellationToken);

        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.Update)]
    public virtual async Task<MessageTemplateAdminDetailDto> UpdateAsync(
        Guid id,
        MessageTemplateAdminUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        if (input.Visibility == MessageTemplateVisibility.TenantShared)
        {
            await AuthorizationService.CheckAsync(ChatAdminPermissions.MessageTemplates.Share);
        }

        var accessScope = await BuildScopeAsync();
        var entity = await TemplateAdminManager.UpdateAdminAsync(
            id,
            input.ConcurrencyStamp,
            input.Name,
            input.Description,
            input.Visibility,
            input.Definition,
            accessScope,
            cancellationToken);

        return MapToDetailDto(entity);
    }

    [UnitOfWork(isTransactional: true)]
    [Authorize(ChatAdminPermissions.MessageTemplates.Publish)]
    public virtual async Task<MessageTemplateAdminPublishOutput> PublishAsync(
        Guid id,
        MessageTemplateAdminPublishInput input,
        CancellationToken cancellationToken = default)
    {
        var accessScope = await BuildScopeAsync();
        var result = await TemplateAdminManager.PublishAdminAsync(
            id,
            input.ConcurrencyStamp,
            accessScope,
            cancellationToken);

        return new MessageTemplateAdminPublishOutput
        {
            TemplateId = result.TemplateId,
            Version = result.Version,
            SchemaVersion = result.SchemaVersion,
            ConcurrencyStamp = result.ConcurrencyStamp
        };
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.Update)]
    public virtual async Task<MessageTemplateAdminDetailDto> SetEnabledAsync(
        Guid id,
        MessageTemplateAdminSetEnabledInput input,
        CancellationToken cancellationToken = default)
    {
        var accessScope = await BuildScopeAsync();
        var entity = await TemplateAdminManager.SetEnabledAdminAsync(
            id,
            input.IsEnabled,
            input.ConcurrencyStamp,
            accessScope,
            cancellationToken);

        return MapToDetailDto(entity);
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.GetDetail)]
    public virtual async Task<PagedResultDto<MessageTemplateAdminRevisionDto>> GetRevisionsAsync(
        Guid id,
        PagedResultRequestDto input,
        CancellationToken cancellationToken = default)
    {
        input ??= new PagedResultRequestDto();
        var accessScope = await BuildScopeAsync();

        var result = await TemplateAdminManager.GetRevisionsAdminAsync(
            id,
            input.SkipCount,
            input.MaxResultCount,
            accessScope,
            cancellationToken);

        return new PagedResultDto<MessageTemplateAdminRevisionDto>(
            result.TotalCount,
            result.Items.Select(MapToRevisionDto).ToList());
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.GetDetail)]
    public virtual async Task<MessageTemplateAdminRevisionDetailDto> GetRevisionAsync(
        Guid id,
        int version,
        CancellationToken cancellationToken = default)
    {
        var accessScope = await BuildScopeAsync();
        var entity = await TemplateAdminManager.GetRevisionAdminAsync(id, version, accessScope, cancellationToken);
        return new MessageTemplateAdminRevisionDetailDto
        {
            Id = entity.Id,
            TemplateId = entity.TemplateId,
            Version = entity.Version,
            SchemaVersion = entity.SchemaVersion,
            DefinitionHash = entity.DefinitionHash,
            DefinitionJson = entity.DefinitionJson,
            CreationTime = entity.CreationTime
        };
    }

    [Authorize(ChatAdminPermissions.MessageTemplates.Update)]
    public virtual async Task<MessageTemplateAdminPreviewOutput> PreviewAsync(
        MessageTemplateAdminPreviewInput input,
        CancellationToken cancellationToken = default)
    {
        var accessScope = await BuildScopeAsync();
        var result = await TemplateAdminManager.PreviewAdminAsync(
            input.TemplateId,
            input.Definition,
            input.Data,
            accessScope,
            cancellationToken);

        return new MessageTemplateAdminPreviewOutput
        {
            IsValid = result.IsValid,
            ErrorMessage = result.ErrorMessage,
            Snapshot = result.Snapshot
        };
    }

    private static MessageTemplateAdminDto MapToDto(MessageTemplateDefinition entity)
    {
        return new MessageTemplateAdminDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Name = entity.Name,
            Description = entity.Description,
            Visibility = entity.Visibility,
            IsEnabled = entity.IsEnabled,
            PublishedVersion = entity.PublishedVersion,
            ConcurrencyStamp = entity.ConcurrencyStamp,
            CreationTime = entity.CreationTime
        };
    }

    private static MessageTemplateAdminDetailDto MapToDetailDto(MessageTemplateDefinition entity)
    {
        return new MessageTemplateAdminDetailDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Name = entity.Name,
            Description = entity.Description,
            Visibility = entity.Visibility,
            IsEnabled = entity.IsEnabled,
            PublishedVersion = entity.PublishedVersion,
            ConcurrencyStamp = entity.ConcurrencyStamp,
            CreationTime = entity.CreationTime,
            DraftJson = entity.DraftJson
        };
    }

    private static MessageTemplateAdminRevisionDto MapToRevisionDto(MessageTemplateRevision entity)
    {
        return new MessageTemplateAdminRevisionDto
        {
            Id = entity.Id,
            TemplateId = entity.TemplateId,
            Version = entity.Version,
            SchemaVersion = entity.SchemaVersion,
            DefinitionHash = entity.DefinitionHash,
            CreationTime = entity.CreationTime
        };
    }
}
