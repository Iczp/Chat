using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 模板消息发送输入
/// </summary>
public class TemplateContentInput : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// 模板ID
    /// </summary>
    [Required]
    public virtual Guid TemplateId { get; set; }

    /// <summary>
    /// 模板发布版本（从1递增，禁止使用0或负数）
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "模板版本必须大于等于 1")]
    public virtual int TemplateVersion { get; set; }

    /// <summary>
    /// 用户填写的模板数据（键值对）
    /// </summary>
    [Required]
    public virtual Dictionary<string, object> Data { get; set; } = [];
}

/// <summary>
/// 模板消息输出 DTO
/// </summary>
public class TemplateContentInfo : MessageContentInfoBase, IContentInfo
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public override Guid? Id { get; set; }

    /// <summary>
    /// 协议版本
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public virtual int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// 模板ID
    /// </summary>
    [JsonPropertyName("templateId")]
    public virtual Guid TemplateId { get; set; }

    /// <summary>
    /// 模板发布版本
    /// </summary>
    [JsonPropertyName("templateVersion")]
    public virtual int TemplateVersion { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    [JsonPropertyName("templateName")]
    public virtual string TemplateName { get; set; }

    /// <summary>
    /// 降级/通知摘要文本
    /// </summary>
    [JsonPropertyName("fallbackText")]
    public virtual string FallbackText { get; set; }

    /// <summary>
    /// 业务数据
    /// </summary>
    [JsonPropertyName("data")]
    public virtual object Data { get; set; }

    /// <summary>
    /// 受控卡片布局结构
    /// </summary>
    [JsonPropertyName("layout")]
    public virtual object Layout { get; set; }

    /// <summary>
    /// 动作列表
    /// </summary>
    [JsonPropertyName("actions")]
    public virtual object Actions { get; set; }

    /// <summary>
    /// 素材资源
    /// </summary>
    [JsonPropertyName("resources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual object Resources { get; set; }
}
