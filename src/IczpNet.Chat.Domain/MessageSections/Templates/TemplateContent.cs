using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageTemplates;
using Volo.Abp.MultiTenancy;
using Volo.Abp;

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
    public virtual Guid TemplateId { get; protected set; }

    /// <summary>
    /// 模板发布版本
    /// </summary>
    public virtual int TemplateVersion { get; protected set; }

    /// <summary>
    /// 协议版本
    /// </summary>
    public virtual int SchemaVersion { get; protected set; } = MessageTemplateConsts.SchemaVersion;

    /// <summary>
    /// 降级/通知摘要文本
    /// </summary>
    [Required]
    [StringLength(MessageTemplateConsts.MaxFallbackTextLength)]
    public virtual string FallbackText { get; protected set; }

    /// <summary>
    /// 完整内容快照 JSON
    /// </summary>
    [Required]
    public virtual string SnapshotJson { get; protected set; }

    protected TemplateContent() { }

    public TemplateContent(
        Guid templateId,
        int templateVersion,
        int schemaVersion,
        string fallbackText,
        string snapshotJson,
        Guid? tenantId = null)
    {
        if (templateId == Guid.Empty || templateVersion < 1 || schemaVersion != MessageTemplateConsts.SchemaVersion ||
            string.IsNullOrWhiteSpace(fallbackText) || fallbackText.Length > MessageTemplateConsts.MaxFallbackTextLength ||
            string.IsNullOrWhiteSpace(snapshotJson))
            throw new UserFriendlyException("模板快照标识或摘要无效。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
        if (Encoding.UTF8.GetByteCount(snapshotJson) > MessageTemplateConsts.MaxSnapshotBytes)
            throw new UserFriendlyException("模板消息快照超过大小限制。", ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge);
        TemplateContentInfo snapshot;
        try { snapshot = JsonSerializer.Deserialize<TemplateContentInfo>(snapshotJson); }
        catch (JsonException) { throw new UserFriendlyException("模板快照 JSON 无效。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid); }
        if (snapshot == null || snapshot.TemplateId != templateId || snapshot.TemplateVersion != templateVersion ||
            snapshot.SchemaVersion != schemaVersion || snapshot.FallbackText != fallbackText)
            throw new UserFriendlyException("模板快照与标量字段不一致。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);

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
