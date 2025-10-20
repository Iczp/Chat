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

    public virtual string GetHeader(string key)
    {
        var httpContext = GetHttpContext();
        if (httpContext != null)
        {
            var headers = httpContext.Request.Headers;
            return headers[key];
        }
        return null;
    }

    public virtual string GetDeviceId()
    {
        return GetHeader(Config.RequestDeviceIdKey);
    }

    public virtual string GetDeviceType()
    {
        return GetHeader(Config.RequestDeviceTypeKey);
    }

    public virtual async Task<string> GetDeviceIdAsync()
    {
        await Task.Yield();
        return GetDeviceId();
    }

    public virtual async Task<string> GetDeviceTypeAsync()
    {
        await Task.Yield();
        return GetDeviceType();
    }

    public virtual async Task<bool> IsEqualsAsync(string inputDeviceId)
    {
        await Task.Yield();
        return GetDeviceId().Equals(inputDeviceId);
    }

    public virtual string GetDeviceIdKey()
    {
        return Config.RequestDeviceIdKey;
    }

    public virtual string GetDeviceTypeKey()
    {
        return Config.RequestDeviceTypeKey;
    }

}
