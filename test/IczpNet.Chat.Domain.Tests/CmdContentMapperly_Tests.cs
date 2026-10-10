using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using Shouldly;
using System;
using System.Text.Json;
using Xunit;

namespace IczpNet.Chat;

public class CmdContentMapperly_Tests
{
    [Fact]
    public void Command_content_create_update_and_output_match_the_current_profile()
    {
        var profile = typeof(ChatDomainModule).Assembly.GetType(
            "IczpNet.Chat.AutoMappers.ChatApplicationAutoMapperProfile", throwOnError: true)!;
        var configuration = new MapperConfiguration(config => config.AddProfile(profile));
        var legacyMapper = configuration.CreateMapper();
        var mapperly = new ChatDomainCmdContentInfoToCmdContentMapper();
        var source = new CmdContentInfo
        {
            Id = Guid.NewGuid(),
            Cmd = "open",
            Text = "notice",
            Url = "app:///notice/1",
        };

        var expected = legacyMapper.Map<CmdContent>(source);
        var actual = mapperly.Map(source);
        AssertSameContent(expected, actual);

        var update = new CmdContentInfo { Cmd = "close", Text = "updated", Url = "app:///notice/2" };
        legacyMapper.Map(update, expected);
        mapperly.Map(update, actual);
        AssertSameContent(expected, actual);

        var expectedOutput = legacyMapper.Map<CmdContentInfo>(expected);
        var actualOutput = mapperly.ReverseMap(actual);
        JsonSerializer.Serialize(actualOutput).ShouldBe(JsonSerializer.Serialize(expectedOutput));
    }

    private static void AssertSameContent(CmdContent expected, CmdContent actual)
    {
        actual.Id.ShouldBe(expected.Id);
        actual.Cmd.ShouldBe(expected.Cmd);
        actual.Text.ShouldBe(expected.Text);
        actual.Url.ShouldBe(expected.Url);
        actual.OwnerId.ShouldBe(expected.OwnerId);
        actual.IsActive.ShouldBe(expected.IsActive);
        actual.IsEnabled.ShouldBe(expected.IsEnabled);
    }
}
