using System;
using System.Threading.Tasks;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat.MessageSections.Messages;

public class RoomReadFence_Tests : ChatDomainTestBase
{
    private static MessageCacheManager Create(ConnectionMultiplexer redis, string prefix, IUnitOfWorkManager uows)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(redis);
        lazy.LazyGetRequiredService<IUnitOfWorkManager>().Returns(uows);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = prefix }));
        lazy.LazyGetRequiredService<IOptions<MessageOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new MessageOptions()));
        return new MessageCacheManager { LazyServiceProvider = lazy };
    }

    [RedisReviewFact]
    public async Task Fence_blocks_stale_read_window_publication_and_clears_on_rollback()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-fence-test:" + Guid.NewGuid().ToString("N") + ":";
        var session = Guid.NewGuid();
        using var scope = ServiceProvider.CreateScope();
        var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var readerUows = Substitute.For<IUnitOfWorkManager>();
        var reader = Create(redis, prefix, readerUows);
        var writer = Create(redis, prefix, uows);
        var window = new SessionMessageWindowCacheItem { MinMessageId = 1 };
        Assert.True(await reader.StoreReadWindowAsync(session, window, ""));
        using (var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
        {
            await writer.PrepareRoomMessageWriteAsync(session);
            var state = await reader.GetReadWindowsAsync(session);
            Assert.Null(state.Recent);
            Assert.False(await reader.StoreReadWindowAsync(session, window, state.Version));
            await uow.RollbackAsync();
        }
        var after = await reader.GetReadWindowsAsync(session);
        Assert.True(await reader.StoreReadWindowAsync(session, window, after.Version));
    }

    [RedisReviewFact]
    public async Task Post_commit_fence_cleanup_failure_keeps_sql_and_forces_cache_fallback()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-fence-test:" + Guid.NewGuid().ToString("N") + ":";
        var session = Guid.NewGuid();
        var pending = prefix + "Messages:ReadWindow:" + session + ":PendingRoomWrite";
        using (var scope = ServiceProvider.CreateScope())
        {
            var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var db = await scope.ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            db.Set<Session>().Add(new Session(session, Guid.NewGuid().ToString(), default));
            await db.SaveChangesAsync();
            await Create(redis, prefix, uows).PrepareRoomMessageWriteAsync(session);
            await redis.GetDatabase().KeyDeleteAsync(pending);
            await redis.GetDatabase().HashSetAsync(pending, "fault", "1");
            await uow.CompleteAsync();
        }
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.True(await db.Set<Session>().AnyAsync(x => x.Id == session));
        });
        var reader = Create(redis, prefix, Substitute.For<IUnitOfWorkManager>());
        var snapshot = await reader.GetReadWindowsAsync(session);
        Assert.Null(snapshot.Recent);
        Assert.Null(snapshot.History);
        Assert.False(await reader.StoreReadWindowAsync(session, new SessionMessageWindowCacheItem { MinMessageId = 1 }, snapshot.Version));
    }

    [RedisReviewFact]
    public async Task Wrong_type_read_window_fails_before_sql_commit()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-fence-test:" + Guid.NewGuid().ToString("N") + ":";
        var session = Guid.NewGuid();
        await redis.GetDatabase().HashSetAsync(prefix + "Messages:ReadWindow:" + session + ":Recent", "fault", "1");
        await Assert.ThrowsAsync<RedisServerException>(async () =>
        {
            using var scope = ServiceProvider.CreateScope();
            var uows = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uows.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            var db = await scope.ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            db.Set<Session>().Add(new Session(session, Guid.NewGuid().ToString(), default));
            await db.SaveChangesAsync();
            await Create(redis, prefix, uows).PrepareRoomMessageWriteAsync(session);
            await uow.CompleteAsync();
        });
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.False(await db.Set<Session>().AnyAsync(x => x.Id == session));
        });
    }
}
