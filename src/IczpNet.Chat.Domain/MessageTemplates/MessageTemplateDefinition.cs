using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IczpNet.Chat.ChatObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace IczpNet.Chat.MessageTemplates;

/// <summary>
/// 消息模板定义聚合根
/// </summary>
public class MessageTemplateDefinition : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 所属 ChatObject ID
    /// </summary>
    public virtual long OwnerId { get; protected set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 模板管理名称（最多80字符）
    /// </summary>
    [Required]
    [StringLength(MessageTemplateConsts.MaxNameLength)]
    public virtual string Name { get; protected set; }

    /// <summary>
    /// 管理说明（最多256字符）
    /// </summary>
    [StringLength(MessageTemplateConsts.MaxDescriptionLength)]
    public virtual string Description { get; protected set; }

    /// <summary>
    /// 可见性
    /// </summary>
    public virtual MessageTemplateVisibility Visibility { get; protected set; } = MessageTemplateVisibility.Private;

    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; protected set; } = true;

    /// <summary>
    /// 当前发布版本（首版为1，未发布时为null）
    /// </summary>
    public virtual int? PublishedVersion { get; protected set; }

    /// <summary>
    /// 当前草稿 JSON 定义
    /// </summary>
    [Required]
    public virtual string DraftJson { get; protected set; }

    protected MessageTemplateDefinition() { }

    public MessageTemplateDefinition(
        Guid id,
        long ownerId,
        string name,
        string description,
        MessageTemplateVisibility visibility,
        string draftJson,
        Guid? tenantId = null) : base(id)
    {
        OwnerId = ownerId;
        Name = name;
        Description = description;
        Visibility = visibility;
        DraftJson = draftJson;
        TenantId = tenantId;
        IsEnabled = true;
        PublishedVersion = null;
    }

    public virtual void UpdateDraft(
        string name,
        string description,
        MessageTemplateVisibility visibility,
        string draftJson)
    {
        Name = name;
        Description = description;
        Visibility = visibility;
        DraftJson = draftJson;
    }

    public virtual void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public virtual void SetPublishedVersion(int version)
    {
        PublishedVersion = version;
    }
}
