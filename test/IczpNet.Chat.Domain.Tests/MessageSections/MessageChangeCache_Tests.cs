using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace IczpNet.Chat;

public class MessageChangeCache_Tests
{
    private static (MessageCacheManager Cache, IDatabase Redis) Create()
    {
        var db = Substitute.For<IDatabase>();
        var connection = Substitute.For<IConnectionMultiplexer>();
        connection.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(db);
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(connection);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = "tests:" }));
        lazy.LazyGetRequiredService<IOptions<MessageOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new MessageOptions()));
        return (new MessageCacheManager { LazyServiceProvider = lazy }, db);
    }

    [Fact]
    public async Task Warm_certified_range_does_not_query_sql()
    {
        var (cache, redis) = Create();
        var entries = new List<MessageChangeEntry> { new() { MessageId = 5, Version = 7 } };
        redis.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns((RedisValue)JsonSerializer.Serialize(entries));
        var result = await cache.GetChangeWindowAsync(Guid.NewGuid(), Guid.NewGuid(), 6, 7, 100,
            () => throw new InvalidOperationException("Must not query SQL"));
        Assert.Equal(5, Assert.Single(result).MessageId);
    }

    [Fact]
    public async Task Redis_failure_falls_back_to_the_bounded_database_query()
    {
        var (cache, redis) = Create();
        redis.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns<Task<RedisValue>>(_ => throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "test"));
        var calls = 0;
        var result = await cache.GetChangeWindowAsync(Guid.NewGuid(), Guid.NewGuid(), 6, 7, 100, () =>
        {
            calls++;
            return Task.FromResult(new List<MessageChangeEntry> { new() { MessageId = 5, Version = 7 } });
        });
        Assert.Equal(1, calls);
        Assert.Equal(7, Assert.Single(result).Version);
    }
}
