using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections;
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
    [Theory]
    [InlineData(null, true)]
    [InlineData(26, true)]
    [InlineData(27, false)]
    [InlineData(44, false)]
    public void Client_id_contract_matches_the_existing_database_limit(int? length, bool valid)
    {
        var input = new MessageInput { ClientMessageId = length.HasValue ? new string('a', length.Value) : null };
        var errors = new List<ValidationResult>();
        Assert.Equal(valid, Validator.TryValidateObject(input, new ValidationContext(input), errors, true));
        if (!valid) Assert.Contains(errors, error => error.MemberNames.Contains(nameof(MessageInput.ClientMessageId)));
    }

    [Fact]
    public async Task Oversized_client_id_is_rejected_before_lock_database_or_content_work()
    {
        var constructor = typeof(MessageManager).GetConstructors().Single();
        var arguments = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        var manager = (MessageManager)constructor.Invoke(arguments);
        var repository = (IMessageRepository)arguments.Single(x => x is IMessageRepository);
        var unitsOfWork = (IUnitOfWorkManager)arguments.Single(x => x is IUnitOfWorkManager);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        manager.LazyServiceProvider = lazy;
        var exception = await Assert.ThrowsAsync<Volo.Abp.UserFriendlyException>(() => manager.CreateMessageAsync(
            new SessionUnitCacheItem { Id = Guid.NewGuid() },
            _ => throw new InvalidOperationException("Invalid input must not create content."),
            $"article:{Guid.NewGuid()}"));
        Assert.Contains("26", exception.Message);
        Assert.Empty(repository.ReceivedCalls());
        Assert.Empty(unitsOfWork.ReceivedCalls());
        Assert.Empty(lazy.ReceivedCalls());
    }

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
