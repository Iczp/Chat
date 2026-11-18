using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionUnits;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace IczpNet.Chat.MessageSections.Tests;

public class ArticleSender_Tests
{
    [Theory]
    [InlineData(ChatObjectTypeEnums.Official, true, true)]
    [InlineData(ChatObjectTypeEnums.Subscription, true, true)]
    [InlineData(ChatObjectTypeEnums.Personal, true, false)]
    [InlineData(ChatObjectTypeEnums.Official, false, false)]
    public async Task Only_enabled_public_accounts_send_through_the_existing_message_manager(ChatObjectTypeEnums type, bool enabled, bool allowed)
    {
        var manager = Substitute.For<IMessageManager>();
        var accounts = Substitute.For<IChatObjectManager>();
        accounts.GetItemByCacheAsync(1).Returns(new ChatObjectInfo { Id = 1, ObjectType = type, IsEnabled = enabled });
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IChatObjectManager>().Returns(accounts);
        lazy.LazyGetRequiredService<IMessageManager>().Returns(manager);
        var sender = new MessageSender(Substitute.For<IRepository<RedEnvelopeContent, Guid>>(), Substitute.For<IRedPacketManager>()) { LazyServiceProvider = lazy };
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, OwnerObjectType = ChatObjectTypeEnums.Official };
        var input = new MessageInput<ArticleContentInput> { ClientMessageId = "stable", Content = new ArticleContentInput { Items = [new ArticleItemInfo { Title = "Article", OriginalUrl = "https://example.com" }] } };
        manager.SendAsync<ArticleContentInfo, ArticleContent>(unit, input, Arg.Any<ArticleContent>()).Returns(new MessageInfo<ArticleContentInfo> { Id = 99 });
        if (allowed)
        {
            Assert.Equal(99, (await sender.SendArticleContentMessageAsync(unit, input)).Id);
            await manager.Received(1).SendAsync<ArticleContentInfo, ArticleContent>(unit, input, Arg.Any<ArticleContent>());
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(() => sender.SendArticleContentMessageAsync(unit, input));
            await manager.DidNotReceiveWithAnyArgs().SendAsync<ArticleContentInfo, ArticleContent>(default, default, default);
        }
    }
}
