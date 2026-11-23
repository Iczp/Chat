using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.MessageTemplates;

/// <summary>
/// 消息模板历史发布版本实体（不可变）
/// </summary>
public class MessageTemplateRevision : CreationAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 模板ID
    /// </summary>
    public virtual Guid TemplateId { get; protected set; }

    [ForeignKey(nameof(TemplateId))]
    public virtual MessageTemplateDefinition Template { get; protected set; }

    /// <summary>
    /// 发布版本号（从1递增）
    /// </summary>
    public virtual int Version { get; protected set; }

    /// <summary>
    /// 协议版本（当前固定为1）
    /// </summary>
    public virtual int SchemaVersion { get; protected set; } = MessageTemplateConsts.SchemaVersion;

    /// <summary>
    /// 发布时的完整定义 JSON
    /// </summary>
    [Required]
    public virtual string DefinitionJson { get; protected set; }

    /// <summary>
    /// 定义的 SHA-256 哈希值
    /// </summary>
    [Required]
    [StringLength(64)]
    public virtual string DefinitionHash { get; protected set; }

    protected MessageTemplateRevision() { }

    public MessageTemplateRevision(
        Guid id,
        Guid templateId,
        int version,
        int schemaVersion,
        string definitionJson,
        string definitionHash,
        Guid? tenantId = null) : base(id)
    {
        TemplateId = templateId;
        Version = version;
        SchemaVersion = schemaVersion;
        DefinitionJson = definitionJson;
        DefinitionHash = definitionHash;
        TenantId = tenantId;
    }
}
