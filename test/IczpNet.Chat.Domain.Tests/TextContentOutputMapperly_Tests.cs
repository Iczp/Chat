using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using Shouldly;
using System;
using Xunit;

namespace IczpNet.Chat;

public class TextContentOutputMapperly_Tests
{
    [Fact]
    public void Persisted_text_content_maps_correctly()
    {
        var id = Guid.NewGuid();
        var source = new TextContent(id) { Text = "hello" };

        var actual = new ChatDomainTextContentToTextContentInfoMapper().Map(source);

        actual.Id.ShouldBe(id);
        actual.Text.ShouldBe("hello");
    }
}
