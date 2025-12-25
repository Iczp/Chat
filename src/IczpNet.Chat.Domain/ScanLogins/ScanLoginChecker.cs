using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ScanLogins;

public class ScanLoginChecker(
    IHttpContextAccessor httpContextAccessor,
    IOptions<ScanLoginOptions> options) : DomainService, IScanLoginChecker
{
    public IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;
    public IOptions<ScanLoginOptions> Options { get; } = options;

    protected ScanLoginOptions Config => Options.Value;

    protected List<string> AllowedDeviceTypes => Config.AllowedDeviceTypes;

    protected bool IsRequiredDeviceType => Config.IsRequiredDeviceType;

    public virtual Task CheckAsync(string scanText)
    {
        HttpContextAccessor.HttpContext.Request.Headers.TryGetValue("App-DeviceType", out var deviceType);
        // 验证 clientId
        //...

        Logger.LogInformation($"deviceType={deviceType}");

        // 验证 deviceType  如： 只能 phone/pad/tablet
        //...

        return Task.CompletedTask;
    }


}
