using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.Specifications;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat;

public class MessageReadCache_Tests
{
    private static readonly DateTime Time = new(2026, 10, 8, 1, 0, 0, DateTimeKind.Utc);
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public MessageReadCache_Tests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private sealed class Reader
    {
        public MessageManager Manager { get; }
        public IMessageRepository Repository { get; }
        public ISessionUnitSettingRepository SettingsRepository { get; }
        public ICancellationTokenProvider CancellationTokens { get; }
        public IMessageCacheManager Cache { get; }
        public IAbpDistributedLock Lock { get; } = Substitute.For<IAbpDistributedLock>();
        public SessionUnitCacheItem Unit { get; } = new() { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        public SessionUnitSettingCacheItem Setting { get; } = new() { IsKilled = false };

        public Reader(int maxBatches = 4)
        {
            var constructor = typeof(MessageManager).GetConstructors().Single();
            var arguments = constructor.GetParameters().Select(x =>
                x.ParameterType == typeof(IOptions<MessageOptions>)
                    ? (object)Microsoft.Extensions.Options.Options.Create(new MessageOptions { ReadCacheMaxBatches = maxBatches })
                    : Substitute.For([x.ParameterType], [])).ToArray();
            Manager = (MessageManager)constructor.Invoke(arguments);
            Repository = arguments.OfType<IMessageRepository>().Single();
            SettingsRepository = arguments.OfType<ISessionUnitSettingRepository>().Single();
            Cache = arguments.OfType<IMessageCacheManager>().Single();
            CancellationTokens = arguments.OfType<ICancellationTokenProvider>().Single();
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            Cache.GetReadSettingsAsync(Unit.Id, Arg.Any<Func<Task<SessionUnitSettingCacheItem>>>(),
                Arg.Any<CancellationToken>()).Returns(Setting);
            lazy.LazyGetRequiredService<IAbpDistributedLock>().Returns(Lock);
            Lock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(Substitute.For<IAbpDistributedLockHandle>());
            Cache.GetDeletedIdsAsync(Arg.Any<Guid>(), Arg.Any<List<long>>(),
                    Arg.Any<Func<List<long>, Task<List<long>>>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<long>());
            Manager.LazyServiceProvider = lazy;
        }

        public void Warm(SessionMessageWindowCacheItem window)
            => Cache.GetReadWindowsAsync(Unit.SessionId.Value, Arg.Any<CancellationToken>())
                .Returns((window, (SessionMessageWindowCacheItem)null, "generation"));

        public void AssertNoSqlOrLock()
        {
            Assert.Empty(Repository.ReceivedCalls());
            Assert.Empty(SettingsRepository.ReceivedCalls());
            Assert.Empty(Lock.ReceivedCalls());
        }
    }

    private static SessionMessageWindowCacheItem.Entry Entry(long id) => new() { Id = id, CreationTime = Time };
    private static SessionMessageWindowCacheItem Window(long lower, long upper, params long[] ids)
        => new() { MinMessageId = lower, MaxMessageId = upper, Items = ids.Select(Entry).ToList() };

    [Fact]
    public async Task Constructor_injected_provider_reads_current_token_on_each_call()
    {
        var reader = new Reader();
        reader.Warm(Window(0, long.MaxValue, 103));
        Assert.Equal(new long[] { 103 }, await reader.Manager.GetHistoryAsync(reader.Unit, null, 1));
        reader.CancellationTokens.Token.Returns(new CancellationToken(canceled: true));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.Manager.GetHistoryAsync(reader.Unit, null, 1));
        reader.AssertNoSqlOrLock();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Warm_reads_filter_private_dates_and_deleted_ids_without_message_sql_or_lock(bool descending)
    {
        var reader = new Reader();
        reader.Setting.HistoryFristTime = Time;
        reader.Setting.HistoryLastTime = Time.AddHours(1);
        reader.Setting.ClearTime = Time.AddSeconds(-1);
        var window = Window(0, long.MaxValue, 101, 102, 103, 104, 105, 106, 107, 108);
        window.Items[0].IsPrivate = true;
        window.Items[0].SenderSessionUnitId = Guid.NewGuid();
        window.Items[0].ReceiverSessionUnitId = Guid.NewGuid();
        window.Items[1].CreationTime = Time.AddSeconds(-1);
        window.Items[2].CreationTime = Time.AddHours(1);
        window.Items[4].IsPrivate = true;
        window.Items[4].SenderSessionUnitId = reader.Unit.Id;
        window.Items[5].IsPrivate = true;
        window.Items[5].ReceiverSessionUnitId = reader.Unit.Id;
        reader.Warm(window);
        reader.Cache.GetDeletedIdsAsync(reader.Unit.Id, Arg.Any<List<long>>(),
                Arg.Any<Func<List<long>, Task<List<long>>>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                Assert.Equal(new long[] { 104, 105, 106, 107, 108 }, call.Arg<List<long>>().OrderBy(x => x));
                return new HashSet<long> { 104 };
            });
        var result = descending
            ? await reader.Manager.GetHistoryAsync(reader.Unit, null, 20)
            : await reader.Manager.GetLatestAsync(reader.Unit, 100, 20);
        Assert.Equal(descending ? new long[] { 108, 107, 106, 105 } : new long[] { 105, 106, 107, 108 }, result);
        reader.AssertNoSqlOrLock();
    }

