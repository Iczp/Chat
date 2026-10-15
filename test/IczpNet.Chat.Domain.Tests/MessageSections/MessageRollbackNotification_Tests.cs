using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using NSubstitute;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus.Distributed;
using Xunit;

namespace IczpNet.Chat;

public class MessageRollbackNotification_Tests
{
    [Fact]
    public async Task Withdrawal_invalidates_the_individual_cache_and_defers_a_content_free_notification_until_commit()
    {
        var cache = Substitute.For<IDistributedCache<MessageCacheItem, MessageCacheKey>>();
        var bus = Substitute.For<IDistributedEventBus>();
        var hosted = Substitute.For<ICurrentHosted>();
        hosted.Name.Returns("Chat");
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), OwnerId = 7 };
        var message = new Message(unit) { ClientMessageId = "original-client-id" };
        typeof(Message).GetProperty(nameof(Message.Id))!.SetValue(message, 7307873L);
        var rollbackTime = new DateTime(2026, 10, 14, 1, 0, 0, DateTimeKind.Utc);
        typeof(Message).GetMethod("Rollback", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(message, [rollbackTime]);
        SendMessageToClientDistributedEto published = null;
        bus.PublishAsync(Arg.Do<SendMessageToClientDistributedEto>(value => published = value), false, false).Returns(Task.CompletedTask);

        var (handler, completions) = CreateHandler(cache, bus, hosted);
        await handler.HandleEventAsync(new EntityUpdatedEventData<Message>(message));
        Assert.Empty(cache.ReceivedCalls());
        Assert.Null(published);
        await Assert.Single(completions)();

        await cache.Received(1).RemoveAsync(Arg.Is<MessageCacheKey>(key => key.MessageId == message.Id), false, false, Arg.Any<CancellationToken>());
        Assert.NotNull(published);
        Assert.Equal(CommandConsts.MessageRollbacked, published.Command);
        Assert.Equal(message.Id, published.MessageId);
        Assert.Equal(unit.SessionId, published.Message.SessionId);
        Assert.Equal(unit.Id, published.Message.SenderSessionUnitId);
        Assert.True(published.Message.IsRollbacked);
        Assert.Equal(rollbackTime, published.Message.RollbackTime);
        Assert.Null(published.Message.Content);
        Assert.Equal(message.ClientMessageId, published.Message.ClientMessageId);
    }

    [Fact]
    public async Task Ordinary_updates_invalidate_the_cache_without_fabricating_a_withdrawal()
    {
        var cache = Substitute.For<IDistributedCache<MessageCacheItem, MessageCacheKey>>();
        var bus = Substitute.For<IDistributedEventBus>();
        var message = new Message(new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid() });
        var (handler, completions) = CreateHandler(cache, bus, Substitute.For<ICurrentHosted>());
        await handler.HandleEventAsync(new EntityUpdatedEventData<Message>(message));
        await Assert.Single(completions)();
        await cache.Received(1).RemoveAsync(Arg.Any<MessageCacheKey>(), false, false, Arg.Any<CancellationToken>());
        await bus.DidNotReceiveWithAnyArgs().PublishAsync(default(SendMessageToClientDistributedEto), default, default);
    }

    private static (MessageUpdatedLocalEventHandler Handler, List<Func<Task>> Completions) CreateHandler(
        IDistributedCache<MessageCacheItem, MessageCacheKey> cache, IDistributedEventBus bus, ICurrentHosted hosted)
    {
        var completions = new List<Func<Task>>();
        var uow = Substitute.For<IUnitOfWork>();
        uow.When(value => value.OnCompleted(Arg.Any<Func<Task>>())).Do(call => completions.Add(call.Arg<Func<Task>>()));
        var manager = Substitute.For<IUnitOfWorkManager>();
        manager.Current.Returns(uow);
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IDistributedCache<MessageCacheItem, MessageCacheKey>)).Returns(cache);
        provider.GetService(typeof(IDistributedEventBus)).Returns(bus);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(scope);
        return (new MessageUpdatedLocalEventHandler(manager, factory, hosted), completions);
    }
}
