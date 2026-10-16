using System.Security.Claims;
using System.Threading.Tasks;
using IczpNet.AppDevice;
using IczpNet.Chat.Devices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace IczpNet.Chat;

public class DeviceResolver_Tests
{
    [Fact]
    public async Task Chat_uses_client_identity_and_request_snapshot()
    {
        var http = new DefaultHttpContext();
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("device_id", "signed") }, "test"));
        http.Request.Headers["App-Device-Id"] = "header";
        http.Request.Headers["App-DeviceType"] = "phone";
        http.Request.Headers["App-Id"] = "GotoIM";
        var accessor = new HttpContextAccessor { HttpContext = http };
        var current = new HttpCurrentDevice(accessor, Microsoft.Extensions.Options.Options.Create(new DeviceParsingOptions()));
        var resolver = new DeviceResolver(Microsoft.Extensions.Options.Options.Create(new DeviceOptions()), accessor, current);
        Assert.Equal("header", await resolver.GetDeviceIdAsync());
        Assert.Equal("phone", resolver.GetDeviceType());
        Assert.Equal("GotoIM", resolver.GetAppId());
        Assert.False(current.Get().HasConflict);
        http.Request.Headers["App-Device-Id"] = "changed";
        Assert.Equal("header", resolver.GetParameter(resolver.GetDeviceIdKey()));
        Assert.Equal("header", resolver.GetParameter("app-device-id"));
        Assert.True(await resolver.IsEqualsAsync("header"));
        Assert.False(await resolver.IsEqualsAsync("HEADER"));
    }

    [Theory]
    [InlineData("/signalr-hubs/chat", "?deviceId=canonical", "canonical")]
    [InlineData("/signalr-hubs/chat", "?App-Device-Id=legacy", "legacy")]
    [InlineData("/api/chat/other", "?deviceId=ignored", null)]
    public void Chat_query_fallback_is_limited_to_hub_paths(string path, string query, string expected)
    {
        var http = new DefaultHttpContext(); http.Request.Path = path; http.Request.QueryString = new QueryString(query);
        var accessor = new HttpContextAccessor { HttpContext = http };
        var resolver = new DeviceResolver(Microsoft.Extensions.Options.Options.Create(new DeviceOptions()), accessor,
            new HttpCurrentDevice(accessor, Microsoft.Extensions.Options.Options.Create(new DeviceParsingOptions { AllowQuery = true })));
        Assert.Equal(expected, resolver.GetDeviceId());
    }

    [Fact]
    public async Task Missing_device_in_background_context_is_safe()
    {
        var accessor = new HttpContextAccessor();
        var resolver = new DeviceResolver(Microsoft.Extensions.Options.Options.Create(new DeviceOptions()), accessor,
            new HttpCurrentDevice(accessor, Microsoft.Extensions.Options.Options.Create(new DeviceParsingOptions())));
        Assert.Null(resolver.GetDeviceId());
        Assert.False(await resolver.IsEqualsAsync("missing"));
    }
}
