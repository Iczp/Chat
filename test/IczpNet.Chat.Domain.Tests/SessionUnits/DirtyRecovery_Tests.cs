using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Xunit;

namespace IczpNet.Chat.SessionUnits;

public class RedisReviewFactAttribute : FactAttribute
{
    public RedisReviewFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS")))
            Skip = "Set CHAT_REVIEW_REDIS to an isolated Redis 3.2 test instance.";
    }
}

public class DirtyRecovery_Tests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;
    public DirtyRecovery_Tests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;
    private static SessionUnitCacheManager Create(ConnectionMultiplexer connection, string prefix)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(connection);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = prefix }));
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IAbpDistributedLockHandle>());
        lazy.LazyGetRequiredService<IAbpDistributedLock>().Returns(distributedLock);
        return new SessionUnitCacheManager { LazyServiceProvider = lazy };
    }

    [RedisReviewFact]
    public async Task Unqueued_snapshot_is_recoverable_and_ack_does_not_remove_new_dirty_generation()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:{Guid.NewGuid():N}:";
        var database = redis.GetDatabase();
        var manager = Create(redis, prefix);
        var element = new SessionUnitElement(1, ChatObjectTypeEnums.Personal, 2,
            ChatObjectTypeEnums.Personal, Guid.NewGuid(), Guid.NewGuid());
        var dirty = prefix + "SessionUnits:Dirty";
        var registry = dirty + ":ProcessingKeys";
        await database.SortedSetAddAsync(dirty, element, 100);

        var job = Assert.Single(await manager.FlushDirtyToJobsAsync());
        Assert.True(await manager.HasPendingDirtyAsync(job));
        Assert.Equal(1, await manager.GetProcessingDirtyCountAsync(job.ProcessingKey));
        Assert.Empty(await manager.FlushDirtyToJobsAsync());
        // Simulate process exit/failed enqueue: a fresh manager resumes retained records.
        await database.SortedSetAddAsync(registry, job.ProcessingKey, 0);
        var resumed = Assert.Single(await Create(redis, prefix).FlushDirtyToJobsAsync());
        Assert.Equal(job.ProcessingKey, resumed.ProcessingKey);
        Assert.Equal(job.SessionUnitIds, resumed.SessionUnitIds);

        await database.SortedSetAddAsync(dirty, element, 101);
        Assert.Equal(1, await manager.AcknowledgeDirtyAsync(resumed));
        Assert.Equal(0, await manager.AcknowledgeDirtyAsync(resumed));
        Assert.False(await manager.HasPendingDirtyAsync(resumed));
        Assert.Equal(101, await database.SortedSetScoreAsync(dirty, element));
        Assert.False(await database.KeyExistsAsync(job.ProcessingKey));
        Assert.Equal(0, await database.SortedSetLengthAsync(registry));
        var next = Assert.Single(await manager.FlushDirtyToJobsAsync());
        Assert.NotEqual(job.ProcessingKey, next.ProcessingKey);
        await manager.AcknowledgeDirtyAsync(next);
    }

    [RedisReviewFact]
    public async Task Dispatch_is_bounded_and_unmatched_versions_are_not_acknowledged()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:{Guid.NewGuid():N}:";
        var database = redis.GetDatabase();
        var manager = Create(redis, prefix);
        var entries = Enumerable.Range(0, 12).Select(i => new SortedSetEntry(
            new SessionUnitElement(1, ChatObjectTypeEnums.Personal, i + 2,
                ChatObjectTypeEnums.Personal, Guid.NewGuid(), Guid.NewGuid()), i + 1)).ToArray();
        await database.SortedSetAddAsync(prefix + "SessionUnits:Dirty", entries);
        var jobs = await manager.FlushDirtyToJobsAsync(scanSize: 5, jobSize: 2);
        Assert.Equal(3, jobs.Count);
        Assert.Equal(5, jobs.Sum(x => x.Count));
        Assert.Equal(12, await manager.GetProcessingDirtyCountAsync(jobs[0].ProcessingKey));
        var job = jobs[0];
        var first = job.DirtyVersions.Keys.First();
        var original = job.DirtyVersions[first];
        job.DirtyVersions[first] = original + 1;
        Assert.Equal(1, await manager.AcknowledgeDirtyAsync(job));
        Assert.Equal(original, await database.SortedSetScoreAsync(job.ProcessingKey, first));
        foreach (var remaining in jobs.Skip(1)) await manager.AcknowledgeDirtyAsync(remaining);
    }

    [RedisReviewFact]
    public async Task Ten_thousand_friend_entries_filter_before_paging_and_release_snapshot()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:{Guid.NewGuid():N}:";
        var database = redis.GetDatabase();
        var manager = Create(redis, prefix);
        var entries = Enumerable.Range(0, 10000).Select(i => new SortedSetEntry(
            new SessionUnitElement(1, ChatObjectTypeEnums.Personal, i + 2,
                ChatObjectTypeEnums.Personal, Guid.NewGuid(), Guid.NewGuid()),
            FriendScore.Create(i % 4, i < 9950 ? 100 + i % 50 : 150 + i - 9950))).ToArray();
        await database.SortedSetAddAsync(prefix + "SessionUnits:Owners:Friends:1", entries);
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var result = await manager.GetChangedFriendsAsync(1, 149, null, null, 31);
        _output.WriteLine($"Redis 3.2: 10,000 owner entries, selected {result.Count}, elapsed {timer.ElapsedMilliseconds} ms.");
        Assert.Equal(Enumerable.Range(150, 31).Select(x => (double)x), result.Select(x => x.Ticks));
        var keys = redis.GetServer(redis.GetEndPoints().Single()).Keys(pattern: prefix + "*:Changes:*");
        Assert.Empty(keys);
    }

    [RedisReviewFact]
    public async Task Changes_include_unpinned_rows_and_all_equal_tick_pages()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:{Guid.NewGuid():N}:";
        var database = redis.GetDatabase();
        var manager = Create(redis, prefix);
        var ids = Enumerable.Range(1, 150).Select(i => new Guid(i, 0, 0, new byte[8])).ToArray();
        var entries = ids.Select((id, i) => new SortedSetEntry(
            new SessionUnitElement(1, ChatObjectTypeEnums.Personal, i + 2,
                ChatObjectTypeEnums.Personal, id, Guid.NewGuid()), FriendScore.Create(i % 2, 1000))).ToArray();
        await database.SortedSetAddAsync(prefix + "SessionUnits:Owners:Friends:1", entries);
        var first = await manager.GetChangedFriendsAsync(1, 999, null, null, 99);
        var second = await manager.GetChangedFriendsAsync(1, first.Last().Ticks, null, first.Last().Id, 99);
        Assert.Equal(99, first.Count);
        Assert.Equal(51, second.Count);
        Assert.Equal(150, first.Concat(second).Select(x => x.Id).Distinct().Count());
    }
}
