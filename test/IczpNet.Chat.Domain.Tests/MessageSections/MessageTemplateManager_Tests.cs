using System;
using System.Collections.Generic;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageTemplates;
using Volo.Abp;
using Xunit;

namespace IczpNet.Chat.Domain.Tests.MessageSections;

public class MessageTemplateManager_Tests
{
    private readonly MessageTemplateManager _manager = new();

    private const string ValidDefinitionJson = """
    {
      "schemaVersion": 1,
      "name": "维修工单",
      "fields": [
        { "name": "title", "type": "string", "required": true, "maxLength": 80 },
        { "name": "orderNo", "type": "string", "required": true, "maxLength": 64 },
        { "name": "status", "type": "string", "required": true, "enum": ["待处理", "处理中", "已完成"] },
        { "name": "detailUrl", "type": "string", "required": false, "maxLength": 500, "format": "url" }
      ],
      "summary": { "prefix": "[维修工单]", "fields": ["orderNo", "status"], "separator": "：" },
      "layout": {
        "type": "card",
        "children": [
          { "type": "text", "bind": "title", "style": "heading" },
          { "type": "field", "label": "工单编号", "bind": "orderNo" },
          { "type": "badge", "bind": "status", "tone": "neutral" },
          { "type": "button", "label": "查看详情", "actionId": "detail" }
        ]
      },
      "actions": [
        { "id": "detail", "type": "openUrl", "bind": "detailUrl" }
      ]
    }
    """;

    [Fact]
    public void ValidateDefinitionJson_Should_Succeed_For_Valid_Json()
    {
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);

        Assert.Equal("维修工单", parsed.Name);
        Assert.Equal(4, parsed.Fields.Count);
        Assert.Single(parsed.Actions);
        Assert.Equal("[维修工单]", parsed.Summary.Prefix);
    }

    [Fact]
    public void ValidateDefinitionJson_Should_Throw_When_SchemaVersion_Not_1()
    {
        var json = """{ "schemaVersion": 2, "name": "测试", "layout": { "type": "card" } }""";
        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(json));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateSchemaUnsupported, ex.Code);
    }

    [Fact]
    public void ValidateDefinitionJson_Should_Throw_When_Root_Not_Card()
    {
        var json = """{ "schemaVersion": 1, "name": "测试", "layout": { "type": "column" } }""";
        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(json));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid, ex.Code);
    }

    [Fact]
    public void ValidateDefinitionJson_Should_Throw_When_Forbidden_FieldName()
    {
        var json = """
        {
          "schemaVersion": 1,
          "name": "测试",
          "fields": [
            { "name": "__proto__", "type": "string", "required": true }
          ],
          "layout": { "type": "card" }
        }
        """;
        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(json));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid, ex.Code);
    }

    [Fact]
    public void ValidateDefinitionJson_Should_Throw_When_Depth_Exceeds_6()
    {
        var json = """
        {
          "schemaVersion": 1,
          "name": "超深测试",
          "layout": {
            "type": "card",
            "children": [{
              "type": "column",
              "children": [{
                "type": "column",
                "children": [{
                  "type": "column",
                  "children": [{
                    "type": "column",
                    "children": [{
                      "type": "column",
                      "children": [{
                        "type": "column",
                        "children": [{ "type": "divider" }]
                      }]
                    }]
                  }]
                }]
              }]
            }]
          }
        }
        """;
        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(json));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid, ex.Code);
    }

    [Fact]
    public void ValidateAndSanitizeData_Should_Filter_And_Validate_Correctly()
    {
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);

        var input = new Dictionary<string, object>
        {
            ["title"] = "空调报修",
            ["orderNo"] = "WX001",
            ["status"] = "待处理",
            ["detailUrl"] = "https://example.com/tickets/1"
        };

        var sanitized = _manager.ValidateAndSanitizeData(input, parsed);

        Assert.Equal("空调报修", sanitized["title"]);
        Assert.Equal("WX001", sanitized["orderNo"]);
        Assert.Equal("待处理", sanitized["status"]);
        Assert.Equal("https://example.com/tickets/1", sanitized["detailUrl"]);
    }

    [Fact]
    public void ValidateAndSanitizeData_Should_Reject_Missing_Required_Field()
    {
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);

        var input = new Dictionary<string, object>
        {
            ["title"] = "空调报修"
            // orderNo and status are missing!
        };

        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateAndSanitizeData(input, parsed));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDataInvalid, ex.Code);
    }

    [Fact]
    public void ValidateAndSanitizeData_Should_Reject_Unknown_Field()
    {
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);

        var input = new Dictionary<string, object>
        {
            ["title"] = "空调报修",
            ["orderNo"] = "WX001",
            ["status"] = "待处理",
            ["extraField"] = "unexpected"
        };

        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateAndSanitizeData(input, parsed));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDataInvalid, ex.Code);
    }

    [Fact]
    public void ValidateAndSanitizeData_Should_Reject_Dangerous_Url()
    {
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);

        var input = new Dictionary<string, object>
        {
            ["title"] = "空调报修",
            ["orderNo"] = "WX001",
            ["status"] = "待处理",
            ["detailUrl"] = "javascript:alert(1)"
        };

        var ex = Assert.Throws<UserFriendlyException>(() => _manager.ValidateAndSanitizeData(input, parsed));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDataInvalid, ex.Code);
    }

    [Fact]
    public void GenerateFallbackText_Should_Format_Summary_Correctly()
    {
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);
        var input = new Dictionary<string, object>
        {
            ["title"] = "空调报修",
            ["orderNo"] = "WX001",
            ["status"] = "待处理"
        };
        var sanitized = _manager.ValidateAndSanitizeData(input, parsed);
        var fallback = _manager.GenerateFallbackText(parsed, sanitized);

        Assert.Equal("[维修工单]WX001：待处理", fallback);
    }

    [Fact]
    public void BuildSnapshot_Should_Create_Valid_Snapshot()
    {
        var templateId = Guid.NewGuid();
        var parsed = _manager.ValidateDefinitionJson(ValidDefinitionJson);
        var input = new Dictionary<string, object>
        {
            ["title"] = "空调报修",
            ["orderNo"] = "WX001",
            ["status"] = "待处理"
        };
        var sanitized = _manager.ValidateAndSanitizeData(input, parsed);
        var fallback = _manager.GenerateFallbackText(parsed, sanitized);

        var snapshot = _manager.BuildSnapshot(templateId, 1, parsed, sanitized, fallback);

        Assert.Equal(1, snapshot.SchemaVersion);
        Assert.Equal(templateId, snapshot.TemplateId);
        Assert.Equal(1, snapshot.TemplateVersion);
        Assert.Equal("维修工单", snapshot.TemplateName);
        Assert.Equal("[维修工单]WX001：待处理", snapshot.FallbackText);
        Assert.NotNull(snapshot.Layout);
        Assert.NotNull(snapshot.Actions);
    }
}
