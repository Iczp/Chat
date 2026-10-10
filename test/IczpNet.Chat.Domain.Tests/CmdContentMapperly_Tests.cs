using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using Shouldly;
using System;
using Xunit;

namespace IczpNet.Chat;

public class CmdContentMapperly_Tests
{
    [Fact]
    public void Command_content_create_update_and_output_work_properly()
    {
        var mapperly = new ChatDomainCmdContentInfoToCmdContentMapper();
        var source = new CmdContentInfo
        {
            Id = Guid.NewGuid(),
            Cmd = "open",
            Text = "notice",
            Url = "app:///notice/1",
        };

        var actual = mapperly.Map(source);
        actual.Id.ShouldBe(Guid.Empty);
        actual.Cmd.ShouldBe(source.Cmd);
        actual.Text.ShouldBe(source.Text);
        actual.Url.ShouldBe(source.Url);

        var update = new CmdContentInfo { Cmd = "close", Text = "updated", Url = "app:///notice/2" };
        mapperly.Map(update, actual);
        actual.Cmd.ShouldBe(update.Cmd);
        actual.Text.ShouldBe(update.Text);
        actual.Url.ShouldBe(update.Url);

        var actualOutput = mapperly.ReverseMap(actual);
        actualOutput.Cmd.ShouldBe("close");
        actualOutput.Text.ShouldBe("updated");
        actualOutput.Url.ShouldBe("app:///notice/2");
    }
}
