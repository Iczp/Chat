using System;
using System.Collections.Generic;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Xunit;

namespace IczpNet.Chat.Domain.Tests.MessageSections;

public class TemplateContentMapperly_Tests
{
    private readonly ChatDomainTemplateContentMapper _contentMapper = new();

    [Fact]
    public void TemplateContent_Should_Be_Disabled_From_Forwarding()
    {
        Assert.Contains(MessageTypes.Template, MessageExtentions.DisabledForwardList);
    }

    [Fact]
    public void ChatDomainTemplateContentMapper_Should_Map_SnapshotJson_To_TemplateContentInfo()
    {
        var templateId = Guid.NewGuid();
        var snapshot = new TemplateContentInfo
        {
            SchemaVersion = 1,
            TemplateId = templateId,
            TemplateVersion = 2,
            TemplateName = "服务派工单",
            FallbackText = "[派工单]SG20261123",
            Data = new Dictionary<string, object> { ["title"] = "紧急抢修" },
            Layout = new { type = "card" },
            Actions = new[] { new { id = "view", type = "openUrl" } }
        };
        var snapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshot);

        var content = new TemplateContent(templateId, 2, 1, "[派工单]SG20261123", snapshotJson);
        content.SetId(Guid.NewGuid());

        var mapped = _contentMapper.Map(content);

        Assert.NotNull(mapped);
        Assert.Equal(1, mapped.SchemaVersion);
        Assert.Equal(templateId, mapped.TemplateId);
        Assert.Equal(2, mapped.TemplateVersion);
        Assert.Equal("服务派工单", mapped.TemplateName);
        Assert.Equal("[派工单]SG20261123", mapped.FallbackText);
        Assert.NotNull(mapped.Data);
        Assert.NotNull(mapped.Layout);
        Assert.NotNull(mapped.Actions);
    }
}
