using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace IczpNet.Chat.MessageTemplates;

public class MessageTemplateManager : DomainService
{
    private static readonly HashSet<string> AllowedFieldTypes = ["string", "boolean", "integer", "number"];
    private static readonly HashSet<string> AllowedFormats = ["text", "url"];
    private static readonly HashSet<string> ForbiddenFieldNames = ["__proto__", "prototype", "constructor"];
    private static readonly HashSet<string> AllowedNodeTypes = ["card", "column", "row", "text", "field", "badge", "image", "divider", "button"];
    private static readonly HashSet<string> AllowedGaps = ["small", "medium", "large"];
    private static readonly HashSet<string> AllowedStyles = ["heading", "body", "secondary"];
    private static readonly HashSet<string> AllowedTones = ["neutral", "success", "warning", "danger"];
    private static readonly HashSet<string> AllowedAspectRatios = ["square", "landscape", "portrait"];
    private static readonly HashSet<string> AllowedActionTypes = ["openUrl", "copyText"];

    public MessageTemplateManager() { }

    /// <summary>迁移和客户端部署完成前，默认关闭模板写入口。</summary>
    public virtual void EnsureEnabled()
    {
        if (!LazyServiceProvider.LazyGetRequiredService<IConfiguration>().GetValue<bool>("Chat:TemplateMessages:Enabled"))
            throw new UserFriendlyException("模板消息功能尚未启用。", ChatErrorCodes.TemplateMessages.TemplateDisabled);
    }

    /// <summary>领域层统一检查租户和模板使用范围，不允许 host 模板隐式共享。</summary>
    public virtual void CheckUsage(MessageTemplateDefinition template, long senderOwnerId, Guid? tenantId)
    {
        if (template.TenantId != tenantId)
            throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
        if (template.OwnerId != senderOwnerId &&
            (template.Visibility != MessageTemplateVisibility.TenantShared || !tenantId.HasValue))
            throw new UserFriendlyException("无权使用该模板。", ChatErrorCodes.TemplateMessages.TemplateForbidden);
    }

    /// <summary>统一验证草稿并发戳。</summary>
    public virtual void CheckConcurrency(MessageTemplateDefinition template, string stamp)
    {
        if (string.IsNullOrWhiteSpace(stamp) || !string.Equals(template.ConcurrencyStamp, stamp, StringComparison.Ordinal))
            throw new UserFriendlyException("并发冲突，请刷新模板后再试。", ChatErrorCodes.TemplateMessages.TemplateConcurrencyConflict);
    }

    /// <summary>验证后修改草稿，禁止绕过共享范围和字段边界。</summary>
    public virtual void UpdateDraft(MessageTemplateDefinition template, string stamp, string name, string description,
        MessageTemplateVisibility visibility, string definition)
    {
        CheckConcurrency(template, stamp);
        ValidateMetadata(name, description, visibility, template.TenantId);
        var parsed = ValidateDefinitionJson(definition);
        template.UpdateDraft(name, description, visibility, parsed.RawJson);
    }

