using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageTemplates;

public class MessageTemplateAdminManager(
    IRepository<MessageTemplateDefinition, Guid> templateRepository,
    IRepository<MessageTemplateRevision, Guid> revisionRepository,
    MessageTemplateManager templateManager) : DomainService, IMessageTemplateAdminManager, ITransientDependency
{
    protected IRepository<MessageTemplateDefinition, Guid> TemplateRepository { get; } = templateRepository;
    protected IRepository<MessageTemplateRevision, Guid> RevisionRepository { get; } = revisionRepository;
    protected MessageTemplateManager TemplateManager { get; } = templateManager;

    private static void EnsureOwnerAuthorized(long ownerId, MessageTemplateAdminScope accessScope)
    {
        if (accessScope == null || accessScope.HasManageAll)
        {
            return;
        }

        if (accessScope.AllowedOwnerIds == null || !accessScope.AllowedOwnerIds.Contains(ownerId))
        {
            throw new AbpAuthorizationException("无权操作该主体的模板资源。");
        }
    }

    private static void EnsureTemplateAccess(
        MessageTemplateDefinition template,
        MessageTemplateAdminScope accessScope,
        bool allowUsableRead)
    {
        if (accessScope == null)
        {
            return;
        }

        if (template.TenantId != accessScope.TenantId)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        if (accessScope.HasManageAll)
        {
            return;
        }

        if (accessScope.AllowedOwnerIds != null && accessScope.AllowedOwnerIds.Contains(template.OwnerId))
        {
            return;
        }

        if (allowUsableRead &&
            accessScope.CanUse &&
            template.IsEnabled &&
            template.PublishedVersion.HasValue &&
            template.TenantId.HasValue &&
            template.Visibility == MessageTemplateVisibility.TenantShared)
        {
            return;
        }

        throw new AbpAuthorizationException("无权访问该模板资源。");
    }

    public virtual async Task<MessageTemplateAdminListResult> GetAdminListAsync(
        long? ownerId,
        string keyword,
        string scope,
        bool? isEnabled,
        int skipCount,
        int maxResultCount,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();

        var query = await TemplateRepository.GetQueryableAsync();

        if (accessScope != null)
        {
            query = query.Where(x => x.TenantId == accessScope.TenantId);
        }

        var allowedIds = accessScope?.AllowedOwnerIds?.ToList() ?? [];

        if (string.Equals(scope, "usable", StringComparison.OrdinalIgnoreCase))
        {
            if (accessScope != null && !accessScope.CanUse && !accessScope.HasManageAll)
            {
                throw new AbpAuthorizationException("缺少模板使用权限。");
            }

            query = query.Where(x => x.IsEnabled && x.PublishedVersion.HasValue);

            if (accessScope != null && !accessScope.HasManageAll)
            {
                query = query.Where(x =>
                    (x.Visibility == MessageTemplateVisibility.TenantShared && x.TenantId.HasValue) ||
                    allowedIds.Contains(x.OwnerId));
            }
        }
        else if (accessScope != null && !accessScope.HasManageAll)
        {
            query = query.Where(x => allowedIds.Contains(x.OwnerId));
        }

        if (ownerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == ownerId.Value);
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == isEnabled.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                x.Name.Contains(kw) ||
                (x.Description != null && x.Description.Contains(kw)));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new MessageTemplateAdminListResult();
        }

        var maxCount = Math.Clamp(maxResultCount > 0 ? maxResultCount : 10, 1, 100);
        var normalizedSkip = Math.Max(0, skipCount);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .ThenBy(x => x.Id)
                .Skip(normalizedSkip)
                .Take(maxCount),
            cancellationToken);

        return new MessageTemplateAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<MessageTemplateDefinition> GetAdminDetailAsync(
        Guid id,
        MessageTemplateAdminScope accessScope = null,
        bool allowUsableRead = false,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();

        var template = await TemplateRepository.FindAsync(id, cancellationToken: cancellationToken);
        if (template == null)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        EnsureTemplateAccess(template, accessScope, allowUsableRead);
        return template;
    }

    public virtual async Task<MessageTemplateDefinition> CreateAdminAsync(
        long ownerId,
        string name,
        string description,
        MessageTemplateVisibility visibility,
        string definition,
        Guid? tenantId,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();
        EnsureOwnerAuthorized(ownerId, accessScope);
        TemplateManager.ValidateMetadata(name, description, visibility, tenantId);
        var parsed = TemplateManager.ValidateDefinitionJson(definition);

        var template = new MessageTemplateDefinition(
            GuidGenerator.Create(),
            ownerId,
            name,
            description,
            visibility,
            parsed.RawJson,
            tenantId);

        await TemplateRepository.InsertAsync(template, autoSave: true, cancellationToken: cancellationToken);
        return template;
    }

    public virtual async Task<MessageTemplateDefinition> UpdateAdminAsync(
        Guid id,
        string concurrencyStamp,
        string name,
        string description,
        MessageTemplateVisibility visibility,
        string definition,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();

        var template = await GetAdminDetailAsync(id, accessScope, allowUsableRead: false, cancellationToken);
        TemplateManager.CheckConcurrency(template, concurrencyStamp);
        TemplateManager.UpdateDraft(template, concurrencyStamp, name, description, visibility, definition);

        await TemplateRepository.UpdateAsync(template, autoSave: true, cancellationToken: cancellationToken);
        return template;
    }

    [UnitOfWork(isTransactional: true)]
    public virtual async Task<MessageTemplateAdminPublishResult> PublishAdminAsync(
        Guid id,
        string concurrencyStamp,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();

        var template = await GetAdminDetailAsync(id, accessScope, allowUsableRead: false, cancellationToken);
        TemplateManager.CheckConcurrency(template, concurrencyStamp);

        var revision = TemplateManager.PreparePublication(template, concurrencyStamp, GuidGenerator.Create());
        await TemplateRepository.UpdateAsync(template, autoSave: true, cancellationToken: cancellationToken);
        await RevisionRepository.InsertAsync(revision, autoSave: true, cancellationToken: cancellationToken);

        return new MessageTemplateAdminPublishResult
        {
            TemplateId = template.Id,
            Version = revision.Version,
            SchemaVersion = MessageTemplateConsts.SchemaVersion,
            ConcurrencyStamp = template.ConcurrencyStamp
        };
    }

    public virtual async Task<MessageTemplateDefinition> SetEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        string concurrencyStamp,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();

        var template = await GetAdminDetailAsync(id, accessScope, allowUsableRead: false, cancellationToken);
        TemplateManager.CheckConcurrency(template, concurrencyStamp);

        template.SetEnabled(isEnabled);
        await TemplateRepository.UpdateAsync(template, autoSave: true, cancellationToken: cancellationToken);
        return template;
    }

    public virtual async Task<MessageTemplateAdminRevisionListResult> GetRevisionsAdminAsync(
        Guid id,
        int skipCount,
        int maxResultCount,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();
        await GetAdminDetailAsync(id, accessScope, allowUsableRead: true, cancellationToken);

        var query = (await RevisionRepository.GetQueryableAsync())
            .Where(x => x.TemplateId == id);

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new MessageTemplateAdminRevisionListResult();
        }

        var maxCount = Math.Clamp(maxResultCount > 0 ? maxResultCount : 10, 1, 100);
        var normalizedSkip = Math.Max(0, skipCount);

        var revisions = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.Version)
                .Skip(normalizedSkip)
                .Take(maxCount),
            cancellationToken);

        return new MessageTemplateAdminRevisionListResult
        {
            TotalCount = totalCount,
            Items = revisions
        };
    }

    public virtual async Task<MessageTemplateRevision> GetRevisionAdminAsync(
        Guid id,
        int version,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();
        await GetAdminDetailAsync(id, accessScope, allowUsableRead: true, cancellationToken);

        var query = (await RevisionRepository.GetQueryableAsync())
            .Where(x => x.TemplateId == id && x.Version == version);
        var revision = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);

        if (revision == null)
        {
            throw new UserFriendlyException($"版本 {version} 不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        return revision;
    }

    public virtual async Task<MessageTemplateAdminPreviewResult> PreviewAdminAsync(
        Guid? templateId,
        string definition,
        Dictionary<string, object> data,
        MessageTemplateAdminScope accessScope = null,
        CancellationToken cancellationToken = default)
    {
        TemplateManager.EnsureEnabled();

        if (!templateId.HasValue || templateId.Value == Guid.Empty)
        {
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        }

        await GetAdminDetailAsync(templateId.Value, accessScope, allowUsableRead: false, cancellationToken);

        try
        {
            var parsed = TemplateManager.ValidateDefinitionJson(definition);
            var sanitizedData = TemplateManager.ValidateAndSanitizeData(data, parsed);
            var fallback = TemplateManager.GenerateFallbackText(parsed, sanitizedData);
            var snapshot = TemplateManager.BuildSnapshot(templateId.Value, 1, parsed, sanitizedData, fallback);

            return new MessageTemplateAdminPreviewResult
            {
                IsValid = true,
                Snapshot = snapshot
            };
        }
        catch (UserFriendlyException ex)
        {
            return new MessageTemplateAdminPreviewResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
