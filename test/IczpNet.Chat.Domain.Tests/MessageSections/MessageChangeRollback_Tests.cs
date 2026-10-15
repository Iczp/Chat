using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat;

public class MessageChangeRollback_Tests
{
    private static (MessageManager Manager, Message Message, IMessageChangeRepository Changes, IUnitOfWork Uow) Create()
    {
        var constructor = typeof(MessageManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(parameter => parameter.ParameterType == typeof(IOptions<MessageOptions>)
            ? (object)Microsoft.Extensions.Options.Options.Create(new MessageOptions { ChangeSyncEnabled = true })
            : Substitute.For([parameter.ParameterType], [])).ToArray();
        var manager = (MessageManager)constructor.Invoke(args);
        args.OfType<ISettingProvider>().Single().GetOrNullAsync(ChatSettings.AllowRollbackHours).Returns("24");
        var uow = Substitute.For<IUnitOfWork>();
        args.OfType<IUnitOfWorkManager>().Single().Current.Returns(uow);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        var changes = Substitute.For<IMessageChangeRepository>();
        lazy.LazyGetRequiredService<IMessageChangeRepository>().Returns(changes);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.Now);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        manager.LazyServiceProvider = lazy;
        var message = new Message(new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid() });
        typeof(Message).GetProperty(nameof(Message.CreationTime))!.SetValue(message, DateTime.Now);
        return (manager, message, changes, uow);
    }

    [Fact]
    public async Task Journal_is_appended_before_the_message_state_is_saved()
    {
        var (manager, message, changes, uow) = Create();
        changes.AppendRollbackAsync(message, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            Assert.False(message.IsRollbacked);
            return new MessageChangeEntry { Version = 7, ChangedAt = call.Arg<DateTime>() };
        });
        await manager.RollbackAsync(message);
        Assert.True(message.IsRollbacked);
        Assert.Equal(7, message.SyncVersion);
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Already_committed_recall_does_not_resave_a_stale_entity()
    {
        var (manager, message, changes, uow) = Create();
        changes.AppendRollbackAsync(message, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new MessageChangeEntry { AlreadyApplied = true, Version = 7 });
        await manager.RollbackAsync(message);
        Assert.False(message.IsRollbacked);
        await uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Failed_journal_write_does_not_change_the_message_in_memory()
    {
        var (manager, message, changes, uow) = Create();
        changes.AppendRollbackAsync(message, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<MessageChangeEntry>>(_ => throw new InvalidOperationException("storage failed"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RollbackAsync(message));
        Assert.False(message.IsRollbacked);
        await uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }
}
