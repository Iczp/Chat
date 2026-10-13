using System;
using System.Text;
using System.Text.Json;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Volo.Abp.Caching;
using Volo.Abp.Json;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Json.SystemTextJson.JsonConverters;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Timing;
using Xunit;

namespace IczpNet.Chat.RedisMapping;

[DependsOn(typeof(AbpJsonSystemTextJsonModule))]
public class MessageJsonTimeZoneTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpClockOptions>(options => options.Kind = DateTimeKind.Utc);
        Configure<AbpJsonOptions>(options => options.InputDateTimeFormats =
            ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd"]);
        var timezone = Substitute.For<ICurrentTimezoneProvider>();
        timezone.TimeZone.Returns("Asia/Shanghai");
        context.Services.AddSingleton(timezone);
    }
}

public class MessageJsonTimeZone_Tests : AbpIntegratedTest<MessageJsonTimeZoneTestModule>
{
    [Theory]
    [InlineData("2026-10-12T10:12:16Z")]
    [InlineData("2026-10-12T14:46:00Z")]
    [InlineData("2026-10-12T23:59:59.1234567Z")]
    public void Response_and_nested_last_message_cache_preserve_the_same_instant(string date)
    {
        var instant = DateTimeOffset.Parse(date).UtcDateTime;
        var message = new MessageCacheItem
        {
            CreationTime = instant,
            RollbackTime = instant,
            QuoteMessage = new MessageQuoteCacheItem { CreationTime = instant }
        };
        var serializer = GetRequiredService<IJsonSerializer>();
        var cache = new Utf8JsonDistributedCacheSerializer(serializer);
        var bytes = cache.Serialize(message);
        var restored = cache.Deserialize<MessageCacheItem>(bytes);

        Assert.Equal(instant, restored.CreationTime);
        Assert.Equal(DateTimeKind.Utc, restored.CreationTime.Kind);
        Assert.Equal(instant, restored.RollbackTime);
        Assert.Equal(instant, restored.QuoteMessage.CreationTime);
        Assert.Equal(DateTimeKind.Utc, restored.QuoteMessage.CreationTime.Kind);

        // The changes endpoint nests the same MessageCacheItem in lastMessage.
        var response = serializer.Serialize(new { LastMessage = restored });
        using var json = JsonDocument.Parse(response);
        var value = json.RootElement.GetProperty("lastMessage").GetProperty("creationTime");
        Assert.EndsWith("Z", value.GetString());
        Assert.Equal(instant, value.GetDateTime());
    }

    [Fact]
    public void Legacy_output_loses_twelve_hour_information()
    {
        GetRequiredService<IOptions<AbpJsonOptions>>().Value.OutputDateTimeFormat = "yyyy-MM-dd hh:mm:ss";
        var serializer = GetRequiredService<IJsonSerializer>();
        var response = serializer.Serialize(new MessageCacheItem
        {
            CreationTime = new DateTime(2026, 10, 12, 14, 46, 0, DateTimeKind.Utc)
        });

        using var json = JsonDocument.Parse(response);
        Assert.Equal("2026-10-12 02:46:00", json.RootElement.GetProperty("creationTime").GetString());
    }

    [Fact]
    public void Legacy_cache_reinterprets_UTC_as_Shanghai_and_loses_eight_hours()
    {
        GetRequiredService<IOptions<AbpJsonOptions>>().Value.OutputDateTimeFormat = "yyyy-MM-dd hh:mm:ss";
        var cache = new Utf8JsonDistributedCacheSerializer(GetRequiredService<IJsonSerializer>());
        var bytes = cache.Serialize(new MessageCacheItem
        {
            CreationTime = new DateTime(2026, 10, 12, 10, 12, 16, DateTimeKind.Utc)
        });
        using var json = JsonDocument.Parse(Encoding.UTF8.GetString(bytes));
        Assert.Equal("2026-10-12 10:12:16", json.RootElement.GetProperty("creationTime").GetString());

        var restored = cache.Deserialize<MessageCacheItem>(bytes);
        Assert.Equal(new DateTime(2026, 10, 12, 2, 12, 16, DateTimeKind.Utc), restored.CreationTime);
        Assert.Equal(DateTimeKind.Utc, restored.CreationTime.Kind);
    }

    [Theory]
    [InlineData("2026-10-12T10:12:16Z")]
    [InlineData("2026-10-12T18:12:16+08:00")]
    [InlineData("2026-10-12 18:12:16")]
    public void ISO_and_legacy_24_hour_inputs_resolve_to_the_same_instant(string date)
    {
        Assert.Equal(DateTimeKind.Utc, GetRequiredService<IClock>().Kind);
        var serializer = GetRequiredService<IJsonSerializer>();
        var message = serializer.Deserialize<MessageCacheItem>("{\"creationTime\":\"" + date + "\"}");

        // IJsonSerializer preserves explicit offsets. Compare instants rather
        // than requiring its DateTime.Kind to match the MVC input converter.
        var instant = new DateTime(2026, 10, 12, 10, 12, 16, DateTimeKind.Utc);
        Assert.Equal(instant, message.CreationTime.ToUniversalTime());
        using var response = JsonDocument.Parse(serializer.Serialize(message));
        Assert.Equal(instant, response.RootElement.GetProperty("creationTime").GetDateTime().ToUniversalTime());
    }

    [Theory]
    [InlineData("2026-10-12T10:12:16Z")]
    [InlineData("2026-10-12T18:12:16+08:00")]
    [InlineData("2026-10-12 18:12:16")]
    public void MVC_date_converter_normalizes_inputs_and_writes_UTC(string date)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(GetRequiredService<AbpDateTimeConverter>());
        var instant = new DateTime(2026, 10, 12, 10, 12, 16, DateTimeKind.Utc);
        var parsed = JsonSerializer.Deserialize<DateTime>("\"" + date + "\"", options);

        Assert.Equal(instant, parsed);
        Assert.Equal(DateTimeKind.Utc, parsed.Kind);
        Assert.Equal("\"2026-10-12T10:12:16Z\"", JsonSerializer.Serialize(parsed, options));
    }
}
