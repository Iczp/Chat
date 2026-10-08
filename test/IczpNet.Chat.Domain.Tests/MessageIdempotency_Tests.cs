using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using NSubstitute;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat;

public class MessageIdempotency_Tests
{
    [Fact]
    public async Task Retried_client_id_returns_original_message_without_content_side_effects_and_commits_before_unlock()
    {
        var constructor = typeof(MessageManager).GetConstructors().Single();
        var arguments = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        var manager = (MessageManager)constructor.Invoke(arguments);
        var repository = (IMessageRepository)arguments.Single(x => x is IMessageRepository);
        var unitsOfWork = (IUnitOfWorkManager)arguments.Single(x => x is IUnitOfWorkManager);
        var uow = Substitute.For<IUnitOfWork>();
        unitsOfWork.Begin(Arg.Any<AbpUnitOfWorkOptions>(), true).Returns(uow);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        var existing = new Message(unit) { ClientMessageId = "stable-client-id" };
        repository.FindAsync(Arg.Any<Expression<Func<Message, bool>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var predicate = call.Arg<Expression<Func<Message, bool>>>().Compile();
                Assert.True(predicate(existing));
                Assert.False(predicate(new Message(unit) { ClientMessageId = "different" }));
                return existing;
            });
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<ICancellationTokenProvider>().Returns(Substitute.For<ICancellationTokenProvider>());
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        var handle = Substitute.For<IAbpDistributedLockHandle>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(handle);
        lazy.LazyGetRequiredService<IAbpDistributedLock>().Returns(distributedLock);
        manager.LazyServiceProvider = lazy;
        var committed = false;
        uow.CompleteAsync(Arg.Any<CancellationToken>()).Returns(_ => { committed = true; return Task.CompletedTask; });
        handle.DisposeAsync().Returns(_ => { Assert.True(committed); return ValueTask.CompletedTask; });
        var result = await manager.CreateMessageAsync(unit,
            _ => throw new InvalidOperationException("Retry must not create content."), "stable-client-id");
        Assert.Same(existing, result);
        await repository.DidNotReceiveWithAnyArgs().InsertAsync(default, default, default);
        await handle.Received(1).DisposeAsync();
    }
}
