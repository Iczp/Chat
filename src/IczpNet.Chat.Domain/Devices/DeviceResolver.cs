using IczpNet.AppDevice.Devices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Devices;

// Compatibility contract for existing Chat consumers; device values come from AppDevice.
public class DeviceResolver(
    IOptions<DeviceOptions> option,
    IHttpContextAccessor httpContextAccessor,
    ICurrentDevice currentDevice) : DomainService, IDeviceResolver
{
    protected DeviceOptions Config => option.Value;

    public virtual string GetParameter(string key)
    {
        if (string.Equals(key, Config.RequestDeviceIdKey, StringComparison.OrdinalIgnoreCase)) return GetDeviceId();
        if (string.Equals(key, Config.RequestDeviceTypeKey, StringComparison.OrdinalIgnoreCase)) return GetDeviceType();
        if (string.Equals(key, Config.RequestAppIdKey, StringComparison.OrdinalIgnoreCase)) return GetAppId();
        if (string.Equals(key, Config.RequestAppVersionKey, StringComparison.OrdinalIgnoreCase)) return GetAppVersion();
        // Preserve the legacy general-purpose accessor for external callers.
        var request = httpContextAccessor.HttpContext?.Request;
        var header = request?.Headers[key].ToString();
        return !string.IsNullOrEmpty(header) ? header : request?.Query[key].ToString();
    }

    public virtual string GetDeviceIdKey() => Config.RequestDeviceIdKey;
    public virtual string GetDeviceId() => EmptyToNull(currentDevice.Get().DeviceId);
    public virtual Task<string> GetDeviceIdAsync() => Task.FromResult(GetDeviceId());
    public virtual string GetDeviceTypeKey() => Config.RequestDeviceTypeKey;
    public virtual string GetDeviceType() => currentDevice.Get().DeviceType;
    public virtual Task<string> GetDeviceTypeAsync() => Task.FromResult(GetDeviceType());
    public virtual string GetAppIdKey() => Config.RequestAppIdKey;
    public virtual string GetAppId() => currentDevice.Get().AppId;
    public virtual Task<string> GetAppIdAsync() => Task.FromResult(GetAppId());
    public virtual string GetAppVersionKey() => Config.RequestAppVersionKey;
    public virtual string GetAppVersion() => currentDevice.Get().AppVersion;
    public virtual Task<string> GetAppVersionAsync() => Task.FromResult(GetAppVersion());
    public virtual Task<bool> IsEqualsAsync(string inputDeviceId)
        => Task.FromResult(!string.IsNullOrWhiteSpace(inputDeviceId) && string.Equals(GetDeviceId(), inputDeviceId, StringComparison.Ordinal));
    private static string EmptyToNull(string value) => string.IsNullOrEmpty(value) ? null : value;
}
