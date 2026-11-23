using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageTemplates;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 模板消息持久化快照内容实体
/// </summary>
[MessageTemplate(MessageTypes.Template)]
[ContentOuput(typeof(TemplateContentInfo))]
public class TemplateContent : MessageContentEntityBase, IMultiTenant
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 模板ID
    /// </summary>
    public virtual Guid TemplateId { get; set; }

    /// <summary>
    /// 模板发布版本
    /// </summary>
    public virtual int TemplateVersion { get; set; }

    /// <summary>
    /// 协议版本
    /// </summary>
    public virtual int SchemaVersion { get; set; } = MessageTemplateConsts.SchemaVersion;

    /// <summary>
    /// 降级/通知摘要文本
    /// </summary>
    [Required]
    [StringLength(MessageTemplateConsts.MaxFallbackTextLength)]
    public virtual string FallbackText { get; set; }

    /// <summary>
    /// 完整内容快照 JSON
    /// </summary>
    [Required]
    public virtual string SnapshotJson { get; set; }

    public TemplateContent() { }

    public TemplateContent(
        Guid templateId,
        int templateVersion,
        int schemaVersion,
        string fallbackText,
        string snapshotJson,
        Guid? tenantId = null)
    {
        TemplateId = templateId;
        TemplateVersion = templateVersion;
        SchemaVersion = schemaVersion;
        FallbackText = fallbackText;
        SnapshotJson = snapshotJson;
        TenantId = tenantId;
    }

    public override string GetBody() => FallbackText;

    public override long GetSize() => Encoding.UTF8.GetByteCount(SnapshotJson ?? string.Empty);
}
