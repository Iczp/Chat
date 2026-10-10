using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using Shouldly;
using System;
using Xunit;

namespace IczpNet.Chat;

public class TextContentOutputMapperly_Tests
{
    [Fact]
    public void Persisted_text_content_maps_like_the_existing_profile()
    {
        var profile = typeof(ChatDomainModule).Assembly.GetType(
            "IczpNet.Chat.AutoMappers.ChatApplicationAutoMapperProfile", throwOnError: true)!;
        var configuration = new MapperConfiguration(config => config.AddProfile(profile));
        var oldMapper = configuration.CreateMapper();
        var source = new TextContent(Guid.NewGuid()) { Text = "hello" };

        var expected = oldMapper.Map<TextContentInfo>(source);
        var actual = new ChatDomainTextContentToTextContentInfoMapper().Map(source);

        actual.Id.ShouldBe(expected.Id);
        actual.Text.ShouldBe(expected.Text);
    }
}