    public virtual void ValidateMetadata(string name, string description, MessageTemplateVisibility visibility, Guid? tenantId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MessageTemplateConsts.MaxNameLength ||
            (description?.Length ?? 0) > MessageTemplateConsts.MaxDescriptionLength ||
            !Enum.IsDefined(visibility) || (visibility == MessageTemplateVisibility.TenantShared && !tenantId.HasValue))
            throw InvalidDefinition("请检查名称、说明与模板共享范围；host 模板不能设为租户共享。");
    }

    /// <summary>发布不可变版本；调用方在同一事务中先保存模板并发更新再插入版本。</summary>
    public virtual MessageTemplateRevision PreparePublication(MessageTemplateDefinition template, string stamp, Guid revisionId)
    {
        CheckConcurrency(template, stamp);
        var definition = ValidateDefinitionJson(template.DraftJson);
        var version = checked((template.PublishedVersion ?? 0) + 1);
        template.SetPublishedVersion(version);
        return new MessageTemplateRevision(revisionId, template.Id, version, MessageTemplateConsts.SchemaVersion,
            definition.RawJson, ComputeSha256(definition.RawJson), template.TenantId);
    }

    /// <summary>
    /// 校验草稿或发布定义 JSON
    /// </summary>
    public virtual ParsedTemplateDefinition ValidateDefinitionJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new UserFriendlyException("模板定义不能为空。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }

        var byteCount = Encoding.UTF8.GetByteCount(json);
        if (byteCount > MessageTemplateConsts.MaxDefinitionBytes)
        {
            throw new UserFriendlyException($"模板定义大小超过限制({MessageTemplateConsts.MaxDefinitionBytes / 1024} KiB)。", ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new UserFriendlyException($"模板定义 JSON 格式非法: {ex.Message}", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }

        using (doc)
        {
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new UserFriendlyException("模板定义根节点必须为对象。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }

            ValidateDefinitionStructure(root);

            // 1. schemaVersion
            if (!root.TryGetProperty("schemaVersion", out var schemaProp) || schemaProp.ValueKind != JsonValueKind.Number || !schemaProp.TryGetInt32(out var schemaVersion) || schemaVersion != MessageTemplateConsts.SchemaVersion)
            {
                throw new UserFriendlyException($"不支持的协议版本，当前仅支持 schemaVersion={MessageTemplateConsts.SchemaVersion}。", ChatErrorCodes.TemplateMessages.TemplateSchemaUnsupported);
            }

            // 2. name
            if (!root.TryGetProperty("name", out var nameProp) || nameProp.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(nameProp.GetString()))
            {
                throw new UserFriendlyException("模板定义中必须包含非空 name。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }
            var templateName = nameProp.GetString()!;
            if (templateName.Length > MessageTemplateConsts.MaxNameLength)
            {
                throw new UserFriendlyException($"模板名称不能超过 {MessageTemplateConsts.MaxNameLength} 字符。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }

            // 3. fields
            var fields = new Dictionary<string, TemplateFieldDefinition>(StringComparer.Ordinal);
            if (root.TryGetProperty("fields", out var fieldsProp))
            {
                if (fieldsProp.ValueKind != JsonValueKind.Array)
                {
                    throw new UserFriendlyException("fields 必须为数组。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (fieldsProp.GetArrayLength() > MessageTemplateConsts.MaxFields)
                {
                    throw new UserFriendlyException($"fields 数量不能超过 {MessageTemplateConsts.MaxFields} 项。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }

                foreach (var f in fieldsProp.EnumerateArray())
                {
                    if (f.ValueKind != JsonValueKind.Object)
                    {
                        throw new UserFriendlyException("字段声明必须为对象。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    if (!f.TryGetProperty("name", out var fNameProp) || fNameProp.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(fNameProp.GetString()))
                    {
                        throw new UserFriendlyException("字段名不能为空。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    var fname = fNameProp.GetString()!;
                    if (ForbiddenFieldNames.Contains(fname) || !Regex.IsMatch(fname, @"^[a-zA-Z_][a-zA-Z0-9_]{0,63}$"))
                    {
                        throw new UserFriendlyException($"非法字段名: '{fname}'。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    if (fields.ContainsKey(fname))
                    {
                        throw new UserFriendlyException($"重复字段名: '{fname}'。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }

                    if (!f.TryGetProperty("type", out var fTypeProp) || fTypeProp.ValueKind != JsonValueKind.String || !AllowedFieldTypes.Contains(fTypeProp.GetString()!))
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 的类型不受支持，仅支持 string/boolean/integer/number。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    var ftype = fTypeProp.GetString()!;

                    var isRequired = f.TryGetProperty("required", out var reqProp) && reqProp.ValueKind == JsonValueKind.True;

                    int? maxLength = null;
                    if (f.TryGetProperty("maxLength", out var maxLenProp))
                    {
                        if (maxLenProp.ValueKind != JsonValueKind.Number || !maxLenProp.TryGetInt32(out var maxLengthValue) || maxLengthValue <= 0 || maxLengthValue > MessageTemplateConsts.MaxFieldTextLength)
                        {
                            throw new UserFriendlyException($"字段 '{fname}' 的 maxLength 必须在 1～{MessageTemplateConsts.MaxFieldTextLength} 之间。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                        }
                        maxLength = maxLenProp.GetInt32();
                    }

                    string format = null;
                    if (f.TryGetProperty("format", out var formatProp))
                    {
                        if (formatProp.ValueKind != JsonValueKind.String || !AllowedFormats.Contains(formatProp.GetString()!))
                        {
                            throw new UserFriendlyException($"字段 '{fname}' 的 format 仅支持 text 或 url。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                        }
                        format = formatProp.GetString();
                    }

                    List<string> enumValues = null;
                    if (f.TryGetProperty("enum", out var enumProp))
                    {
                        if (enumProp.ValueKind != JsonValueKind.Array)
                        {
                            throw new UserFriendlyException($"字段 '{fname}' 的 enum 必须为数组。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                        }
                        enumValues = enumProp.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).ToList();
                    }

                    fields[fname] = new TemplateFieldDefinition
                    {
                        Name = fname,
                        Type = ftype,
                        Required = isRequired,
                        MaxLength = maxLength,
                        Format = format,
                        Enum = enumValues
                    };
                }
            }

            // 4. actions
            var actions = new Dictionary<string, TemplateActionDefinition>(StringComparer.Ordinal);
            if (root.TryGetProperty("actions", out var actionsProp))
            {
                if (actionsProp.ValueKind != JsonValueKind.Array)
                {
                    throw new UserFriendlyException("actions 必须为数组。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (actionsProp.GetArrayLength() > MessageTemplateConsts.MaxActions)
                {
                    throw new UserFriendlyException($"actions 数量不能超过 {MessageTemplateConsts.MaxActions} 项。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }

                foreach (var a in actionsProp.EnumerateArray())
                {
                    if (a.ValueKind != JsonValueKind.Object)
                    {
                        throw new UserFriendlyException("action 必须为对象。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    if (!a.TryGetProperty("id", out var aIdProp) || aIdProp.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(aIdProp.GetString()))
                    {
                        throw new UserFriendlyException("action 的 id 不能为空。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    var aid = aIdProp.GetString()!;
                    if (ForbiddenFieldNames.Contains(aid) || !Regex.IsMatch(aid, @"^[a-zA-Z_][a-zA-Z0-9_]{0,63}$"))
                        throw InvalidDefinition("action.id 必须为安全标识符。");
                    if (actions.ContainsKey(aid))
                    {
                        throw new UserFriendlyException($"重复 actionId: '{aid}'。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }

                    if (!a.TryGetProperty("type", out var aTypeProp) || aTypeProp.ValueKind != JsonValueKind.String || !AllowedActionTypes.Contains(aTypeProp.GetString()!))
                    {
                        throw new UserFriendlyException($"action '{aid}' 的类型仅支持 openUrl 或 copyText。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    var atype = aTypeProp.GetString()!;

                    if (!a.TryGetProperty("bind", out var aBindProp) || aBindProp.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(aBindProp.GetString()))
                    {
                        throw new UserFriendlyException($"action '{aid}' 必须声明 bind 字段。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    var abind = aBindProp.GetString()!;
                    if (!fields.TryGetValue(abind, out var boundField))
                    {
                        throw new UserFriendlyException($"action '{aid}' 绑定的字段 '{abind}' 未在 fields 中声明。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }

                    if (boundField.Type != "string" || (atype == "openUrl" && boundField.Format != "url"))
                    {
                        throw new UserFriendlyException($"openUrl action '{aid}' 绑定的字段 '{abind}' format 必须为 url。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }

                    actions[aid] = new TemplateActionDefinition { Id = aid, Type = atype, Bind = abind };
                }
            }

            // 5. layout
            if (!root.TryGetProperty("layout", out var layoutProp) || layoutProp.ValueKind != JsonValueKind.Object)
            {
                throw new UserFriendlyException("layout 必须为根对象。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }

            int totalNodes = 0;
            ValidateLayoutNode(layoutProp, depth: 1, ref totalNodes, isRoot: true, fields, actions);

            // 6. summary
            TemplateSummaryDefinition summary = null;
            if (root.TryGetProperty("summary", out var summaryProp) && summaryProp.ValueKind == JsonValueKind.Object)
            {
                var prefix = summaryProp.TryGetProperty("prefix", out var pProp) && pProp.ValueKind == JsonValueKind.String ? pProp.GetString() : null;
                var separator = summaryProp.TryGetProperty("separator", out var sProp) && sProp.ValueKind == JsonValueKind.String ? sProp.GetString() : "：";
                var summaryFields = new List<string>();
                if (summaryProp.TryGetProperty("fields", out var sfProp) && sfProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in sfProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.String || !fields.ContainsKey(item.GetString()!))
                            throw InvalidDefinition("summary.fields 引用了未声明字段。");
                        summaryFields.Add(item.GetString()!);
                    }
                }
                summary = new TemplateSummaryDefinition { Prefix = prefix, Separator = separator, Fields = summaryFields };
            }

            var normalizedJson = JsonSerializer.Serialize(root);
            if (Encoding.UTF8.GetByteCount(normalizedJson) > MessageTemplateConsts.MaxDefinitionBytes)
                throw new UserFriendlyException("规范化后的模板定义超过大小限制。", ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge);

            return new ParsedTemplateDefinition
            {
                Name = templateName,
                Fields = fields,
                Actions = actions,
                Summary = summary,
                RawJson = normalizedJson
            };
        }
    }

    private static UserFriendlyException InvalidDefinition(string message) =>
        new(message, ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);

    private static void CheckProperties(JsonElement value, params string[] allowed)
    {
        if (value.ValueKind != JsonValueKind.Object) throw InvalidDefinition("定义项必须是对象。");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
            if (!seen.Add(property.Name) || !allowed.Contains(property.Name))
                throw InvalidDefinition($"不支持或重复的属性 '{property.Name}'。");
    }

    private static void CheckText(JsonElement value, string property, bool required = false, int limit = MessageTemplateConsts.MaxStaticTextLength)
    {
        if (!value.TryGetProperty(property, out var item))
        {
            if (required) throw InvalidDefinition($"缺少属性 '{property}'。");
            return;
        }
        if (item.ValueKind != JsonValueKind.String || item.GetString()!.Length > limit ||
            (required && string.IsNullOrWhiteSpace(item.GetString())))
            throw InvalidDefinition($"属性 '{property}' 必须是长度不超过 {limit} 的文本。");
    }

    private static void ValidateDefinitionStructure(JsonElement root)
    {
        CheckProperties(root, "schemaVersion", "name", "fields", "summary", "layout", "actions", "resources");
        // Blob 引用保活尚未接入；首期只开放 URL 图片，拒绝伪造资源及生命周期不完整的 Blob。
        if (root.TryGetProperty("resources", out var resources) &&
            (resources.ValueKind != JsonValueKind.Array || resources.GetArrayLength() != 0))
            throw InvalidDefinition("首期暂不支持 Blob 素材，请使用已授权的长期图片 URL。");
        if (root.TryGetProperty("fields", out var fields))
        {
            if (fields.ValueKind != JsonValueKind.Array) throw InvalidDefinition("fields 必须为数组。");
            foreach (var field in fields.EnumerateArray())
            {
                CheckProperties(field, "name", "type", "required", "maxLength", "enum", "format");
                CheckText(field, "name", true, MessageTemplateConsts.MaxNameLength);
                CheckText(field, "type", true);
                CheckText(field, "format");
                if (field.TryGetProperty("required", out var required) && required.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    throw InvalidDefinition("required 必须为布尔值。");
                var isString = field.GetProperty("type").GetString() == "string";
                if (!isString && (field.TryGetProperty("maxLength", out _) || field.TryGetProperty("format", out _) || field.TryGetProperty("enum", out _)))
                    throw InvalidDefinition("maxLength、format、enum 只适用于字符串字段。");
                if (field.TryGetProperty("enum", out var values))
                {
                    if (values.ValueKind != JsonValueKind.Array || values.GetArrayLength() == 0 ||
                        values.EnumerateArray().Any(x => x.ValueKind != JsonValueKind.String || x.GetString()!.Length > MessageTemplateConsts.MaxFieldTextLength))
                        throw InvalidDefinition("enum 必须为非空的字符串数组。");
                }
            }
        }
        if (root.TryGetProperty("actions", out var actions))
        {
            if (actions.ValueKind != JsonValueKind.Array) throw InvalidDefinition("actions 必须为数组。");
            foreach (var action in actions.EnumerateArray())
            {
                CheckProperties(action, "id", "type", "bind");
                CheckText(action, "id", true, MessageTemplateConsts.MaxNameLength);
                CheckText(action, "type", true);
                CheckText(action, "bind", true, MessageTemplateConsts.MaxNameLength);
            }
        }
        if (root.TryGetProperty("summary", out var summary))
        {
            CheckProperties(summary, "prefix", "fields", "separator");
            CheckText(summary, "prefix");
            CheckText(summary, "separator");
            if (summary.TryGetProperty("fields", out var summaryFields) &&
                (summaryFields.ValueKind != JsonValueKind.Array || summaryFields.GetArrayLength() > MessageTemplateConsts.MaxFields))
                throw InvalidDefinition("summary.fields 必须为有界数组。");
        }
        if (root.TryGetProperty("layout", out var layout))
        {
            var images = 0;
            ValidateLayoutShape(layout, 1, ref images);
        }
    }

    private static void ValidateLayoutShape(JsonElement node, int depth, ref int images)
    {
        if (depth > MessageTemplateConsts.MaxDepth) throw InvalidDefinition("布局嵌套深度超过限制。");
        if (node.ValueKind != JsonValueKind.Object) throw InvalidDefinition("节点必须为对象。");
        CheckText(node, "type", true);
        var type = node.GetProperty("type").GetString();
        var allowed = type switch
        {
            "card" => new[] { "type", "children" },
            "row" or "column" => new[] { "type", "children", "gap" },
            "text" => new[] { "type", "text", "bind", "style" },
            "field" => new[] { "type", "label", "bind" },
            "badge" => new[] { "type", "text", "bind", "tone" },
            "image" => new[] { "type", "bind", "aspectRatio" },
            "divider" => new[] { "type" },
            "button" => new[] { "type", "label", "actionId" },
            _ => throw InvalidDefinition("未知布局组件。")
        };
        CheckProperties(node, allowed);
        foreach (var property in new[] { "text", "bind", "label", "style", "tone", "gap", "aspectRatio", "actionId" })
            CheckText(node, property);
        if (type == "image" && ++images > MessageTemplateConsts.MaxResources)
            throw InvalidDefinition("图片数量不能超过3张。");
        if (node.TryGetProperty("children", out var children))
        {
            if (children.ValueKind != JsonValueKind.Array) throw InvalidDefinition("children 必须为数组。");
            foreach (var child in children.EnumerateArray()) ValidateLayoutShape(child, depth + 1, ref images);
        }
    }

    private void ValidateLayoutNode(
        JsonElement node,
        int depth,
        ref int totalNodes,
        bool isRoot,
        Dictionary<string, TemplateFieldDefinition> fields,
        Dictionary<string, TemplateActionDefinition> actions)
    {
        totalNodes++;
        if (totalNodes > MessageTemplateConsts.MaxNodes)
        {
            throw new UserFriendlyException($"布局节点数超过限制 (最大 {MessageTemplateConsts.MaxNodes})。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }
        if (depth > MessageTemplateConsts.MaxDepth)
        {
            throw new UserFriendlyException($"布局嵌套深度超过限制 (最大 {MessageTemplateConsts.MaxDepth})。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }

        if (node.ValueKind != JsonValueKind.Object)
        {
            throw new UserFriendlyException("布局节点必须为对象。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }

        if (!node.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String)
        {
            throw new UserFriendlyException("节点缺少 type 属性。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }
        var type = typeProp.GetString()!;

        if (isRoot)
        {
            if (type != "card")
            {
                throw new UserFriendlyException("根节点 type 必须为 card。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }
        }
        else
        {
            if (type == "card")
            {
                throw new UserFriendlyException("非根节点禁止使用 card。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }
        }

        if (!AllowedNodeTypes.Contains(type))
        {
            throw new UserFriendlyException($"未知节点类型: '{type}'。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
        }

        // bind check for container types
        if (type is "card" or "column" or "row")
        {
            if (node.TryGetProperty("bind", out _))
            {
                throw new UserFriendlyException($"容器节点 '{type}' 不能包含 bind 属性。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
            }
        }

        // Check attributes per type
        switch (type)
        {
            case "card":
            case "column":
                if (type == "column" && node.TryGetProperty("gap", out var colGap) && !AllowedGaps.Contains(colGap.GetString() ?? ""))
                {
                    throw new UserFriendlyException($"column 的 gap 仅支持 small/medium/large。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (node.TryGetProperty("children", out var colChildren))
                {
                    if (colChildren.ValueKind != JsonValueKind.Array) throw new UserFriendlyException("children 必须为数组。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    foreach (var child in colChildren.EnumerateArray())
                    {
                        ValidateLayoutNode(child, depth + 1, ref totalNodes, false, fields, actions);
                    }
                }
                break;

            case "row":
                if (node.TryGetProperty("gap", out var rowGap) && !AllowedGaps.Contains(rowGap.GetString() ?? ""))
                {
                    throw new UserFriendlyException($"row 的 gap 仅支持 small/medium/large。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (node.TryGetProperty("children", out var rowChildren))
                {
                    if (rowChildren.ValueKind != JsonValueKind.Array) throw new UserFriendlyException("children 必须为数组。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    if (rowChildren.GetArrayLength() > 3)
                    {
                        throw new UserFriendlyException("row 最多包含 3 个子节点。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                    }
                    foreach (var child in rowChildren.EnumerateArray())
                    {
                        ValidateLayoutNode(child, depth + 1, ref totalNodes, false, fields, actions);
                    }
                }
                break;

            case "text":
                var hasText = node.TryGetProperty("text", out var tProp) && tProp.ValueKind == JsonValueKind.String;
                var hasBind = node.TryGetProperty("bind", out var tbProp) && tbProp.ValueKind == JsonValueKind.String;
                if (hasText == hasBind)
                {
                    throw new UserFriendlyException("text 节点必须且只能提供 text 或 bind 二者之一。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (hasText && tProp.GetString()!.Length > MessageTemplateConsts.MaxStaticTextLength)
                {
                    throw new UserFriendlyException($"静态文本不能超过 {MessageTemplateConsts.MaxStaticTextLength} 字符。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (hasBind && !fields.ContainsKey(tbProp.GetString()!))
                {
                    throw new UserFriendlyException($"text 节点绑定的字段 '{tbProp.GetString()}' 未声明。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (node.TryGetProperty("style", out var styleProp) && !AllowedStyles.Contains(styleProp.GetString() ?? ""))
                {
                    throw new UserFriendlyException("text 的 style 仅支持 heading/body/secondary。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                break;

            case "field":
                if (!node.TryGetProperty("label", out var lProp) || lProp.ValueKind != JsonValueKind.String)
                {
                    throw new UserFriendlyException("field 节点必须提供 label。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (!node.TryGetProperty("bind", out var fbProp) || fbProp.ValueKind != JsonValueKind.String || !fields.ContainsKey(fbProp.GetString()!))
                {
                    throw new UserFriendlyException($"field 节点绑定的字段非法或未声明。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                break;

            case "badge":
                var hasBText = node.TryGetProperty("text", out var btProp) && btProp.ValueKind == JsonValueKind.String;
                var hasBBind = node.TryGetProperty("bind", out var bbProp) && bbProp.ValueKind == JsonValueKind.String;
                if (hasBText == hasBBind)
                {
                    throw new UserFriendlyException("badge 节点必须且只能提供 text 或 bind 二者之一。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (hasBBind && !fields.ContainsKey(bbProp.GetString()!))
                {
                    throw new UserFriendlyException($"badge 绑定的字段 '{bbProp.GetString()}' 未声明。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (node.TryGetProperty("tone", out var toneProp) && !AllowedTones.Contains(toneProp.GetString() ?? ""))
                {
                    throw new UserFriendlyException("badge 的 tone 仅支持 neutral/success/warning/danger。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                break;

            case "image":
                var hasImgBind = node.TryGetProperty("bind", out var ibProp) && ibProp.ValueKind == JsonValueKind.String;
                var hasImgRes = node.TryGetProperty("resourceId", out var irProp) && irProp.ValueKind == JsonValueKind.String;
                if (!hasImgBind && !hasImgRes)
                {
                    throw new UserFriendlyException("image 节点必须提供 bind 或 resourceId。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (hasImgBind && (!fields.TryGetValue(ibProp.GetString()!, out var fdef) || fdef.Type != "string" || fdef.Format != "url"))
                {
                    throw new UserFriendlyException($"image 节点绑定的字段 '{ibProp.GetString()}' 必须为 format:url 的字段。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (node.TryGetProperty("aspectRatio", out var arProp) && !AllowedAspectRatios.Contains(arProp.GetString() ?? ""))
                {
                    throw new UserFriendlyException("image 的 aspectRatio 仅支持 square/landscape/portrait。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                break;

            case "divider":
                break;

            case "button":
                if (!node.TryGetProperty("label", out var btnLProp) || btnLProp.ValueKind != JsonValueKind.String)
                {
                    throw new UserFriendlyException("button 节点必须提供 label。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                if (!node.TryGetProperty("actionId", out var btnAProp) || btnAProp.ValueKind != JsonValueKind.String || !actions.ContainsKey(btnAProp.GetString()!))
                {
                    throw new UserFriendlyException($"button 节点引用的 actionId '{btnAProp.GetString()}' 未声明。", ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid);
                }
                break;
        }
    }

    /// <summary>
    /// 校验发送数据并过滤出白名单字段
    /// </summary>
    public virtual Dictionary<string, object> ValidateAndSanitizeData(
        Dictionary<string, object> inputData,
        ParsedTemplateDefinition definition)
    {
        if (inputData == null)
        {
            throw new UserFriendlyException("消息数据不能为空。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
        }

        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var key in inputData.Keys)
            if (!definition.Fields.ContainsKey(key))
                throw InvalidData(key, "未声明字段");

        foreach (var (name, field) in definition.Fields)
        {
            if (!inputData.TryGetValue(name, out var raw) || raw == null ||
                raw is JsonElement missing && missing.ValueKind == JsonValueKind.Null)
            {
                if (field.Required) throw InvalidData(name, "必填字段缺失");
                continue;
            }
            JsonElement value;
            try { value = raw is JsonElement json ? json : JsonSerializer.SerializeToElement(raw); }
            catch (Exception ex) when (ex is JsonException or ArgumentException or NotSupportedException)
            { throw InvalidData(name, "字段类型无效"); }
            switch (field.Type)
            {
                case "string":
                    if (value.ValueKind != JsonValueKind.String) throw InvalidData(name, "必须为字符串");
                    var text = value.GetString()!;
                    if (text.Length > (field.MaxLength ?? MessageTemplateConsts.MaxFieldTextLength))
                        throw InvalidData(name, "超过长度上限");
                    if (field.Enum != null && !field.Enum.Contains(text)) throw InvalidData(name, "不在允许枚举中");
                    if (field.Format == "url") ValidateSafeWebUrl(text, name);
                    result[name] = text;
                    break;
                case "boolean":
                    if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw InvalidData(name, "必须为布尔值");
                    result[name] = value.GetBoolean();
                    break;
                case "integer":
                    if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out var integer) ||
                        !double.IsFinite(integer) || Math.Abs(integer) > 9007199254740991d || Math.Truncate(integer) != integer)
                        throw InvalidData(name, "必须为 JavaScript 安全范围内的整数");
                    result[name] = (long)integer;
                    break;
                case "number":
                    if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out var number) || !double.IsFinite(number) || Math.Abs(number) > 9007199254740991d)
                        throw InvalidData(name, "必须为有限数值");
                    result[name] = number;
                    break;
                default:
                    throw InvalidData(name, "不支持的类型");
            }
        }

        return result;
    }

    private static UserFriendlyException InvalidData(string field, string rule) =>
        new($"字段 '{field}': {rule}。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);

    /// <summary>
    /// 生成降级与通知摘要文本
    /// </summary>
    public virtual string GenerateFallbackText(
        ParsedTemplateDefinition definition,
        Dictionary<string, object> sanitizedData)
    {
        var sb = new StringBuilder();
        if (definition.Summary != null)
        {
            if (!string.IsNullOrWhiteSpace(definition.Summary.Prefix))
            {
                sb.Append(definition.Summary.Prefix);
            }
            if (definition.Summary.Fields != null && definition.Summary.Fields.Count > 0)
            {
                var parts = new List<string>();
                foreach (var field in definition.Summary.Fields)
                {
                    if (sanitizedData.TryGetValue(field, out var val) && val != null)
                    {
                        if (val is bool b)
                        {
                            parts.Add(b ? "是" : "否");
                        }
                        else if (val is double d)
                        {
                            parts.Add(d.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            parts.Add(Convert.ToString(val, CultureInfo.InvariantCulture) ?? "");
                        }
                    }
                }
                var sep = definition.Summary.Separator ?? "：";
                sb.Append(string.Join(sep, parts));
            }
        }

        if (sb.Length == 0)
        {
            sb.Append($"[{definition.Name}]");
        }

        // Clean control characters and truncate to 256
        var clean = Regex.Replace(sb.ToString(), @"<[^>]*>", "");
        clean = Regex.Replace(clean, @"[\p{C}\s]+", " ").Trim();
        if (clean.Length > MessageTemplateConsts.MaxFallbackTextLength)
        {
            clean = clean[..MessageTemplateConsts.MaxFallbackTextLength];
        }
        return clean;
    }

    /// <summary>
    /// 构建快照对象
    /// </summary>
    public virtual TemplateContentInfo BuildSnapshot(
        Guid templateId,
        int templateVersion,
        ParsedTemplateDefinition definition,
        Dictionary<string, object> sanitizedData,
        string fallbackText)
    {
        using var doc = JsonDocument.Parse(definition.RawJson);
        var root = doc.RootElement;

        object layoutObj = root.TryGetProperty("layout", out var l) ? JsonSerializer.Deserialize<object>(l.GetRawText()) : null;
        object actionsObj = root.TryGetProperty("actions", out var a) ? JsonSerializer.Deserialize<object>(a.GetRawText()) : null;
        object resourcesObj = root.TryGetProperty("resources", out var r) ? JsonSerializer.Deserialize<object>(r.GetRawText()) : null;

        var snapshot = new TemplateContentInfo
        {
            SchemaVersion = MessageTemplateConsts.SchemaVersion,
            TemplateId = templateId,
            TemplateVersion = templateVersion,
            TemplateName = definition.Name,
            FallbackText = fallbackText,
            Data = sanitizedData,
            Layout = layoutObj,
            Actions = actionsObj,
            Resources = resourcesObj
        };

        var json = JsonSerializer.Serialize(snapshot);
        var bytes = Encoding.UTF8.GetByteCount(json);
        if (bytes > MessageTemplateConsts.MaxSnapshotBytes)
        {
            throw new UserFriendlyException($"生成的消息快照大小超过限制({MessageTemplateConsts.MaxSnapshotBytes / 1024} KiB)。", ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge);
        }

        return snapshot;
    }

    /// <summary>
    /// 计算定义字符串 SHA-256
    /// </summary>
    public virtual string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void ValidateSafeWebUrl(string url, string fieldName)
    {
        if (url.Any(char.IsControl) || url.Contains('\\') || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new UserFriendlyException($"字段 '{fieldName}' 不是合法的绝对 URL 地址。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
        }
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new UserFriendlyException($"字段 '{fieldName}' 的协议仅支持 http 或 https，禁止 '{uri.Scheme}'。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
        }
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new UserFriendlyException($"字段 '{fieldName}' 禁止包含凭据信息。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
        }
    }
}

public class ParsedTemplateDefinition
{
    public string Name { get; set; }
    public Dictionary<string, TemplateFieldDefinition> Fields { get; set; } = [];
    public Dictionary<string, TemplateActionDefinition> Actions { get; set; } = [];
    public TemplateSummaryDefinition Summary { get; set; }
    public string RawJson { get; set; }
}

public class TemplateFieldDefinition
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }
    public int? MaxLength { get; set; }
    public string Format { get; set; }
    public List<string> Enum { get; set; }
}

public class TemplateActionDefinition
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Bind { get; set; }
}

public class TemplateSummaryDefinition
{
    public string Prefix { get; set; }
    public string Separator { get; set; }
    public List<string> Fields { get; set; }
}
