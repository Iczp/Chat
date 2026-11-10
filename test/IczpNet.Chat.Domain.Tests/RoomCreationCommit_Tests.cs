using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat;

public class RoomCreationCommit_Tests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_or_nontransactional_work_unit_starts_a_transaction(bool hasCurrent)
    {
        var constructor = typeof(RoomManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        ((IOptions<RoomOptions>)args.Single(x => x is IOptions<RoomOptions>)).Value.Returns(new RoomOptions());
        var manager = (RoomManager)constructor.Invoke(args);
        var uows = (IUnitOfWorkManager)args.Single(x => x is IUnitOfWorkManager);
        if (hasCurrent)
        {
            var current = Substitute.For<IUnitOfWork>();
            current.Options.Returns(new AbpUnitOfWorkOptions { IsTransactional = false });
            uows.Current.Returns(current);
        }
        var transaction = Substitute.For<IUnitOfWork>();
        uows.Begin(Arg.Is<AbpUnitOfWorkOptions>(x => x.IsTransactional), true).Returns(transaction);
        var types = (IChatObjectTypeManager)args.Single(x => x is IChatObjectTypeManager);
        types.GetAsync(ChatObjectTypeEnums.Room).Returns<Task<ChatObjectType>>(_ => throw new InvalidOperationException("stop"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.CreateAsync("test", [], null));
        uows.Received(1).Begin(Arg.Is<AbpUnitOfWorkOptions>(x => x.IsTransactional), true);
        transaction.Received(1).Dispose();
    }

    [Fact]
    public async Task Cache_connections_and_announcement_are_deferred_until_commit()
    {
        var constructor = typeof(RoomManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        ((IOptions<RoomOptions>)args.Single(x => x is IOptions<RoomOptions>)).Value.Returns(new RoomOptions());
        var manager = (RoomManager)constructor.Invoke(args);
        var cache = Substitute.For<ISessionUnitCacheManager>();
        var online = Substitute.For<IOnlineManager>();
        var sender = Substitute.For<IMessageSender>();
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ISessionUnitCacheManager)).Returns(cache);
        provider.GetService(typeof(IOnlineManager)).Returns(online);
        provider.GetService(typeof(IMessageSender)).Returns(sender);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(scope);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IServiceScopeFactory>().Returns(factory);
        manager.LazyServiceProvider = lazy;
        var callbacks = new List<Func<Task>>();
        var uow = Substitute.For<IUnitOfWork>();
        uow.When(x => x.OnCompleted(Arg.Any<Func<Task>>())).Do(x => callbacks.Add(x.Arg<Func<Task>>()));
        var sessionId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = sessionId, OwnerId = 10 };
        var content = new CmdContentInfo { Text = "created" };
        var method = typeof(RoomManager).GetMethod("RegisterCreatedRoomCompletion", BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(manager, [uow, new List<SessionUnitCacheItem> { unit }, unit, content, sessionId]);

        Assert.Empty(cache.ReceivedCalls());
        Assert.Empty(online.ReceivedCalls());
        Assert.Empty(sender.ReceivedCalls());
        await Assert.Single(callbacks)();
        await cache.Received(1).AddUnitsAsync(Arg.Is<IEnumerable<SessionUnitCacheItem>>(x => x.Single().Id == unit.Id));
        await online.Received(1).AddSessionAsync(Arg.Is<List<(Guid SessionId, long OwnerId)>>(x => x.Single().SessionId == sessionId));
        await sender.Received(1).SendCmdAsync(unit, Arg.Is<MessageInput<CmdContentInfo>>(x => x.Content == content && x.ClientMessageId.Length <= 26 && x.ClientMessageId.StartsWith("RC")));
    }
}

public class RoomCreationRollback_Tests : ChatDomainTestBase
{
    [Fact]
    public async Task Rolled_back_transaction_does_not_publish_cache_or_announcement()
    {
        var constructor = typeof(RoomManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        ((IOptions<RoomOptions>)args.Single(x => x is IOptions<RoomOptions>)).Value.Returns(new RoomOptions());
        var manager = (RoomManager)constructor.Invoke(args);
        var cache = Substitute.For<ISessionUnitCacheManager>();
        var online = Substitute.For<IOnlineManager>();
        var sender = Substitute.For<IMessageSender>();
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ISessionUnitCacheManager)).Returns(cache);
        provider.GetService(typeof(IOnlineManager)).Returns(online);
        provider.GetService(typeof(IMessageSender)).Returns(sender);
        var callbackScope = Substitute.For<IServiceScope>();
        callbackScope.ServiceProvider.Returns(provider);
        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(callbackScope);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IServiceScopeFactory>().Returns(factory);
        manager.LazyServiceProvider = lazy;
        var sessionId = Guid.NewGuid();
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = sessionId, OwnerId = 10 };
        using (var scope = ServiceProvider.CreateScope())
        {
            var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var db = await scope.ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            db.Set<Session>().Add(new Session(sessionId, Guid.NewGuid().ToString(), default));
            await db.SaveChangesAsync();
            var method = typeof(RoomManager).GetMethod("RegisterCreatedRoomCompletion", BindingFlags.Instance | BindingFlags.NonPublic)!;
            method.Invoke(manager, [uow, new List<SessionUnitCacheItem> { unit }, unit, new CmdContentInfo(), sessionId]);
            await uow.RollbackAsync();
        }
        Assert.Empty(cache.ReceivedCalls());
        Assert.Empty(online.ReceivedCalls());
        Assert.Empty(sender.ReceivedCalls());
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.False(await db.Set<Session>().AnyAsync(x => x.Id == sessionId));
        });
    }
}
