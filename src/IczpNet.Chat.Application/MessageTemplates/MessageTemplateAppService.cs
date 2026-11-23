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
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using System.Threading;

namespace IczpNet.Chat.MessageTemplates;

[Authorize]
[RemoteService(false)] // 使用显式 Controller，避免与自动控制器生成重复路径。
public class MessageTemplateAppService(
    IRepository<MessageTemplateDefinition, Guid> templateRepository,
    IRepository<MessageTemplateRevision, Guid> revisionRepository,
    MessageTemplateManager templateManager) : ChatAppService, IMessageTemplateAppService
{
    protected IRepository<MessageTemplateDefinition, Guid> TemplateRepository { get; } = templateRepository;
    protected IRepository<MessageTemplateRevision, Guid> RevisionRepository { get; } = revisionRepository;
    protected MessageTemplateManager TemplateManager { get; } = templateManager;
    private CancellationToken RequestCancellationToken => LazyServiceProvider.LazyGetRequiredService<ICancellationTokenProvider>().Token;

    public virtual async Task<PagedResultDto<MessageTemplateDto>> GetListAsync(MessageTemplateGetListInput input)
    {
        TemplateManager.EnsureEnabled();
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
                ((x.Visibility == MessageTemplateVisibility.TenantShared && x.TenantId.HasValue) || userChatObjectIds.Contains(x.OwnerId)));
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

        if (input.OwnerId.HasValue) query = query.Where(x => x.OwnerId == input.OwnerId.Value);

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            query = query.Where(x => x.Name.Contains(input.Keyword) || (x.Description != null && x.Description.Contains(input.Keyword)));
        }

        var totalCount = await query.CountAsync(RequestCancellationToken);

        var maxCount = Math.Min(input.MaxResultCount > 0 ? input.MaxResultCount : 10, 100);
        var skipCount = Math.Max(input.SkipCount, 0);

        var items = await query
            .OrderByDescending(x => x.CreationTime)
            .ThenBy(x => x.Id)
            .Skip(skipCount)
            .Take(maxCount)
            .ToListAsync(RequestCancellationToken);

        var dtoList = ObjectMapper.Map<List<MessageTemplateDefinition>, List<MessageTemplateDto>>(items);
        return new PagedResultDto<MessageTemplateDto>(totalCount, dtoList);
    }

    public virtual async Task<MessageTemplateDetailDto> GetAsync(Guid id)
    {
        TemplateManager.EnsureEnabled();
        var template = await TemplateRepository.FindAsync(id, cancellationToken: RequestCancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<MessageTemplateDetailDto> CreateAsync(MessageTemplateCreateInput input)
    {
        TemplateManager.EnsureEnabled();
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Create);
        await CheckPolicyForUserAsync(input.OwnerId);

        if (input.Visibility == MessageTemplateVisibility.TenantShared)
        {
            await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Share);
        }

        TemplateManager.ValidateMetadata(input.Name, input.Description, input.Visibility, CurrentTenant.Id);
        var definition = TemplateManager.ValidateDefinitionJson(input.Definition);

        var template = new MessageTemplateDefinition(
            GuidGenerator.Create(),
            input.OwnerId,
            input.Name,
            input.Description,
            input.Visibility,
            definition.RawJson,
            CurrentTenant.Id);

        await TemplateRepository.InsertAsync(template, autoSave: true, cancellationToken: RequestCancellationToken);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<MessageTemplateDetailDto> UpdateAsync(Guid id, MessageTemplateUpdateInput input)
    {
        TemplateManager.EnsureEnabled();
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Update);

        var template = await TemplateRepository.FindAsync(id, cancellationToken: RequestCancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        TemplateManager.CheckConcurrency(template, input.ConcurrencyStamp);

        if (input.Visibility == MessageTemplateVisibility.TenantShared && template.Visibility != MessageTemplateVisibility.TenantShared)
        {
            await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Share);
        }

        TemplateManager.UpdateDraft(template, input.ConcurrencyStamp, input.Name, input.Description, input.Visibility, input.Definition);

        await TemplateRepository.UpdateAsync(template, autoSave: true, cancellationToken: RequestCancellationToken);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task<MessageTemplatePublishOutput> PublishAsync(Guid id, MessageTemplatePublishInput input)
    {
        TemplateManager.EnsureEnabled();
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Publish);

        var template = await TemplateRepository.FindAsync(id, cancellationToken: RequestCancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        TemplateManager.CheckConcurrency(template, input.ConcurrencyStamp);

        var revision = TemplateManager.PreparePublication(template, input.ConcurrencyStamp, GuidGenerator.Create());
        // 先以原始并发戳更新模板；冲突先返回409，避免先插入相同版本而抛唯一索引500。
        // 方法的事务涵盖两次SaveChanges，插入版本失败时模板更新也回滚。
        await TemplateRepository.UpdateAsync(template, autoSave: true, cancellationToken: RequestCancellationToken);
        await RevisionRepository.InsertAsync(revision, autoSave: true, cancellationToken: RequestCancellationToken);

        return new MessageTemplatePublishOutput
        {
            TemplateId = template.Id,
            Version = revision.Version,
            SchemaVersion = MessageTemplateConsts.SchemaVersion,
            ConcurrencyStamp = template.ConcurrencyStamp
        };
    }

    public virtual async Task<MessageTemplateDetailDto> SetEnabledAsync(Guid id, MessageTemplateSetEnabledInput input)
    {
        TemplateManager.EnsureEnabled();
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Update);

        var template = await TemplateRepository.FindAsync(id, cancellationToken: RequestCancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateManagementPermissionAsync(template.OwnerId);

        TemplateManager.CheckConcurrency(template, input.ConcurrencyStamp);

        template.SetEnabled(input.IsEnabled);
        await TemplateRepository.UpdateAsync(template, autoSave: true, cancellationToken: RequestCancellationToken);

        return ObjectMapper.Map<MessageTemplateDefinition, MessageTemplateDetailDto>(template);
    }

    public virtual async Task<PagedResultDto<MessageTemplateRevisionDto>> GetRevisionsAsync(Guid id, PagedResultRequestDto input)
    {
        TemplateManager.EnsureEnabled();
        var template = await TemplateRepository.FindAsync(id, cancellationToken: RequestCancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateReadPermissionAsync(template);

        var query = (await RevisionRepository.GetQueryableAsync())
            .Where(x => x.TemplateId == id);

        var totalCount = await query.CountAsync(RequestCancellationToken);
        var maxCount = Math.Min(input.MaxResultCount > 0 ? input.MaxResultCount : 10, 100);
        var skipCount = Math.Max(input.SkipCount, 0);

        var revisions = await query
            .OrderByDescending(x => x.Version)
            .Skip(skipCount)
            .Take(maxCount)
            .ToListAsync(RequestCancellationToken);

        var dtoList = ObjectMapper.Map<List<MessageTemplateRevision>, List<MessageTemplateRevisionDto>>(revisions);
        return new PagedResultDto<MessageTemplateRevisionDto>(totalCount, dtoList);
    }

    public virtual async Task<MessageTemplateRevisionDetailDto> GetRevisionAsync(Guid id, int version)
    {
        TemplateManager.EnsureEnabled();
        var template = await TemplateRepository.FindAsync(id, cancellationToken: RequestCancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await CheckTemplateReadPermissionAsync(template);

        var revision = await (await RevisionRepository.GetQueryableAsync())
            .FirstOrDefaultAsync(x => x.TemplateId == id && x.Version == version, RequestCancellationToken);

        if (revision == null)
        {
            throw new UserFriendlyException($"版本 {version} 不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        return ObjectMapper.Map<MessageTemplateRevision, MessageTemplateRevisionDetailDto>(revision);
    }

    public virtual async Task<MessageTemplatePreviewOutput> PreviewAsync(MessageTemplatePreviewInput input)
    {
        TemplateManager.EnsureEnabled();
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Default);

        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Update);
        if (!input.TemplateId.HasValue || input.TemplateId.Value == Guid.Empty)
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        var template = await TemplateRepository.FindAsync(input.TemplateId.Value, cancellationToken: RequestCancellationToken);
        if (template == null)
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        await CheckTemplateManagementPermissionAsync(template.OwnerId);

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
        await AuthorizationService.CheckAsync(ChatPermissions.MessageTemplatePermission.Default);
        if (await AuthorizationService.IsGrantedAsync(ChatPermissions.MessageTemplatePermission.ManageAll))
        {
            return;
        }
        await CheckPolicyForUserAsync(ownerId);
    }

    private async Task CheckTemplateReadPermissionAsync(MessageTemplateDefinition template)
    {
        if (template.IsEnabled && template.PublishedVersion.HasValue && template.TenantId.HasValue &&
            template.Visibility == MessageTemplateVisibility.TenantShared &&
            await AuthorizationService.IsGrantedAsync(ChatPermissions.MessageTemplatePermission.Use))
        {
            return;
        }
        await CheckTemplateManagementPermissionAsync(template.OwnerId);
    }
}
