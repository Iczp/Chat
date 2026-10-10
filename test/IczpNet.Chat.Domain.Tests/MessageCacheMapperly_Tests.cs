using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Shouldly;
using System;
using System.Reflection;
using Xunit;

namespace IczpNet.Chat;

public class MessageCacheMapperly_Tests
{
    [Fact]
    public void Message_cache_without_quote_maps_correctly()
    {
        var source = CreateMessage("root");
        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        actual.ShouldNotBeNull();
        actual.ClientMessageId.ShouldBe("root");
        actual.MessageType.ShouldBe(MessageTypes.Text);
        actual.QuoteMessage.ShouldBeNull();
    }

    [Fact]
    public void Message_cache_with_text_content_maps_correctly()
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

        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        actual.ShouldNotBeNull();
        actual.ClientMessageId.ShouldBe("with-content");
        actual.Content.ShouldBeOfType<TextContentInfo>().Text.ShouldBe("hello");
    }

    [Fact]
    public void Message_cache_with_quote_maps_correctly()
    {
        var source = CreateMessage("root");
        var quote = CreateMessage("quoted");
        source.SetQuoteMessage(quote);
        typeof(Message).GetProperty(nameof(Message.QuoteMessage), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(source, quote);

        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        actual.ShouldNotBeNull();
        actual.QuoteMessage.ShouldNotBeNull();
        actual.QuoteMessage.ClientMessageId.ShouldBe("quoted");
    }

    [Fact]
    public void Message_cache_with_quoted_content_maps_correctly()
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

        var actual = new ChatDomainMessageToMessageCacheItemMapper().Map(source);

        actual.ShouldNotBeNull();
        actual.QuoteMessage.ShouldNotBeNull();
        actual.QuoteMessage.Content.ShouldBeOfType<TextContentInfo>().Text.ShouldBe("quoted text");
    }

    [Fact]
    public void Message_cache_update_clears_an_existing_quote_when_none_present()
    {
        var source = CreateMessage("root");
        var actual = new MessageCacheItem { QuoteMessage = new MessageQuoteCacheItem() };

        new ChatDomainMessageToMessageCacheItemMapper().Map(source, actual);

        actual.ClientMessageId.ShouldBe("root");
        actual.QuoteMessage.ShouldBeNull();
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
