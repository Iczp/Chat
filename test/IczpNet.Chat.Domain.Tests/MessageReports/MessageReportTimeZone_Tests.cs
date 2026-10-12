using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.MessageReports;

public class MessageReportTimeZone_Tests
{
    private sealed class CalendarFixture : IDisposable
    {
        public ISettingValueProvider Global { get; } = Provider("G", null);
        public ISettingValueProvider Configuration { get; } = Provider("C", "Asia/Shanghai");
        public ISettingValueProvider User { get; } = Provider("U", "America/New_York");
        public ISettingValueProvider Tenant { get; } = Provider("T", "America/New_York");
        private readonly ServiceProvider _services;
        public DateBucket Calendar { get; }

        public CalendarFixture()
        {
            var definitions = Substitute.For<ISettingDefinitionManager>();
            definitions.GetAsync(TimingSettingNames.TimeZone).Returns(new SettingDefinition(TimingSettingNames.TimeZone, "UTC"));
            var providers = Substitute.For<ISettingValueProviderManager>();
            var defaultProvider = Provider("D", "UTC");
            providers.Providers.Returns(new List<ISettingValueProvider> { User, Tenant, Global, Configuration, defaultProvider });
            var timezones = Substitute.For<ITimezoneProvider>();
            timezones.GetTimeZoneInfo(Arg.Any<string>()).Returns(call => TimeZoneInfo.FindSystemTimeZoneById(call.Arg<string>()));
            _services = new ServiceCollection()
                .AddSingleton(definitions).AddSingleton(providers).AddSingleton(timezones).BuildServiceProvider();
            Calendar = new DateBucket(_services.GetRequiredService<IServiceScopeFactory>());
        }

        private static ISettingValueProvider Provider(string name, string value)
        {
            var provider = Substitute.For<ISettingValueProvider>();
            provider.Name.Returns(name);
            provider.GetOrNullAsync(Arg.Any<SettingDefinition>()).Returns(Task.FromResult(value));
            return provider;
        }

        public void Dispose() => _services.Dispose();
    }

    [Theory]
    [InlineData("2026-10-11T15:59:59Z", 20261011L, 2026101123L)]
    [InlineData("2026-10-11T16:00:00Z", 20261012L, 2026101200L)]
    [InlineData("2026-10-12T05:00:00Z", 20261012L, 2026101213L)]
    [InlineData("2026-10-12T16:00:00Z", 20261013L, 2026101300L)]
    public async Task Platform_calendar_uses_Shanghai_boundaries_and_24_hour_buckets(string instant, long day, long hour)
    {
        using var fixture = new CalendarFixture();
        var utc = DateTimeOffset.Parse(instant).UtcDateTime;
        var local = await fixture.Calendar.GetBusinessTimeAsync(utc);

        Assert.Equal(day, fixture.Calendar.Create(local, MessageReportTypes.Day));
        Assert.Equal(hour, fixture.Calendar.Create(local, MessageReportTypes.Hour));
        Assert.Equal(day, await fixture.Calendar.CreateAsync(utc));
        await fixture.Global.Received(1).GetOrNullAsync(Arg.Any<SettingDefinition>());
        await fixture.Configuration.Received(1).GetOrNullAsync(Arg.Any<SettingDefinition>());
        await fixture.User.DidNotReceiveWithAnyArgs().GetOrNullAsync(default);
        await fixture.Tenant.DidNotReceiveWithAnyArgs().GetOrNullAsync(default);
    }

    [Fact]
    public async Task Global_calendar_takes_precedence_over_configuration()
    {
        using var fixture = new CalendarFixture();
        fixture.Global.GetOrNullAsync(Arg.Any<SettingDefinition>()).Returns(Task.FromResult("UTC"));

        Assert.Equal(20261011L, await fixture.Calendar.CreateAsync(new DateTime(2026, 10, 11, 16, 0, 0, DateTimeKind.Utc)));
        await fixture.Configuration.DidNotReceiveWithAnyArgs().GetOrNullAsync(default);
    }

