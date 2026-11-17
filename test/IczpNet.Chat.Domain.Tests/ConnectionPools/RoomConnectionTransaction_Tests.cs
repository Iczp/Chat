using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IczpNet.Chat.Connections;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat.ConnectionPools;

public class RoomConnectionTransaction_Tests : ChatDomainTestBase
{
    private static OnlineManager Create(ConnectionMultiplexer redis, string prefix)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(redis);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = prefix }));
        lazy.LazyGetRequiredService<IOptions<ConnectionOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new ConnectionOptions()));
        return new OnlineManager { LazyServiceProvider = lazy };
    }

    [RedisReviewFact]
    public async Task Rollback_removes_only_new_owner_and_preserves_int64_and_concurrent_owner()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-pool-test:" + Guid.NewGuid().ToString("N") + ":";
        var db = redis.GetDatabase();
        var session = Guid.NewGuid();
        var key = prefix + "Online:Sessions:" + session;
        await db.HashSetAsync(prefix + "Online:Devices:42", "conn", "pc:device");
        await db.HashSetAsync(key, "conn", "[9007199254740993]");
        var manager = Create(redis, prefix);
        using (var scope = ServiceProvider.CreateScope())
        {
            var managerUow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = managerUow.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            await manager.AddSessionInTransactionAsync([(session, 42)], uow);
            Assert.Equal("[9007199254740993,42]", (string)await db.HashGetAsync(key, "conn"));
            await db.HashSetAsync(key, "conn", "[9007199254740993,42,99]");
            await uow.RollbackAsync();
        }
        Assert.Equal("[9007199254740993,99]", (string)await db.HashGetAsync(key, "conn"));
    }

    [RedisReviewFact]
    public async Task Commit_preserves_connection_membership()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-pool-test:" + Guid.NewGuid().ToString("N") + ":";
        var db = redis.GetDatabase();
        var session = Guid.NewGuid();
        var key = prefix + "Online:Sessions:" + session;
        await db.HashSetAsync(prefix + "Online:Devices:42", "conn", "pc:device");
        var manager = Create(redis, prefix);
        using (var scope = ServiceProvider.CreateScope())
        {
            var managerUow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = managerUow.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            await manager.AddSessionInTransactionAsync([(session, 42)], uow);
            await uow.CompleteAsync();
        }
        Assert.Equal("[42]", (string)await db.HashGetAsync(key, "conn"));
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        Assert.Empty(server.Keys(pattern: prefix + "Online:RoomPending:*"));
    }

    [RedisReviewFact]
    public async Task Concurrent_invites_to_same_connection_preserve_the_committed_owner()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-pool-test:" + Guid.NewGuid().ToString("N") + ":";
        var db = redis.GetDatabase();
        var session = Guid.NewGuid();
        var key = prefix + "Online:Sessions:" + session;
        await db.HashSetAsync(prefix + "Online:Devices:42", "conn", "pc:device");
        await db.HashSetAsync(prefix + "Online:Devices:43", "conn", "pc:device");
        var manager = Create(redis, prefix);
        using var scope1 = ServiceProvider.CreateScope();
        using var scope2 = ServiceProvider.CreateScope();
        var manager1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var manager2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow1 = manager1.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
        using var uow2 = manager2.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
        await Task.WhenAll(manager.AddSessionInTransactionAsync([(session, 42)], uow1),
            manager.AddSessionInTransactionAsync([(session, 43)], uow2));
        await uow1.RollbackAsync();
        uow1.Dispose();
        await uow2.CompleteAsync();
        Assert.Equal("[43]", (string)await db.HashGetAsync(key, "conn"));
    }

    [RedisReviewFact]
    public async Task Partial_redis_failure_rolls_back_other_connection_writes()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-pool-test:" + Guid.NewGuid().ToString("N") + ":";
        var db = redis.GetDatabase();
        var good = Guid.NewGuid();
        var bad = Guid.NewGuid();
        await db.HashSetAsync(prefix + "Online:Devices:42", "conn", "pc:device");
        await db.StringSetAsync(prefix + "Online:Sessions:" + bad, "wrong-type");
        var manager = Create(redis, prefix);
        await Assert.ThrowsAsync<RedisServerException>(async () =>
        {
            using var scope = ServiceProvider.CreateScope();
            var managerUow = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = managerUow.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            await manager.AddSessionInTransactionAsync([(good, 42), (bad, 42)], uow);
            await uow.CompleteAsync();
        });
        Assert.False(await db.HashExistsAsync(prefix + "Online:Sessions:" + good, "conn"));
    }
}
