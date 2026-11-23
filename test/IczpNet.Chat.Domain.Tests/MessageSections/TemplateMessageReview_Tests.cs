using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageTemplates;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace IczpNet.Chat.Domain.Tests.MessageSections;

public class TemplateMessageReview_Tests
{
    private readonly MessageTemplateManager _manager = new();
    private const string Minimal = """{"schemaVersion":1,"name":"通知","fields":[],"layout":{"type":"card","children":[]}}""";

    [Theory]
    [InlineData("01-minimal-card.json")]
    [InlineData("02-full-card.json")]
    [InlineData("03-optional-field-missing.json")]
    [InlineData("04-unknown-component.json")]
    [InlineData("05-future-schema-version.json")]
    [InlineData("06-illegal-url.json")]
    [InlineData("07-too-deep-layout.json")]
    [InlineData("08-too-long-text.json")]
    [InlineData("09-corrupted-payload.json")]
    public void Authoritative_fixtures_validate_real_definition_and_data(string filename)
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
            "MessageSections", "Fixtures", "template-message", filename)));
        var root = fixture.RootElement;
        void Build()
        {
            var definitionJson = root.GetProperty("definition");
            var definition = _manager.ValidateDefinitionJson(definitionJson.ValueKind == JsonValueKind.String
                ? definitionJson.GetString() : definitionJson.GetRawText());
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(root.GetProperty("data").GetRawText());
            var sanitized = _manager.ValidateAndSanitizeData(data, definition);
            var fallback = _manager.GenerateFallbackText(definition, sanitized);
            var snapshot = _manager.BuildSnapshot(Guid.NewGuid(), 1, definition, sanitized, fallback);
            Assert.Equal(root.GetProperty("clientExpectation").GetProperty("expectedFallbackText").GetString(), snapshot.FallbackText);
        }
        var expectation = root.GetProperty("serverExpectation");
        if (expectation.GetProperty("accepted").GetBoolean()) Build();
        else
        {
            var exception = Assert.Throws<UserFriendlyException>(Build);
            Assert.EndsWith(expectation.GetProperty("errorCode").GetString()!.Replace("Template", ""), exception.Code);
        }
    }

    [Theory]
    [InlineData("\"string\"", "42")]
    [InlineData("\"string\"", "{\"secret\":\"nested\"}")]
    [InlineData("\"string\"", "[1,2]")]
    [InlineData("\"boolean\"", "\"true\"")]
    [InlineData("\"integer\"", "\"12\"")]
    [InlineData("\"integer\"", "9007199254740992")]
    [InlineData("\"integer\"", "1.1")]
    [InlineData("\"number\"", "\"NaN\"")]
    public void Data_does_not_coerce_wrong_types_or_unsafe_numbers(string type, string value)
    {
        var definition = _manager.ValidateDefinitionJson("{\"schemaVersion\":1,\"name\":\"test\",\"fields\":[{\"name\":\"value\",\"type\":" + type + ",\"required\":true}],\"layout\":{\"type\":\"card\"}}");
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>("{\"value\":" + value + "}");
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDataInvalid,
            Assert.Throws<UserFriendlyException>(() => _manager.ValidateAndSanitizeData(data, definition)).Code);
    }

    [Theory]
    [InlineData("{\"type\":\"text\",\"text\":\"ok\",\"onclick\":\"evil\"}")]
    [InlineData("{\"type\":\"badge\",\"text\":\"ok\",\"tone\":1}")]
    [InlineData("{\"type\":\"divider\",\"children\":[]}")]
    [InlineData("{\"type\":\"image\",\"resourceId\":\"forged\"}")]
    [InlineData("{\"type\":\"text\",\"type\":\"divider\",\"text\":\"duplicate\"}")]
    public void Invalid_node_properties_are_rejected_with_a_validation_error(string node)
    {
        var json = "{\"schemaVersion\":1,\"name\":\"test\",\"layout\":{\"type\":\"card\",\"children\":[" + node + "]}}";
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDefinitionInvalid,
            Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(json)).Code);
    }

    [Fact]
    public void Images_and_raw_blob_resources_have_hard_limits()
    {
        var definition = """{"schemaVersion":1,"name":"test","fields":[{"name":"url","type":"string","format":"url"}],"layout":{"type":"card","children":[IMAGES]}}""";
        var json = definition.Replace("IMAGES", string.Join(",", Enumerable.Repeat("{\"type\":\"image\",\"bind\":\"url\"}", 4)));
        Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(json));
        Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(Minimal.Replace("\"fields\":[]", "\"resources\":[{\"id\":\"blob\",\"blobId\":\"forged\"}],\"fields\":[]")));
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("999999999999999999999")]
    public void Malformed_numeric_version_returns_validation_not_server_error(string version)
    {
        var exception = Assert.Throws<UserFriendlyException>(() => _manager.ValidateDefinitionJson(Minimal.Replace("\"schemaVersion\":1", "\"schemaVersion\":" + version)));
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateSchemaUnsupported, exception.Code);
    }

    [Fact]
    public void Feature_is_disabled_by_default_and_only_explicit_true_enables_it()
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConfiguration>().Returns(new ConfigurationBuilder().Build());
        var manager = new MessageTemplateManager { LazyServiceProvider = lazy };
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateDisabled, Assert.Throws<UserFriendlyException>(manager.EnsureEnabled).Code);
        lazy.LazyGetRequiredService<IConfiguration>().Returns(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string> { ["Chat:TemplateMessages:Enabled"] = "true" }).Build());
        manager.EnsureEnabled();
    }

    [Fact]
    public void Private_host_shared_and_foreign_tenant_usage_are_separate()
    {
        var tenant = Guid.NewGuid();
        var own = new MessageTemplateDefinition(Guid.NewGuid(), 1, "test", null, MessageTemplateVisibility.Private, Minimal, tenant);
        _manager.CheckUsage(own, 1, tenant);
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateForbidden, Assert.Throws<UserFriendlyException>(() => _manager.CheckUsage(own, 2, tenant)).Code);
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateNotFound, Assert.Throws<UserFriendlyException>(() => _manager.CheckUsage(own, 1, Guid.NewGuid())).Code);
        own.UpdateDraft("test", null, MessageTemplateVisibility.TenantShared, Minimal);
        _manager.CheckUsage(own, 2, tenant);
        var host = new MessageTemplateDefinition(Guid.NewGuid(), 1, "test", null, MessageTemplateVisibility.TenantShared, Minimal);
        Assert.Throws<UserFriendlyException>(() => _manager.CheckUsage(host, 2, null));
        Assert.Throws<UserFriendlyException>(() => _manager.ValidateMetadata("test", null, MessageTemplateVisibility.TenantShared, null));
    }

    [Fact]
    public void Publication_validates_stamp_and_freezes_definition()
    {
        var template = new MessageTemplateDefinition(Guid.NewGuid(), 1, "test", null, MessageTemplateVisibility.Private, Minimal);
        template.ConcurrencyStamp = "version-1";
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplateConcurrencyConflict,
            Assert.Throws<UserFriendlyException>(() => _manager.PreparePublication(template, "stale", Guid.NewGuid())).Code);
        var revision = _manager.PreparePublication(template, "version-1", Guid.NewGuid());
        Assert.Equal(1, revision.Version);
        Assert.Equal(1, template.PublishedVersion);
        template.UpdateDraft("new", null, MessageTemplateVisibility.Private, Minimal.Replace("通知", "updated"));
        Assert.DoesNotContain("updated", revision.DefinitionJson);
        Assert.Equal(_manager.ComputeSha256(revision.DefinitionJson), revision.DefinitionHash);
    }

    [Fact]
    public void Snapshot_metadata_tampering_degrades_to_scalar_summary_without_layout()
    {
        var id = Guid.NewGuid();
        var snapshot = new TemplateContentInfo { TemplateId = id, TemplateVersion = 1, SchemaVersion = 1,
            FallbackText = "summary", Layout = new { type = "card" }, Data = new { } };
        var content = new TemplateContent(id, 1, 1, "summary", JsonSerializer.Serialize(snapshot));
        typeof(TemplateContent).GetProperty(nameof(content.TemplateVersion))!.SetValue(content, 2);
        var mapped = new ChatDomainTemplateContentMapper().Map(content);
        Assert.Null(mapped.Layout);
        Assert.Equal("summary", mapped.FallbackText);
        Assert.Equal(2, mapped.TemplateVersion);
    }

    [Fact]
    public void Snapshot_size_is_checked_after_serializing_all_unicode_data()
    {
        var fields = Enumerable.Range(0, 30).Select(index => new { name = "f" + index, type = "string" }).ToArray();
        var definition = _manager.ValidateDefinitionJson(JsonSerializer.Serialize(new { schemaVersion = 1,
            name = "size", fields, layout = new { type = "card" } }));
        var data = Enumerable.Range(0, 30).ToDictionary(index => "f" + index, _ => (object)new string('中', 2000));
        var sanitized = _manager.ValidateAndSanitizeData(data, definition);
        Assert.Equal(ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge,
            Assert.Throws<UserFriendlyException>(() => _manager.BuildSnapshot(Guid.NewGuid(), 1, definition, sanitized, "summary")).Code);
    }
}
