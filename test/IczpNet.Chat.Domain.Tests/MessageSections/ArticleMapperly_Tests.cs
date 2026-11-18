using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using System;
using Xunit;

namespace IczpNet.Chat.MessageSections.Tests;

public class ArticleMapperly_Tests
{
    [Fact]
    public void Article_output_and_typed_message_keep_the_full_group()
    {
        var entity = ArticleContent.Create(new ArticleContentInput { Items = [new ArticleItemInfo { Title = "First", OriginalUrl = "https://example.com/1" }, new ArticleItemInfo { Title = "Second", OriginalUrl = "https://example.com/2" }] });
        var content = new ChatDomainArticleContentMapper().Map(entity);
        Assert.Equal(2, content.Items.Count);
        Assert.Equal("First", content.Title);
        var message = new Probe(content);
        message.SetMessageType(MessageTypes.Article);
        var output = new ChatDomainMessageToArticleMessageMapper(new ChatDomainSessionUnitToSessionUnitSenderInfoMapper(null)).Map(message);
        Assert.Same(content, output.Content);
        Assert.Equal(MessageTypes.Article, output.MessageType);
    }
    private class Probe(ArticleContentInfo content) : Message(new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), OwnerId = 1 })
    {
        public override dynamic Content => content;
    }
}
