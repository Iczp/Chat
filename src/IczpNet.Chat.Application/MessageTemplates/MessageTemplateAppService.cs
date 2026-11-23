using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageTemplates.Dtos;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageTemplates;

[Authorize]
public class MessageTemplateAppService(
    IRepository<MessageTemplateDefinition, Guid> templateRepository,
    IRepository<MessageTemplateRevision, Guid> revisionRepository,
    MessageTemplateManager templateManager) : ChatAppService, IMessageTemplateAppService
{
    protected IRepository<MessageTemplateDefinition, Guid> TemplateRepository { get; } = templateRepository;
    protected IRepository<MessageTemplateRevision, Guid> RevisionRepository { get; } = revisionRepository;
    protected MessageTemplateManager TemplateManager { get; } = templateManager;

    public virtual async Task<PagedResultDto<MessageTemplateDto>> GetListAsync(MessageTemplateGetListInput input)
    {
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Default);

        var query = await TemplateRepository.GetQueryableAsync();

        var hasManageAll = await AuthorizationService.IsGrantedAsync(ChatPermissions.MessageTemplatePermission.ManageAll);
        List<long> userChatObjectIds = [];
        if (CurrentUser.Id.HasValue)
        {
            userChatObjectIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        }

        if (input.Scope == "usable")
        {
            await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Use);
            // usable: must be enabled, published, and either TenantShared or owned by user
            query = query.Where(x => x.IsEnabled && x.PublishedVersion.HasValue &&
                (x.Visibility == MessageTemplateVisibility.TenantShared || userChatObjectIds.Contains(x.OwnerId)));
        }
        else
        {
            // default / "owned"
            if (!hasManageAll)
            {
                query = query.Where(x => userChatObjectIds.Contains(x.OwnerId));
            }
            else if (input.OwnerId.HasValue)
            {
                query = query.Where(x => x.OwnerId == input.OwnerId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            query = query.Where(x => x.Name.Contains(input.Keyword) || (x.Description != null && x.Description.Contains(input.Keyword)));
        }

        var totalCount = await query.CountAsync();

        var maxCount = Math.Min(input.MaxResultCount > 0 ? input.MaxResultCount : 10, 100);
        var skipCount = Math.Max(input.SkipCount, 0);

        var items = await query
            .OrderByDescending(x => x.CreationTime)
            .ThenBy(x => x.Id)
            .Skip(skipCount)
            .Take(maxCount)
            .ToListAsync();

        var dtoList = ObjectMapper.Map<List<MessageTemplateDefinition>, List<MessageTemplateDto>>(items);
        return new PagedResultDto<MessageTemplateDto>(totalCount, dtoList);
    }

    public virtual async Task<MessageTemplateDetailDto> GetAsync(Guid id)
    {
        var template = await TemplateRepository.FindAsync(id);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<MessageTemplateDetailDto> CreateAsync(MessageTemplateCreateInput input)
    {
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Create);
        await CheckPolicyForUserAsync(input.OwnerId);

        if (input.Visibility == MessageTemplateVisibility.TenantShared)
        {
            await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Share);
        }

        TemplateManager.ValidateDefinitionJson(input.Definition);

        var template = new MessageTemplateDefinition(
            GuidGenerator.Create(),
            input.OwnerId,
            input.Name,
            input.Description,
            input.Visibility,
            input.Definition,
            CurrentTenant.Id);

        await TemplateRepository.InsertAsync(template, autoSave: true);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<MessageTemplateDetailDto> UpdateAsync(Guid id, MessageTemplateUpdateInput input)
    {
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Update);

        var template = await TemplateRepository.FindAsync(id);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        if (template.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new UserFriendlyException("并发冲突，模板已被其他人修改，请刷新后再试。", ChatErrorCodes.TemplateMessages.TemplateConcurrencyConflict);
        }

        if (input.Visibility == MessageTemplateVisibility.TenantShared && template.Visibility != MessageTemplateVisibility.TenantShared)
        {
            await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Share);
        }

        TemplateManager.ValidateDefinitionJson(input.Definition);

        template.UpdateDraft(input.Name, input.Description, input.Visibility, input.Definition);

        await TemplateRepository.UpdateAsync(template, autoSave: true);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<MessageTemplatePublishOutput> PublishAsync(Guid id, MessageTemplatePublishInput input)
    {
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Publish);

        var template = await TemplateRepository.FindAsync(id);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        if (template.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new UserFriendlyException("并发冲突，模板已被其他人修改，请刷新后再试。", ChatErrorCodes.TemplateMessages.TemplateConcurrencyConflict);
        }

        var parsed = TemplateManager.ValidateDefinitionJson(template.DraftJson);
        var hash = TemplateManager.ComputeSha256(template.DraftJson);

        var nextVersion = (template.PublishedVersion ?? 0) + 1;

        var revision = new MessageTemplateRevision(
            GuidGenerator.Create(),
            template.Id,
            nextVersion,
            MessageTemplateConsts.SchemaVersion,
            template.DraftJson,
            hash,
            CurrentTenant.Id);

        await RevisionRepository.InsertAsync(revision, autoSave: true);

        template.SetPublishedVersion(nextVersion);
        await TemplateRepository.UpdateAsync(template, autoSave: true);

        return new MessageTemplatePublishOutput
        {
            TemplateId = template.Id,
            Version = nextVersion,
            SchemaVersion = MessageTemplateConsts.SchemaVersion,
            ConcurrencyStamp = template.ConcurrencyStamp
        };
    }

    public virtual async Task<MessageTemplateDetailDto> SetEnabledAsync(Guid id, MessageTemplateSetEnabledInput input)
    {
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Update);

        var template = await TemplateRepository.FindAsync(id);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        if (template.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new UserFriendlyException("并发冲突，模板已被其他人修改，请刷新后再试。", ChatErrorCodes.TemplateMessages.TemplateConcurrencyConflict);
        }

        template.SetEnabled(input.IsEnabled);
        await TemplateRepository.UpdateAsync(template, autoSave: true);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<PagedResultDto<MessageTemplateRevisionDto>> GetRevisionsAsync(Guid id, PagedResultRequestDto input)
    {
        var template = await TemplateRepository.FindAsync(id);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateReadPermissionAsync(template);

        var query = (await RevisionRepository.GetQueryableAsync())
            .Where(x => x.TemplateId == id);

        var totalCount = await query.CountAsync();
        var maxCount = Math.Min(input.MaxResultCount > 0 ? input.MaxResultCount : 10, 100);
        var skipCount = Math.Max(input.SkipCount, 0);

        var revisions = await query
            .OrderByDescending(x => x.Version)
            .Skip(skipCount)
            .Take(maxCount)
            .ToListAsync();

        var dtoList = ObjectMapper.Map<List<MessageTemplateRevision>, List<MessageTemplateRevisionDto>>(revisions);
        return new PagedResultDto<MessageTemplateRevisionDto>(totalCount, dtoList);
    }

    public virtual async Task<MessageTemplateRevisionDetailDto> GetRevisionAsync(Guid id, int version)
    {
        var template = await TemplateRepository.FindAsync(id);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateReadPermissionAsync(template);

        var revision = await (await RevisionRepository.GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.TemplateId == id && x.Version == version);

        if (revision == null)
        {
            throw new UserFriendlyException($"版本 {version} 不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        return ObjectMapper.Map<MessageTemplateRevision, MessageTemplateRevisionDetailDto>(revision);
    }

    public virtual async Task<MessageTemplatePreviewOutput> PreviewAsync(MessageTemplatePreviewInput input)
    {
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Default);

        if (input.TemplateId.HasValue && input.TemplateId.Value != Guid.Empty)
        {
            var template = await TemplateRepository.FindAsync(input.TemplateId.Value);
            if (template != null)
            {
                await CheckTemplateManagementPermissionAsync(template.OwnerId);
            }
        }

        try
        {
            var parsed = TemplateManager.ValidateDefinitionJson(input.Definition);
            var sanitizedData = TemplateManager.ValidateAndSanitizeData(input.Data, parsed);
            var fallback = TemplateManager.GenerateFallbackText(parsed, sanitizedData);
            var snapshot = TemplateManager.BuildSnapshot(input.TemplateId ?? Guid.Empty, 1, parsed, sanitizedData, fallback);

            return new MessageTemplatePreviewOutput
            {
                IsValid = true,
                Snapshot = snapshot
            };
        }
        catch (UserFriendlyException ex)
        {
            return new MessageTemplatePreviewOutput
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task CheckTemplateManagementPermissionAsync(long ownerId)
    {
        if (await AuthorizationService.IsGrantedAsync(ChatPermissions.MessageTemplatePermission.ManageAll))
        {
            return;
        }
        await CheckPolicyForUserAsync(ownerId);
    }

    private async Task CheckTemplateReadPermissionAsync(MessageTemplateDefinition template)
    {
        if (template.Visibility == MessageTemplateVisibility.TenantShared &&
            await AuthorizationService.IsGrantedAsync(ChatPermissions.MessageTemplatePermission.Use))
        {
            return;
        }
        await CheckTemplateManagementPermissionAsync(template.OwnerId);
    }
}
