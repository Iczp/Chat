using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using Shouldly;
using System;
using System.Text.Json;
using Xunit;

namespace IczpNet.Chat;

public class MessageInfoObjectMapperly_Tests
{
    [Fact]
    public void Message_job_output_matches_the_current_profile()
    {
        var profile = typeof(ChatDomainModule).Assembly.GetType(
            "IczpNet.Chat.AutoMappers.ChatApplicationAutoMapperProfile", throwOnError: true)!;
        var configuration = new MapperConfiguration(config => config.AddProfile(profile));
        var legacyMapper = configuration.CreateMapper();
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

        var expected = legacyMapper.Map<MessageInfo<object>>(source);
        var mapperly = new ChatDomainMessageToMessageInfoObjectMapper();
        var actual = mapperly.Map(source);

        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));

        var expectedExisting = new MessageInfo<object> { ClientMessageId = "previous" };
        var actualExisting = new MessageInfo<object> { ClientMessageId = "previous" };
        legacyMapper.Map(source, expectedExisting);
        mapperly.Map(source, actualExisting);
        JsonSerializer.Serialize(actualExisting).ShouldBe(JsonSerializer.Serialize(expectedExisting));
    }
}
