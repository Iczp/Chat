using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageTemplates.Dtos;

public class MessageTemplateDto : EntityDto<Guid>
{
    public virtual long OwnerId { get; set; }
    public virtual string Name { get; set; }
    public virtual string Description { get; set; }
    public virtual MessageTemplateVisibility Visibility { get; set; }
    public virtual bool IsEnabled { get; set; }
    public virtual int? PublishedVersion { get; set; }
    public virtual string ConcurrencyStamp { get; set; }
    public virtual DateTime CreationTime { get; set; }
}

public class MessageTemplateDetailDto : MessageTemplateDto
{
    public virtual string DraftJson { get; set; }
}

public class MessageTemplateRevisionDto : EntityDto<Guid>
{
    public virtual Guid TemplateId { get; set; }
    public virtual int Version { get; set; }
    public virtual int SchemaVersion { get; set; }
    public virtual string DefinitionHash { get; set; }
    public virtual DateTime CreationTime { get; set; }
}

public class MessageTemplateRevisionDetailDto : MessageTemplateRevisionDto
{
    public virtual string DefinitionJson { get; set; }
}

public class MessageTemplateGetListInput : PagedResultRequestDto
{
    public virtual long? OwnerId { get; set; }
    public virtual string Keyword { get; set; }
    public virtual string Scope { get; set; } // "owned" or "usable"
}

public class MessageTemplateCreateInput
{
    [Required]
    public virtual long OwnerId { get; set; }

    [Required]
    [StringLength(MessageTemplateConsts.MaxNameLength)]
    public virtual string Name { get; set; }

    [StringLength(MessageTemplateConsts.MaxDescriptionLength)]
    public virtual string Description { get; set; }

    public virtual MessageTemplateVisibility Visibility { get; set; } = MessageTemplateVisibility.Private;

    [Required]
    public virtual string Definition { get; set; }
}

public class MessageTemplateUpdateInput
{
    [Required]
    [StringLength(MessageTemplateConsts.MaxNameLength)]
    public virtual string Name { get; set; }

    [StringLength(MessageTemplateConsts.MaxDescriptionLength)]
    public virtual string Description { get; set; }

    public virtual MessageTemplateVisibility Visibility { get; set; } = MessageTemplateVisibility.Private;

    [Required]
    public virtual string Definition { get; set; }

    [Required]
    public virtual string ConcurrencyStamp { get; set; }
}

public class MessageTemplatePublishInput
{
    [Required]
    public virtual string ConcurrencyStamp { get; set; }
}

public class MessageTemplatePublishOutput
{
    public virtual Guid TemplateId { get; set; }
    public virtual int Version { get; set; }
    public virtual int SchemaVersion { get; set; }
    public virtual string ConcurrencyStamp { get; set; }
}

public class MessageTemplateSetEnabledInput
{
    public virtual bool IsEnabled { get; set; }

    [Required]
    public virtual string ConcurrencyStamp { get; set; }
}

public class MessageTemplatePreviewInput
{
    public virtual Guid? TemplateId { get; set; }

    [Required]
    public virtual string Definition { get; set; }

    [Required]
    public virtual Dictionary<string, object> Data { get; set; } = [];
}

public class MessageTemplatePreviewOutput
{
    public virtual bool IsValid { get; set; }
    public virtual string ErrorMessage { get; set; }
    public virtual TemplateContentInfo Snapshot { get; set; }
}
