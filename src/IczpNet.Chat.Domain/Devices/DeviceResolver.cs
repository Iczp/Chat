using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Devices;

public class DeviceResolver(
    IOptions<DeviceOptions> option,
    IHttpContextAccessor httpContextAccessor) : DomainService, IDeviceResolver
{
    protected IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;

    protected IOptions<DeviceOptions> Options { get; } = option;

    protected DeviceOptions Config => Options.Value ?? new DeviceOptions();

    protected virtual HttpContext GetHttpContext()
    {
        return HttpContextAccessor.HttpContext;
    }

    /// <summary>
    ///  headers | Request.Query
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual string GetParameter(string key)
    {
        var httpContext = GetHttpContext();

        if (httpContext == null)
        {
            return null;
        }

        var headers = httpContext?.Request.Headers;

        if (!string.IsNullOrEmpty(headers[key]))
        {
            return headers[key];
        }

        if (!string.IsNullOrEmpty(httpContext?.Request.Query[key]))
        {
            return httpContext?.Request.Query[key];
        }

        return null;
    }
    public virtual string GetDeviceIdKey()
    {
        return Config.RequestDeviceIdKey;
    }

    public virtual string GetDeviceId()
    {
        return GetParameter(Config.RequestDeviceIdKey);
    }
    public virtual async Task<string> GetDeviceIdAsync()
    {
        await Task.Yield();
        return GetDeviceId();
    }



    public virtual string GetDeviceTypeKey()
    {
        return Config.RequestDeviceTypeKey;
    }

    public virtual string GetDeviceType()
    {
        return GetParameter(Config.RequestDeviceTypeKey);
    }

    public virtual async Task<string> GetDeviceTypeAsync()
    {
        await Task.Yield();
        return GetDeviceType();
    }

    public virtual string GetAppIdKey()
    {
        return Config.RequestAppIdKey;
    }

    public virtual string GetAppId()
    {
        return GetParameter(Config.RequestAppIdKey);
    }

    public virtual async Task<string> GetAppIdAsync()
    {
        await Task.Yield();
        return GetAppId();
    }

    public virtual string GetAppVersionKey()
    {
        return Config.RequestAppVersionKey;
    }

    public virtual string GetAppVersion()
    {
        return GetParameter(Config.RequestAppVersionKey);
    }

    public virtual async Task<string> GetAppVersionAsync()
    {
        await Task.Yield();
        return GetAppVersion();
    }

    public virtual async Task<bool> IsEqualsAsync(string inputDeviceId)
    {
        await Task.Yield();
        return GetDeviceId().Equals(inputDeviceId);
    }




}
