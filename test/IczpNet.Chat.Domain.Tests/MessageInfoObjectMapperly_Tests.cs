using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Shouldly;
using System;
using Xunit;

namespace IczpNet.Chat;

public class MessageInfoObjectMapperly_Tests
{
    [Fact]
    public void Message_job_output_maps_correctly()
    {
        var source = new Message(new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerId = 123,
            DestinationId = 456,
        })
        {
            ClientMessageId = "client-1",
        };
        source.SetMessageType(MessageTypes.Text);
        source.SetKey("source", "value");

        var mapperly = new ChatDomainMessageToMessageInfoObjectMapper();
        var actual = mapperly.Map(source);

        actual.ShouldNotBeNull();
        actual.ClientMessageId.ShouldBe("client-1");
        actual.MessageType.ShouldBe(MessageTypes.Text);

        var actualExisting = new MessageInfo<object> { ClientMessageId = "previous" };
        mapperly.Map(source, actualExisting);
        actualExisting.ClientMessageId.ShouldBe("client-1");
        actualExisting.MessageType.ShouldBe(MessageTypes.Text);
    }
}
