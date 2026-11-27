using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Templates;

namespace IczpNet.Chat.MessageTemplates;

public class MessageTemplateAdminListResult
{
    public long TotalCount { get; set; }

    public List<MessageTemplateDefinition> Items { get; set; } = new();
}

public class MessageTemplateAdminRevisionListResult
{
    public long TotalCount { get; set; }

    public List<MessageTemplateRevision> Items { get; set; } = new();
}

public class MessageTemplateAdminPublishResult
{
    public Guid TemplateId { get; set; }

    public int Version { get; set; }

    public int SchemaVersion { get; set; }

    public string ConcurrencyStamp { get; set; }
}

public class MessageTemplateAdminPreviewResult
{
    public bool IsValid { get; set; }

    public string ErrorMessage { get; set; }

    public TemplateContentInfo Snapshot { get; set; }
}

public interface IMessageTemplateAdminManager
{
    Task<MessageTemplateAdminListResult> GetAdminListAsync(
        long? ownerId,
        string keyword,
        string scope,
        bool? isEnabled,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateDefinition> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateDefinition> CreateAdminAsync(
        long ownerId,
        string name,
        string description,
        MessageTemplateVisibility visibility,
        string definition,
        Guid? tenantId,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateDefinition> UpdateAdminAsync(
        Guid id,
        string concurrencyStamp,
        string name,
        string description,
        MessageTemplateVisibility visibility,
        string definition,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminPublishResult> PublishAdminAsync(
        Guid id,
        string concurrencyStamp,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateDefinition> SetEnabledAdminAsync(
        Guid id,
        bool isEnabled,
        string concurrencyStamp,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminRevisionListResult> GetRevisionsAdminAsync(
        Guid id,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateRevision> GetRevisionAdminAsync(
        Guid id,
        int version,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminPreviewResult> PreviewAdminAsync(
        Guid? templateId,
        string definition,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default);
}
