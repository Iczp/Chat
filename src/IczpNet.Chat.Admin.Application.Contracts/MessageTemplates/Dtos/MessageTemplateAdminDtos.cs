using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageTemplates;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.MessageTemplates.Dtos;

public class MessageTemplateAdminDto : EntityDto<Guid>
{
    [JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)]
    public long OwnerId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public MessageTemplateVisibility Visibility { get; set; }

    public bool IsEnabled { get; set; }

    public int? PublishedVersion { get; set; }

    public string ConcurrencyStamp { get; set; }

    public DateTime CreationTime { get; set; }
}

public class MessageTemplateAdminDetailDto : MessageTemplateAdminDto
{
    public string DraftJson { get; set; }
}

public class MessageTemplateAdminRevisionDto : EntityDto<Guid>
{
    public Guid TemplateId { get; set; }

    public int Version { get; set; }

    public int SchemaVersion { get; set; }

    public string DefinitionHash { get; set; }

    public DateTime CreationTime { get; set; }
}

public class MessageTemplateAdminRevisionDetailDto : MessageTemplateAdminRevisionDto
{
    public string DefinitionJson { get; set; }
}

public class MessageTemplateAdminGetListInput : PagedResultRequestDto
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long? OwnerId { get; set; }

    public string Keyword { get; set; }

    public string Scope { get; set; }

    public bool? IsEnabled { get; set; }
}

public class MessageTemplateAdminCreateInput
{
    [Required]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long OwnerId { get; set; }

    [Required]
    [StringLength(MessageTemplateConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(MessageTemplateConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public MessageTemplateVisibility Visibility { get; set; } = MessageTemplateVisibility.Private;

    [Required]
    public string Definition { get; set; }
}

public class MessageTemplateAdminUpdateInput
{
    [Required]
    [StringLength(MessageTemplateConsts.MaxNameLength)]
    public string Name { get; set; }

    [StringLength(MessageTemplateConsts.MaxDescriptionLength)]
    public string Description { get; set; }

    public MessageTemplateVisibility Visibility { get; set; } = MessageTemplateVisibility.Private;

    [Required]
    public string Definition { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; }
}

public class MessageTemplateAdminPublishInput
{
    [Required]
    public string ConcurrencyStamp { get; set; }
}

public class MessageTemplateAdminPublishOutput
{
    public Guid TemplateId { get; set; }

    public int Version { get; set; }

    public int SchemaVersion { get; set; }

    public string ConcurrencyStamp { get; set; }
}

public class MessageTemplateAdminSetEnabledInput
{
    public bool IsEnabled { get; set; }

    [Required]
    public string ConcurrencyStamp { get; set; }
}

public class MessageTemplateAdminPreviewInput
{
    public Guid? TemplateId { get; set; }

    [Required]
    public string Definition { get; set; }

    [Required]
    public Dictionary<string, object> Data { get; set; } = [];
}

public class MessageTemplateAdminPreviewOutput
{
    public bool IsValid { get; set; }

    public string ErrorMessage { get; set; }

    public TemplateContentInfo Snapshot { get; set; }
}
