using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using Xunit;

namespace IczpNet.Chat.SessionUnits;

public class RoomCacheWrite_Tests
{
    [RedisReviewFact]
    public async Task Add_units_writes_to_redis_and_reports_wrong_type()
    {
        using var connection = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = "room-test:" + Guid.NewGuid().ToString("N") + ":";
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(connection);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = prefix }));
        lazy.LazyGetRequiredService<IOptions<SessionUnitOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new SessionUnitOptions()));
        var json = Substitute.For<IJsonSerializer>();
        json.Serialize(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns("{}");
        lazy.LazyGetRequiredService<IJsonSerializer>().Returns(json);
        var manager = new SessionUnitCacheManager { LazyServiceProvider = lazy };
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid(), CreationTime = DateTime.UtcNow };
        var database = connection.GetDatabase();
        var unitKey = prefix + "SessionUnits:Units:" + unit.Id;
        var other = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 2, SessionId = unit.SessionId, CreationTime = DateTime.UtcNow };
        var otherKey = prefix + "SessionUnits:Units:" + other.Id;
        var membersKey = prefix + "SessionUnits:Sessions:Members:" + unit.SessionId;
        await manager.AddUnitsAsync([unit, other]);
        Assert.True(await database.KeyExistsAsync(unitKey));
        Assert.Equal(2, await database.SortedSetLengthAsync(membersKey));
        await manager.RemoveNewUnitsAsync([unit]);
        Assert.False(await database.KeyExistsAsync(unitKey));
        Assert.True(await database.KeyExistsAsync(otherKey));
        Assert.Equal(1, await database.SortedSetLengthAsync(membersKey));
        await database.StringSetAsync(unitKey, "wrong type");
        await Assert.ThrowsAsync<RedisServerException>(() => manager.AddUnitsAsync([unit]));
    }

    [Fact]
    public async Task Add_units_propagates_redis_batch_failures()
    {
        var batch = Substitute.For<IBatch>();
        batch.HashSetAsync(Arg.Any<RedisKey>(), Arg.Any<HashEntry[]>(), Arg.Any<CommandFlags>())
            .Returns(Task.FromException(new RedisServerException("wrong key type")));
        var database = Substitute.For<IDatabase>();
        database.CreateBatch(Arg.Any<object>()).Returns(batch);
        var connection = Substitute.For<IConnectionMultiplexer>();
        connection.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(connection);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = "test:" }));
        lazy.LazyGetRequiredService<IOptions<SessionUnitOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new SessionUnitOptions()));
        var json = Substitute.For<IJsonSerializer>();
        json.Serialize(Arg.Any<object>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns("{}");
        lazy.LazyGetRequiredService<IJsonSerializer>().Returns(json);
        var manager = new SessionUnitCacheManager { LazyServiceProvider = lazy };
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid(), CreationTime = DateTime.UtcNow };

        await Assert.ThrowsAsync<RedisServerException>(() => manager.AddUnitsAsync([unit]));
        batch.Received(1).Execute();
    }
}


