using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Messages;
using Xunit;

namespace IczpNet.Chat;

public class MessageSessionSeqMapping_Tests
{
    [Fact]
    public void Cached_message_and_quote_expose_their_session_sequences()
    {
        var source = new MessageCacheItem
        {
            SessionSeq = 42,
            QuoteMessage = new MessageQuoteCacheItem { SessionSeq = 17 }
        };
        var fast = new MessageSectionApplicationMessageCacheItemToMessageFastDtoMapper().Map(source);
        var owner = new MessageSectionApplicationMessageCacheItemToMessageOwnerDtoMapper().Map(source);
        Assert.Equal(42, fast.SessionSeq);
        Assert.Equal(17, fast.QuoteMessage.SessionSeq);
        Assert.Equal(42, owner.SessionSeq);
    }
}