    [Fact]
    public async Task Delayed_message_uses_occurrence_bucket_and_default_flush_uses_business_today()
    {
        using var fixture = new CalendarFixture();
        var manager = CreateManager(fixture.Calendar, new DateTime(2026, 10, 12, 16, 1, 0, DateTimeKind.Utc), out var database);
        var message = new MessageCacheItem
        {
            SessionId = Guid.NewGuid(), MessageType = MessageTypes.Text,
            CreationTime = new DateTime(2026, 10, 12, 5, 0, 0, DateTimeKind.Utc)
        };
        var field = (RedisValue)$"{message.SessionId}:{message.MessageType}";

        await manager.StatAsync(message);
        await manager.FlushAsync(MessageReportTypes.Day);

        await database.Received(1).HashIncrementAsync("Test:MessageReports:Day:20261012", field, 1L, CommandFlags.None);
        await database.Received(1).HashIncrementAsync("Test:MessageReports:Hour:2026101213", field, 1L, CommandFlags.None);
        await database.Received(1).HashIncrementAsync("Test:MessageReports:Month:202610", field, 1L, CommandFlags.None);
        await database.Received(1).KeyExistsAsync("Test:MessageReports:Day:20261013", CommandFlags.None);
        await fixture.Global.Received(1).GetOrNullAsync(Arg.Any<SettingDefinition>());
    }

    [Fact]
    public async Task Compensation_checks_the_same_business_calendar_in_a_bounded_pipeline()
    {
        using var fixture = new CalendarFixture();
        var manager = CreateManager(fixture.Calendar, new DateTime(2026, 10, 11, 16, 0, 0, DateTimeKind.Utc), out var database);
        var batch = Substitute.For<IBatch>();
        database.CreateBatch(Arg.Any<object>()).Returns(batch);

        await manager.CompensateAsync();

        batch.Received(1).Execute();
        await batch.Received(1).KeyExistsAsync("Test:MessageReports:Hour:2026101200", CommandFlags.None);
        await batch.Received(1).KeyExistsAsync("Test:MessageReports:Hour:2026101123", CommandFlags.None);
        await batch.Received(1).KeyExistsAsync("Test:MessageReports:Day:20261012", CommandFlags.None);
        await batch.Received(1).KeyExistsAsync("Test:MessageReports:Month:202610", CommandFlags.None);
        await batch.Received(79).KeyExistsAsync(Arg.Any<RedisKey>(), CommandFlags.None);
    }

    [Fact]
    public async Task Events_older_than_compensation_window_attempt_to_flush_their_original_buckets()
    {
        using var fixture = new CalendarFixture();
        var manager = CreateManager(fixture.Calendar, new DateTime(2026, 10, 11, 16, 0, 0, DateTimeKind.Utc), out var database);
        await manager.StatAsync(new MessageCacheItem
        {
            SessionId = Guid.NewGuid(), MessageType = MessageTypes.Text,
            CreationTime = new DateTime(2026, 1, 5, 5, 0, 0, DateTimeKind.Utc)
        });

        await database.Received(1).KeyExistsAsync("Test:MessageReports:Hour:2026010513", CommandFlags.None);
        await database.Received(1).KeyExistsAsync("Test:MessageReports:Day:20260105", CommandFlags.None);
        await database.Received(1).KeyExistsAsync("Test:MessageReports:Month:202601", CommandFlags.None);
    }

    private static MessageReportManager CreateManager(DateBucket calendar, DateTime now, out IDatabase database)
    {
        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        database = Substitute.For<IDatabase>();
        var connection = Substitute.For<IConnectionMultiplexer>();
        connection.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        lazy.LazyGetRequiredService<IConnectionMultiplexer>().Returns(connection);
        lazy.LazyGetRequiredService<IOptions<AbpDistributedCacheOptions>>()
            .Returns(Microsoft.Extensions.Options.Options.Create(new AbpDistributedCacheOptions { KeyPrefix = "Test:" }));
        lazy.LazyGetService<ILogger>(Arg.Any<Func<IServiceProvider, ILogger>>()).Returns(NullLogger.Instance);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(now);
        lazy.LazyGetRequiredService<IClock>().Returns(clock);
        return new MessageReportManager(Substitute.For<IAbpDistributedLock>(), calendar,
            Microsoft.Extensions.Options.Options.Create(new MessageReportOptions()), Substitute.For<IMessageReportDayRepository>(),
            Substitute.For<IMessageReportMonthRepository>(), Substitute.For<IMessageReportHourRepository>())
        { LazyServiceProvider = lazy };
    }
}
