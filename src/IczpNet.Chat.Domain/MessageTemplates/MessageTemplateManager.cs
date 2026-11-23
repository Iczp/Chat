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

            // 1. schemaVersion
            if (!root.TryGetProperty("schemaVersion", out var schemaProp) || schemaProp.ValueKind != JsonValueKind.Number || schemaProp.GetInt32() != MessageTemplateConsts.SchemaVersion)
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
                    if (ForbiddenFieldNames.Contains(fname) || !Regex.IsMatch(fname, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
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
                        if (maxLenProp.ValueKind != JsonValueKind.Number || maxLenProp.GetInt32() <= 0 || maxLenProp.GetInt32() > MessageTemplateConsts.MaxFieldTextLength)
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

                    if (atype == "openUrl" && boundField.Format != "url")
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
                        if (item.ValueKind == JsonValueKind.String && fields.ContainsKey(item.GetString()!))
                        {
                            summaryFields.Add(item.GetString()!);
                        }
                    }
                }
                summary = new TemplateSummaryDefinition { Prefix = prefix, Separator = separator, Fields = summaryFields };
            }

            return new ParsedTemplateDefinition
            {
                Name = templateName,
                Fields = fields,
                Actions = actions,
                Summary = summary,
                RawJson = json
            };
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
                if (hasImgBind && (!fields.TryGetValue(ibProp.GetString()!, out var fdef) || fdef.Format != "url"))
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

        // Check for unknown fields (additionalProperties = false)
        foreach (var key in inputData.Keys)
        {
            if (!definition.Fields.ContainsKey(key))
            {
                throw new UserFriendlyException($"提交了未在模板中声明的未知字段: '{key}'。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
            }
        }

        foreach (var (fname, fdef) in definition.Fields)
        {
            if (!inputData.TryGetValue(fname, out var rawVal) || rawVal == null)
            {
                if (fdef.Required)
                {
                    throw new UserFriendlyException($"必填字段 '{fname}' 缺失。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                }
                continue;
            }

            // Type check and normalization
            switch (fdef.Type)
            {
                case "string":
                    var strVal = rawVal is JsonElement je && je.ValueKind == JsonValueKind.String
                        ? je.GetString()!
                        : rawVal.ToString()!;

                    if (fdef.MaxLength.HasValue && strVal.Length > fdef.MaxLength.Value)
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 长度超出最大限制 ({fdef.MaxLength.Value})。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                    }
                    if (strVal.Length > MessageTemplateConsts.MaxFieldTextLength)
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 长度超出系统上限 ({MessageTemplateConsts.MaxFieldTextLength})。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                    }
                    if (fdef.Enum != null && fdef.Enum.Count > 0 && !fdef.Enum.Contains(strVal))
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 的值不在允许的枚举列表中。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                    }
                    if (fdef.Format == "url")
                    {
                        ValidateSafeWebUrl(strVal, fname);
                    }
                    result[fname] = strVal;
                    break;

                case "boolean":
                    bool boolVal;
                    if (rawVal is JsonElement jb && (jb.ValueKind == JsonValueKind.True || jb.ValueKind == JsonValueKind.False))
                    {
                        boolVal = jb.GetBoolean();
                    }
                    else if (rawVal is bool b)
                    {
                        boolVal = b;
                    }
                    else if (bool.TryParse(rawVal.ToString(), out var parsedB))
                    {
                        boolVal = parsedB;
                    }
                    else
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 必须为布尔值。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                    }
                    result[fname] = boolVal;
                    break;

                case "integer":
                    long intVal;
                    if (rawVal is JsonElement ji && ji.ValueKind == JsonValueKind.Number && ji.TryGetInt64(out var parsedLong))
                    {
                        intVal = parsedLong;
                    }
                    else if (long.TryParse(rawVal.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var pl))
                    {
                        intVal = pl;
                    }
                    else
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 必须为整数。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                    }
                    result[fname] = intVal;
                    break;

                case "number":
                    double numVal;
                    if (rawVal is JsonElement jn && jn.ValueKind == JsonValueKind.Number && jn.TryGetDouble(out var parsedDouble))
                    {
                        numVal = parsedDouble;
                    }
                    else if (double.TryParse(rawVal.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var pd))
                    {
                        numVal = pd;
                    }
                    else
                    {
                        throw new UserFriendlyException($"字段 '{fname}' 必须为数值。", ChatErrorCodes.TemplateMessages.TemplateDataInvalid);
                    }
                    result[fname] = numVal;
                    break;
            }
        }

        return result;
    }

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
                            parts.Add(val.ToString() ?? "");
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
        var clean = Regex.Replace(sb.ToString(), @"[\p{C}-[\r\n\t]]", "");
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
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
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
