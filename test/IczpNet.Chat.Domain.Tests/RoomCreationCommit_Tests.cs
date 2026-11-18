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


}

public class RoomCreationRollback_Tests : ChatDomainTestBase
{
    [Fact]
    public async Task Prepared_cache_is_removed_when_the_transaction_rolls_back()
    {
        var constructor = typeof(RoomManager).GetConstructors().Single();
        var args = constructor.GetParameters().Select(x => Substitute.For([x.ParameterType], [])).ToArray();
        ((IOptions<RoomOptions>)args.Single(x => x is IOptions<RoomOptions>)).Value.Returns(new RoomOptions());
        var manager = (RoomManager)constructor.Invoke(args);
        var cache = (ISessionUnitCacheManager)args.Single(x => x is ISessionUnitCacheManager);
        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ISessionUnitCacheManager)).Returns(cache);
        var callbackScope = Substitute.For<IServiceScope>();
        callbackScope.ServiceProvider.Returns(provider);
        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(callbackScope);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IServiceScopeFactory>().Returns(factory);
        manager.LazyServiceProvider = lazy;
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), SessionId = Guid.NewGuid(), OwnerId = 10 };
        using (var scope = ServiceProvider.CreateScope())
        {
            var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var method = typeof(RoomManager).GetMethod("PrepareUnitCachesAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            await (Task)method.Invoke(manager, [uow, new List<SessionUnitCacheItem> { unit }])!;
            await uow.RollbackAsync();
        }
        await cache.Received(1).AddUnitsAsync(Arg.Is<IEnumerable<SessionUnitCacheItem>>(x => x.Single().Id == unit.Id));
        await cache.Received(1).RemoveNewUnitsAsync(Arg.Is<IEnumerable<SessionUnitCacheItem>>(x => x.Single().Id == unit.Id));
    }

    [Fact]
    public async Task Pre_commit_failure_rolls_back_flushed_sql()
    {
        var sessionId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            using var scope = ServiceProvider.CreateScope();
            var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var db = await scope.ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            db.Set<Session>().Add(new Session(sessionId, Guid.NewGuid().ToString(), default));
            await db.SaveChangesAsync();
            throw new InvalidOperationException("redis unavailable");
        });
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.False(await db.Set<Session>().AnyAsync(x => x.Id == sessionId));
        });
    }

    [Fact]
    public async Task Post_commit_failure_cannot_roll_back_committed_sql()
    {
        var sessionId = Guid.NewGuid();
        using (var scope = ServiceProvider.CreateScope())
        {
            var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var db = await scope.ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            db.Set<Session>().Add(new Session(sessionId, Guid.NewGuid().ToString(), default));
            await db.SaveChangesAsync();
            uow.OnCompleted(() => Task.FromException(new InvalidOperationException("redis unavailable")));
            await Assert.ThrowsAsync<InvalidOperationException>(() => uow.CompleteAsync());
        }
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.True(await db.Set<Session>().AnyAsync(x => x.Id == sessionId));
        });
    }


}
