using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Shouldly;
using System;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace IczpNet.Chat;

public class MessageCacheMapperly_Tests
{
    private readonly IMapper _legacyMapper;

    public MessageCacheMapperly_Tests()
    {
        var profile = typeof(ChatDomainModule).Assembly.GetType(
            "IczpNet.Chat.AutoMappers.ChatApplicationAutoMapperProfile", throwOnError: true)!;
        var configuration = new MapperConfiguration(config => config.AddProfile(profile));
        _legacyMapper = configuration.CreateMapper();
    }

    [Fact]
    public void Message_cache_without_quote_matches_the_current_profile()
    {
        var source = CreateMessage("root");
        var expected = _legacyMapper.Map<MessageCacheItem>(source);
        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
    }

    [Fact]
    public void Message_cache_with_text_content_matches_the_current_profile()
    {
        var source = new MessageWithTestContent(new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerId = 123,
        })
        {
            ClientMessageId = "with-content",
            ContentValue = new TextContentInfo { Text = "hello" },
        };
        source.SetMessageType(MessageTypes.Text);

        var expected = _legacyMapper.Map<MessageCacheItem>(source);
        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
        actual.Content.ShouldBeOfType<TextContentInfo>().Text.ShouldBe("hello");
    }

    [Fact]
    public void Message_cache_with_quote_matches_the_current_profile()
    {
        var source = CreateMessage("root");
        var quote = CreateMessage("quoted");
        source.SetQuoteMessage(quote);
        typeof(Message).GetProperty(nameof(Message.QuoteMessage), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(source, quote);

        var expected = _legacyMapper.Map<MessageCacheItem>(source);
        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
        actual.QuoteMessage.ShouldNotBeNull();
    }

    [Fact]
    public void Message_cache_with_quoted_content_matches_the_current_profile()
    {
        var unit = new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerId = 123,
        };
        var source = new MessageWithTestContent(unit)
        {
            ClientMessageId = "root",
            ContentValue = new TextContentInfo { Text = "root text" },
        };
        var quote = new MessageWithTestContent(unit)
        {
            ClientMessageId = "quoted",
            ContentValue = new TextContentInfo { Text = "quoted text" },
        };
        source.SetQuoteMessage(quote);
        typeof(Message).GetProperty(nameof(Message.QuoteMessage), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(source, quote);

        var expected = _legacyMapper.Map<MessageCacheItem>(source);
        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
        actual.QuoteMessage.Content.ShouldBeOfType<TextContentInfo>().Text.ShouldBe("quoted text");
    }

    [Fact]
    public void Message_cache_update_clears_an_existing_quote_like_the_current_profile()
    {
        var source = CreateMessage("root");
        var expected = new MessageCacheItem { QuoteMessage = new MessageQuoteCacheItem() };
        var actual = new MessageCacheItem { QuoteMessage = new MessageQuoteCacheItem() };

        _legacyMapper.Map(source, expected);
        new ChatDomainMessageToMessageCacheItemMapper().Map(source, actual);

        JsonSerializer.Serialize(actual).ShouldBe(JsonSerializer.Serialize(expected));
    }

    private static Message CreateMessage(string clientMessageId)
    {
        var source = new Message(new SessionUnitCacheItem
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            OwnerId = 123,
            DestinationId = 456,
        })
        {
            ClientMessageId = clientMessageId,
        };
        source.SetMessageType(MessageTypes.Text);
        source.SetKey("source", "value");
        return source;
    }

    private sealed class MessageWithTestContent(SessionUnitCacheItem unit) : Message(unit)
    {
        public object ContentValue { get; set; } = null!;

        public override object GetContentDto() => ContentValue;
    }
}
