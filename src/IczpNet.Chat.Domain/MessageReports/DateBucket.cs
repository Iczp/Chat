using IczpNet.Chat.Enums;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace IczpNet.Chat.MessageReports;

public class DateBucket : IDateBucket, ISingletonDependency
{
    private readonly Lazy<Task<TimeZoneInfo>> _platformTimeZone;

    public DateBucket(IServiceScopeFactory serviceScopeFactory)
    {
        // The platform calendar changes through a coordinated deployment. Cache
        // immutable zone information per process, without keeping a settings scope alive.
        _platformTimeZone = new Lazy<Task<TimeZoneInfo>>(() => ResolveTimeZoneAsync(serviceScopeFactory));
    }

    private static async Task<TimeZoneInfo> ResolveTimeZoneAsync(IServiceScopeFactory serviceScopeFactory)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var definition = await scope.ServiceProvider.GetRequiredService<ISettingDefinitionManager>()
            .GetAsync(TimingSettingNames.TimeZone);
        var providers = scope.ServiceProvider.GetRequiredService<ISettingValueProviderManager>().Providers;

        // Platform reports have one calendar; user and tenant display settings must not move a bucket.
        foreach (var providerName in new[] { GlobalSettingValueProvider.ProviderName, ConfigurationSettingValueProvider.ProviderName, DefaultValueSettingValueProvider.ProviderName })
        {
            var provider = providers.FirstOrDefault(x => x.Name == providerName);
            if (provider == null || (definition.Providers.Any() && !definition.Providers.Contains(providerName)))
            {
                continue;
            }

            var value = await provider.GetOrNullAsync(definition);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            return scope.ServiceProvider.GetRequiredService<ITimezoneProvider>().GetTimeZoneInfo(value);
        }

        throw new InvalidOperationException($"No platform time zone is configured for {TimingSettingNames.TimeZone}.");
    }

    public async Task<DateTime> GetBusinessTimeAsync(DateTime dateTime)
    {
        var zone = await _platformTimeZone.Value;
        var utc = dateTime.Kind == DateTimeKind.Local
            ? dateTime.ToUniversalTime()
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
    }

    public async Task<long> CreateAsync(DateTime dateTime, string format = "yyyyMMdd")
    {
        return Create(await GetBusinessTimeAsync(dateTime), format);
    }

    public long Create(DateTime dateTime, string format = "yyyyMMdd")
    {
        return long.Parse(dateTime.ToString(format, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }

    public long Create(DateTime dateTime, MessageReportTypes reportType)
    {
        return reportType switch
        {
            MessageReportTypes.Month => Create(dateTime, "yyyyMM"),
            MessageReportTypes.Day => Create(dateTime, "yyyyMMdd"),
            MessageReportTypes.Hour => Create(dateTime, "yyyyMMddHH"),
            _ => throw new NotImplementedException(),
        };
    }
}
