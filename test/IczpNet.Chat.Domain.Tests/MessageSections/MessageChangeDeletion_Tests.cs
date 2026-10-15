using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat;

public class MessageChangeDeletion_Tests
{
    private static (SessionUnitSettingManager Manager, IMessageChangeRepository Changes, IUnitOfWork Uow,
        List<Func<Task>> Completions, IDistributedEventBus Bus) Create(bool enabled = true)
    {
        var constructor = typeof(SessionUnitSettingManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(p => Substitute.For([p.ParameterType], [])).ToArray();
        var manager = (SessionUnitSettingManager)constructor.Invoke(args);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        manager.LazyServiceProvider = lazy;
        lazy.LazyGetRequiredService<IOptions<MessageOptions>>().Returns(Microsoft.Extensions.Options.Options.Create(new MessageOptions { ChangeSyncEnabled = enabled }));
        var changes = Substitute.For<IMessageChangeRepository>();
        lazy.LazyGetRequiredService<IMessageChangeRepository>().Returns(changes);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        lazy.LazyGetRequiredService<ICancellationTokenProvider>().Returns(Substitute.For<ICancellationTokenProvider>());
        var uow = Substitute.For<IUnitOfWork>();
        var completions = new List<Func<Task>>();
        uow.When(x => x.OnCompleted(Arg.Any<Func<Task>>())).Do(call => completions.Add(call.Arg<Func<Task>>()));
        var uows = Substitute.For<IUnitOfWorkManager>();
        uows.Current.Returns(uow);
        lazy.LazyGetRequiredService<IUnitOfWorkManager>().Returns(uows);
        var bus = Substitute.For<IDistributedEventBus>();
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(IDistributedEventBus)).Returns(bus);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(scope);
        lazy.LazyGetRequiredService<IServiceScopeFactory>().Returns(factory);
        lazy.LazyGetRequiredService<ICurrentHosted>().Returns(Substitute.For<ICurrentHosted>());
        return (manager, changes, uow, completions, bus);
    }

    [Fact]
    public async Task Disabled_sync_rejects_deletion_before_accessing_the_schema()
    {
        var test = Create(false);
        Assert.Equal("Chat:MessageChangeSyncDisabled",
            (await Assert.ThrowsAsync<BusinessException>(() => test.Manager.DeleteMessageAsync(Guid.NewGuid(), 5))).Code);
        Assert.Empty(test.Changes.ReceivedCalls());
    }

    [Fact]
    public async Task Notification_is_content_free_member_scoped_and_deferred_until_commit()
    {
        var test = Create();
        var unit = Guid.NewGuid();
        var session = Guid.NewGuid();
        test.Changes.AppendDeleteAsync(unit, 5, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new MessageChangeEntry { SessionId = session, MessageId = 5, Version = 7 });
        SendMessageToClientDistributedEto published = null;
        test.Bus.PublishAsync(Arg.Do<SendMessageToClientDistributedEto>(x => published = x), false, false).Returns(Task.CompletedTask);
        await test.Manager.DeleteMessageAsync(unit, 5);
        Assert.Null(published);
        await test.Uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await Assert.Single(test.Completions)();
        Assert.Equal(CommandConsts.MessageDeleted, published.Command);
        Assert.Equal(unit, published.TargetSessionUnitId);
        Assert.Equal(session, published.Message.SessionId);
        Assert.Equal(7, published.Message.SyncVersion);
        Assert.Null(published.Message.Content);
    }

    [Fact]
    public async Task Retry_does_not_register_another_notification()
    {
        var test = Create();
        test.Changes.AppendDeleteAsync(Arg.Any<Guid>(), 5, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new MessageChangeEntry { AlreadyApplied = true });
        await test.Manager.DeleteMessageAsync(Guid.NewGuid(), 5);
        Assert.Empty(test.Completions);
    }

    [Fact]
    public async Task Failed_delete_does_not_save_or_register_notification()
    {
        var test = Create();
        test.Changes.AppendDeleteAsync(Arg.Any<Guid>(), 5, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<MessageChangeEntry>>(_ => throw new InvalidOperationException("storage failed"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => test.Manager.DeleteMessageAsync(Guid.NewGuid(), 5));
        Assert.Empty(test.Completions);
        await test.Uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