    [Fact]
    public async Task Warm_sparse_pages_use_exclusive_cursors_without_requiring_an_anchor_message()
    {
        var reader = new Reader();
        reader.Warm(Window(0, long.MaxValue, 90, 103, 105, 140, 200));
        Assert.Equal(new long[] { 103, 105 }, await reader.Manager.GetLatestAsync(reader.Unit, 100, 2));
        Assert.Equal(new long[] { 140, 200 }, await reader.Manager.GetLatestAsync(reader.Unit, 105, 2));
        Assert.Equal(new long[] { 140, 105 }, await reader.Manager.GetHistoryAsync(reader.Unit, 199, 2));
        Assert.Equal(new long[] { 103, 90 }, await reader.Manager.GetHistoryAsync(reader.Unit, 105, 2));
        reader.AssertNoSqlOrLock();
    }

    [Fact]
    public async Task Warm_reads_respect_start_end_and_clear_boundaries()
    {
        var reader = new Reader();
        reader.Setting.StartMessageId = 101;
        reader.Setting.EndMessageId = 105;
        reader.Setting.ClearTime = Time;
        var window = Window(0, long.MaxValue, 101, 102, 103, 105, 106);
        foreach (var entry in window.Items.Where(x => x.Id != 102)) entry.CreationTime = Time.AddSeconds(1);
        reader.Warm(window);
        Assert.Equal(new long[] { 103, 105 }, await reader.Manager.GetLatestAsync(reader.Unit, 0, 10));
        Assert.Equal(new long[] { 105, 103 }, await reader.Manager.GetHistoryAsync(reader.Unit, null, 10));
        reader.AssertNoSqlOrLock();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Batch_budget_falls_back_from_last_scanned_id_and_keeps_extra_boundary_message(bool descending)
    {
        var reader = new Reader(maxBatches: 1);
        // 103 is the database's extra row, used as the certified boundary and excluded from this window.
        reader.Warm(descending ? Window(103, long.MaxValue, 104, 105) : Window(0, 103, 101, 102));
        reader.Cache.GetDeletedIdsAsync(reader.Unit.Id, Arg.Any<List<long>>(),
                Arg.Any<Func<List<long>, Task<List<long>>>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<long> { 101, 102, 104, 105 });
        reader.Repository.GetVisibleIdsAsync(reader.Unit.Id, Arg.Any<long?>(), Arg.Any<long?>(),
                2, descending, Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                Assert.Equal(descending ? 0L : 102L, call.ArgAt<long?>(1));
                Assert.Equal(descending ? 104L : long.MaxValue, call.ArgAt<long?>(2));
                return descending ? new List<long> { 103, 99 } : new List<long> { 103, 109 };
            });
        var result = descending
            ? await reader.Manager.GetHistoryAsync(reader.Unit, null, 2)
            : await reader.Manager.GetLatestAsync(reader.Unit, 0, 2);
        Assert.Equal(descending ? new long[] { 103, 99 } : new long[] { 103, 109 }, result);
        Assert.Single(reader.Repository.ReceivedCalls());
        Assert.Empty(reader.Lock.ReceivedCalls());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Killed_member_is_rejected_before_deletion_lookup_or_message_sql(bool descending)
    {
        var reader = new Reader();
        reader.Setting.IsKilled = true;
        await Assert.ThrowsAnyAsync<Exception>(() => descending
            ? reader.Manager.GetHistoryAsync(reader.Unit, null)
            : reader.Manager.GetLatestAsync(reader.Unit, 0));
        await reader.Cache.DidNotReceiveWithAnyArgs().GetDeletedIdsAsync(default, default, default, default);
        reader.AssertNoSqlOrLock();
    }

    [Fact]
    public async Task Cold_read_fetches_one_bounded_window_and_subsequent_read_uses_the_shared_cache()
    {
        var reader = new Reader();
        var filled = false;
        var window = Window(0, long.MaxValue, 103, 109, 140);
        reader.Cache.GetReadWindowsAsync(reader.Unit.SessionId.Value, Arg.Any<CancellationToken>())
            .Returns(_ => (filled ? window : null, (SessionMessageWindowCacheItem)null, "generation"));
        reader.Repository.GetReadWindowAsync(reader.Unit.SessionId.Value, 0, long.MaxValue, 128, false,
            Arg.Any<CancellationToken>()).Returns(window);
        reader.Cache.StoreReadWindowAsync(reader.Unit.SessionId.Value, window, "generation", Arg.Any<CancellationToken>())
            .Returns(_ => { filled = true; return true; });
        Assert.Equal(new long[] { 103, 109 }, await reader.Manager.GetLatestAsync(reader.Unit, 0, 2));
        reader.Repository.ClearReceivedCalls();
        reader.Lock.ClearReceivedCalls();
        Assert.Equal(new long[] { 103, 109 }, await reader.Manager.GetLatestAsync(reader.Unit, 0, 2));
        reader.AssertNoSqlOrLock();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Redis_failure_returns_bounded_authoritative_visible_ids(bool descending)
    {
        var reader = new Reader();
        reader.Cache.GetReadWindowsAsync(reader.Unit.SessionId.Value, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(SessionMessageWindowCacheItem, SessionMessageWindowCacheItem, string)>(
                new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Isolated cache outage")));
        reader.Repository.GetVisibleIdsAsync(reader.Unit.Id, descending ? null : 100L, descending ? 200L : null,
                2, descending, Arg.Any<CancellationToken>())
            .Returns(descending ? new List<long> { 140, 103 } : new List<long> { 103, 140 });
        var result = descending
            ? await reader.Manager.GetHistoryAsync(reader.Unit, 200, 2)
            : await reader.Manager.GetLatestAsync(reader.Unit, 100, 2);
        Assert.Equal(descending ? new long[] { 140, 103 } : new long[] { 103, 140 }, result);
        Assert.Single(reader.Repository.ReceivedCalls());
        Assert.Empty(reader.SettingsRepository.ReceivedCalls());
        Assert.Empty(reader.Lock.ReceivedCalls());
    }

    private static MessageCacheManager CreateCache(ConnectionMultiplexer redis, string prefix, List<Func<Task>> callbacks,
        bool transactional = false)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(redis);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = prefix }));
        lazy.LazyGetRequiredService<IOptions<MessageOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new MessageOptions()));
        lazy.LazyGetRequiredService<IDeletedRecorderManager>().Returns(Substitute.For<IDeletedRecorderManager>());
        var uow = Substitute.For<IUnitOfWork>();
        uow.Options.Returns(new AbpUnitOfWorkOptions { IsTransactional = transactional });
        uow.When(x => x.OnCompleted(Arg.Any<Func<Task>>())).Do(call => callbacks.Add(call.Arg<Func<Task>>()));
        var uows = Substitute.For<IUnitOfWorkManager>();
        uows.Current.Returns(uow);
        lazy.LazyGetRequiredService<IUnitOfWorkManager>().Returns(uows);
        return new MessageCacheManager { LazyServiceProvider = lazy };
    }

    [RedisReviewFact]
    public async Task Transactional_reads_do_not_publish_uncommitted_settings_windows_or_deleted_flags()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var cache = CreateCache(redis, prefix, [], transactional: true);
        var unit = Guid.NewGuid();
        var session = Guid.NewGuid();
        var settingsKey = (RedisKey)$"{prefix}Messages:ReadSettings:{unit}";
        var deletedKey = (RedisKey)$"{prefix}Messages:ReadDeleted:{unit}";
        try
        {
            var settings = await cache.GetReadSettingsAsync(unit,
                () => Task.FromResult(new SessionUnitSettingCacheItem { IsKilled = true }));
            Assert.True(settings.IsKilled);
            Assert.False(await redis.GetDatabase().KeyExistsAsync(settingsKey));
            Assert.False(await cache.StoreReadWindowAsync(session, Window(0, long.MaxValue, 103), ""));
            Assert.Null((await cache.GetReadWindowsAsync(session)).Recent);
            Assert.Equal(new long[] { 103 }, await cache.GetDeletedIdsAsync(unit, [103], _ => Task.FromResult(new List<long> { 103 })));
            Assert.False(await redis.GetDatabase().KeyExistsAsync(deletedKey));
        }
        finally
        {
            await CleanWindows(redis.GetDatabase(), prefix, session);
            await redis.GetDatabase().KeyDeleteAsync([settingsKey, deletedKey]);
        }
    }

    private static Message CreateMessage(SessionUnitCacheItem unit, long id)
    {
        var message = new Message(unit) { CreationTime = Time };
        typeof(Message).GetProperty(nameof(Message.Id)).SetValue(message, id);
        return message;
    }

    [RedisReviewFact]
    public async Task Appends_and_new_deleted_flags_do_not_extend_existing_snapshot_expiration()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        var recentKey = (RedisKey)$"{prefix}Messages:ReadWindow:{unit.SessionId}:Recent";
        var deletedKey = (RedisKey)$"{prefix}Messages:ReadDeleted:{unit.Id}";
        try
        {
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, long.MaxValue, 103), ""));
            await redis.GetDatabase().KeyExpireAsync(recentKey, TimeSpan.FromMinutes(1));
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(CreateMessage(unit, 105)));
            await Assert.Single(callbacks)();
            Assert.True(await redis.GetDatabase().KeyTimeToLiveAsync(recentKey) <= TimeSpan.FromMinutes(1));
            Assert.Equal(new long[] { 103, 105 }, (await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent.Items.Select(x => x.Id));
            await cache.GetDeletedIdsAsync(unit.Id, [103], _ => Task.FromResult(new List<long>()));
            await redis.GetDatabase().KeyExpireAsync(deletedKey, TimeSpan.FromMinutes(1));
            await cache.GetDeletedIdsAsync(unit.Id, [105], _ => Task.FromResult(new List<long> { 105 }));
            Assert.True(await redis.GetDatabase().KeyTimeToLiveAsync(deletedKey) <= TimeSpan.FromMinutes(1));
            Assert.Equal(new long[] { 105 }, await cache.GetDeletedIdsAsync(unit.Id, [103, 105],
                _ => throw new InvalidOperationException("Warm deletion flags must not query SQL.")));
        }
        finally
        {
            await CleanWindows(redis.GetDatabase(), prefix, unit.SessionId.Value);
            await redis.GetDatabase().KeyDeleteAsync([deletedKey, (RedisKey)$"{prefix}Messages:ReadDeleted:{unit.Id}:Version"]);
        }
    }

    private static Task CleanWindows(IDatabase database, string prefix, Guid session)
        => database.KeyDeleteAsync(new RedisKey[]
        {
            $"{prefix}Messages:ReadWindow:{session}:Version",
            $"{prefix}Messages:ReadWindow:{session}:Recent",
            $"{prefix}Messages:ReadWindow:{session}:History"
        });

    [RedisReviewFact]
    public async Task Settings_generation_blocks_stale_refill_and_commit_invalidates_a_precommit_refill()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = Guid.NewGuid();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<SessionUnitSettingCacheItem>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            var pending = cache.GetReadSettingsAsync(unit, () =>
            {
                started.SetResult(true);
                return release.Task;
            });
            await started.Task;
            await cache.HandleEventAsync(new EntityUpdatedEventData<SessionUnitSetting>(new SessionUnitSetting { SessionUnitId = unit }));
            release.SetResult(new SessionUnitSettingCacheItem { IsKilled = false });
            await pending;
            Assert.False(await redis.GetDatabase().KeyExistsAsync($"{prefix}Messages:ReadSettings:{unit}"));
            // A reader before SQL commit may refill old committed settings; OnCompleted clears them again.
            await cache.GetReadSettingsAsync(unit, () => Task.FromResult(new SessionUnitSettingCacheItem { IsKilled = false }));
            await Assert.Single(callbacks)();
            var changed = await cache.GetReadSettingsAsync(unit,
                () => Task.FromResult(new SessionUnitSettingCacheItem { IsKilled = true }));
            Assert.True(changed.IsKilled);
            Assert.True((await cache.GetReadSettingsAsync(unit,
                () => throw new InvalidOperationException("Warm visibility settings must not query SQL."))).IsKilled);
            // Rollback leaves the database authoritative: precommit invalidation permits a fresh old-state refill.
            callbacks.Clear();
            await cache.HandleEventAsync(new EntityUpdatedEventData<SessionUnitSetting>(new SessionUnitSetting { SessionUnitId = unit }));
            Assert.Single(callbacks);
            var rolledBack = await cache.GetReadSettingsAsync(unit,
                () => Task.FromResult(new SessionUnitSettingCacheItem { IsKilled = false }));
            Assert.False(rolledBack.IsKilled);
        }
        finally
        {
            await redis.GetDatabase().KeyDeleteAsync(new RedisKey[]
            {
                $"{prefix}Messages:ReadSettings:{unit}", $"{prefix}Messages:ReadSettings:{unit}:Version"
            });
        }
    }

    [RedisReviewFact]
    public async Task Cold_window_cas_rejects_a_snapshot_invalidated_while_database_query_was_in_flight()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        try
        {
            var snapshot = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            await cache.HandleEventAsync(new EntityUpdatedEventData<Message>(CreateMessage(unit, 103)));
            Assert.False(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, long.MaxValue, 103), snapshot.Version));
            var current = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            Assert.Null(current.Recent);
            Assert.NotEqual(snapshot.Version, current.Version);
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, long.MaxValue, 103), current.Version));
            await Assert.Single(callbacks)();
            Assert.Null((await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent);
        }
        finally { await CleanWindows(redis.GetDatabase(), prefix, unit.SessionId.Value); }
    }

    [RedisReviewFact]
    public async Task Deleted_hash_invalidation_prevents_an_old_negative_result_repopulating_the_new_generation()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = Guid.NewGuid();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<List<long>>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            var pending = cache.GetDeletedIdsAsync(unit, [103, 105], ids =>
            {
                Assert.Equal(new long[] { 103, 105 }, ids);
                started.SetResult(true);
                return release.Task;
            });
            await started.Task;
            await cache.HandleEventAsync(new EntityCreatedEventData<DeletedRecorder>(new DeletedRecorder(unit, 103)));
            release.SetResult([]);
            Assert.Empty(await pending);
            // The stale snapshot was returned to its already-running reader but must not poison later readers.
            var fetches = 0;
            var current = await cache.GetDeletedIdsAsync(unit, [103, 105], ids =>
            {
                fetches++;
                Assert.Equal(new long[] { 103, 105 }, ids);
                return Task.FromResult(new List<long> { 103 });
            });
            Assert.Equal(new long[] { 103 }, current);
            Assert.Equal(1, fetches);
            Assert.Equal(new long[] { 103 }, await cache.GetDeletedIdsAsync(unit, [103, 105],
                _ => throw new InvalidOperationException("Warm deletion flags must not query SQL.")));
            await Assert.Single(callbacks)();
            Assert.False(await redis.GetDatabase().KeyExistsAsync($"{prefix}Messages:ReadDeleted:{unit}"));
        }
        finally
        {
            await redis.GetDatabase().KeyDeleteAsync(new RedisKey[]
            {
                $"{prefix}Messages:ReadDeleted:{unit}", $"{prefix}Messages:ReadDeleted:{unit}:Version"
            });
        }
    }

    [RedisReviewFact]
    public async Task Creation_appends_only_after_commit_uses_assigned_id_and_preserves_long_precision_and_older_history()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        const long first = 9007199254740993;
        try
        {
            var snapshot = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(first - 1, long.MaxValue, first), snapshot.Version));
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, first - 1, 100, 200), snapshot.Version));
            var message = CreateMessage(unit, 0);
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(message));
            Assert.Equal(new long[] { first }, (await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent.Items.Select(x => x.Id));
            typeof(Message).GetProperty(nameof(Message.Id)).SetValue(message, first + 2);
            await Assert.Single(callbacks)();
            var committed = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            Assert.Equal(new long[] { first, first + 2 }, committed.Recent.Items.Select(x => x.Id));
            Assert.Equal(new long[] { 100, 200 }, committed.History.Items.Select(x => x.Id));
            Assert.NotEqual(snapshot.Version, committed.Version);
            // A rolled-back unit of work never invokes its OnCompleted callback.
            callbacks.Clear();
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(CreateMessage(unit, first + 4)));
            Assert.Single(callbacks);
            Assert.Equal(new long[] { first, first + 2 }, (await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent.Items.Select(x => x.Id));
        }
        finally { await CleanWindows(redis.GetDatabase(), prefix, unit.SessionId.Value); }
    }

    [RedisReviewFact]
    public async Task Empty_recent_window_accepts_first_committed_message_and_remains_deserializable()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        try
        {
            var snapshot = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, long.MaxValue), snapshot.Version));
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(CreateMessage(unit, 103)));
            await Assert.Single(callbacks)();
            var recent = (await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent;
            Assert.Equal(103, Assert.Single(recent.Items).Id);
            Assert.Equal(0, recent.MinMessageId);
            Assert.Equal(long.MaxValue, recent.MaxMessageId);
        }
        finally { await CleanWindows(redis.GetDatabase(), prefix, unit.SessionId.Value); }
    }

    [RedisReviewFact]
    public async Task Recent_window_trims_to_128_sorts_late_commits_and_deduplicates_retried_append()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        try
        {
            var snapshot = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            var originalIds = Enumerable.Range(0, 128).Select(i => 1000L + 2 * i).ToArray();
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(999, long.MaxValue, originalIds), snapshot.Version));
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(CreateMessage(unit, 1256)));
            await Assert.Single(callbacks)();
            var appended = (await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent;
            Assert.Equal(128, appended.Items.Count);
            Assert.Equal(1000, appended.MinMessageId);
            Assert.Equal(1002, appended.Items[0].Id);
            Assert.Equal(1256, appended.Items[^1].Id);
            // Lower Id can commit after a newer transaction; insert it in order inside the certified window.
            callbacks.Clear();
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(CreateMessage(unit, 1065)));
            var lateCommit = Assert.Single(callbacks);
            await lateCommit();
            await lateCommit();
            var late = (await cache.GetReadWindowsAsync(unit.SessionId.Value)).Recent;
            var expected = originalIds.Skip(2).Append(1256L).Append(1065L).OrderBy(x => x);
            Assert.Equal(expected, late.Items.Select(x => x.Id));
            Assert.Equal(1002, late.MinMessageId);
            Assert.Equal(128, late.Items.Count);
            Assert.Single(late.Items.Where(x => x.Id == 1065));
        }
        finally { await CleanWindows(redis.GetDatabase(), prefix, unit.SessionId.Value); }
    }

    [RedisReviewFact]
    public async Task A_late_commit_inside_a_certified_history_window_invalidates_history_without_changing_recent()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var callbacks = new List<Func<Task>>();
        var cache = CreateCache(redis, prefix, callbacks);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        try
        {
            var snapshot = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(1000, long.MaxValue, 1100), snapshot.Version));
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, 1000, 100, 200, 300), snapshot.Version));
            await cache.HandleEventAsync(new EntityCreatedEventData<Message>(CreateMessage(unit, 250)));
            Assert.NotNull((await cache.GetReadWindowsAsync(unit.SessionId.Value)).History);
            await Assert.Single(callbacks)();
            var committed = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            Assert.Null(committed.History);
            Assert.Equal(1100, Assert.Single(committed.Recent.Items).Id);
            Assert.NotEqual(snapshot.Version, committed.Version);
        }
        finally { await CleanWindows(redis.GetDatabase(), prefix, unit.SessionId.Value); }
    }

    [RedisReviewFact]
    public async Task Five_hundred_warm_cache_filter_reads_measure_latency_without_database_fetches()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("CHAT_REVIEW_REDIS"));
        var prefix = $"Review:MessageRead:{Guid.NewGuid():N}:";
        var cache = CreateCache(redis, prefix, []);
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), OwnerId = 1, SessionId = Guid.NewGuid() };
        try
        {
            var snapshot = await cache.GetReadWindowsAsync(unit.SessionId.Value);
            var ids = Enumerable.Range(0, 128).Select(i => 1000L + i).ToArray();
            Assert.True(await cache.StoreReadWindowAsync(unit.SessionId.Value, Window(0, long.MaxValue, ids), snapshot.Version));
            await cache.GetReadSettingsAsync(unit.Id, () => Task.FromResult(new SessionUnitSettingCacheItem { IsKilled = false }));
            Assert.Empty(await cache.GetDeletedIdsAsync(unit.Id, ids.ToList(), _ => Task.FromResult(new List<long>())));

            async Task ReadWarmAsync()
            {
                var settingsTask = cache.GetReadSettingsAsync(unit.Id,
                    () => throw new InvalidOperationException("Warm visibility settings must not query SQL."));
                var windowsTask = cache.GetReadWindowsAsync(unit.SessionId.Value);
                await Task.WhenAll(settingsTask, windowsTask);
                var settings = await settingsTask;
                var candidates = (await windowsTask).Recent.Items
                    .Where(x => OwnerMessageSpecification.IsVisible(x, unit.Id, settings)).ToList();
                var deleted = await cache.GetDeletedIdsAsync(unit.Id, candidates.Select(x => x.Id).ToList(),
                    _ => throw new InvalidOperationException("Warm deletion flags must not query SQL."));
                Assert.Equal(128, candidates.Count(x => !deleted.Contains(x.Id)));
            }

            await ReadWarmAsync();
            const int iterations = 500;
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++) await ReadWarmAsync();
            timer.Stop();
            _output.WriteLine($"Redis warm cache filtering: {iterations} reads, 128 metadata entries and deletion flags; " +
                $"total {timer.Elapsed.TotalMilliseconds:F2} ms, average {timer.Elapsed.TotalMilliseconds / iterations:F3} ms/read. " +
                "Settings and windows are fetched concurrently; deletion flags follow. This measures cache filtering only, not full HTTP requests or message content retrieval.");
        }
        finally
        {
            var database = redis.GetDatabase();
            await CleanWindows(database, prefix, unit.SessionId.Value);
            await database.KeyDeleteAsync(new RedisKey[]
            {
                $"{prefix}Messages:ReadSettings:{unit.Id}", $"{prefix}Messages:ReadSettings:{unit.Id}:Version",
                $"{prefix}Messages:ReadDeleted:{unit.Id}", $"{prefix}Messages:ReadDeleted:{unit.Id}:Version"
            });
        }
    }
}
