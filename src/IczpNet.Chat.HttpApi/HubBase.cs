using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Devices;
using IczpNet.Chat.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.WebClientInfo;
using Volo.Abp.Clients;

namespace IczpNet.Chat;

public abstract class HubBase<T, TConnPool> : AbpHub<T> where T : class
    where TConnPool : IConnectionPool, new()
{
    protected ICurrentClient CurrentClient => LazyServiceProvider.LazyGetService<ICurrentClient>()!;
    protected IWebClientInfoProvider WebClientInfoProvider => LazyServiceProvider.LazyGetService<IWebClientInfoProvider>()!;
    protected ICurrentHosted CurrentHosted => LazyServiceProvider.LazyGetService<ICurrentHosted>()!;

    protected IClientApp ClientApp => LazyServiceProvider.LazyGetService<IClientApp>()!;

    protected virtual async Task<TConnPool> BuildInfoAsync()
    {
        await Task.Yield();

        var httpContext = Context.GetHttpContext();

        var appId = CurrentUser.GetAppId() ?? httpContext?.Request.Query["appId"];

        var deviceId = CurrentUser.GetDeviceId() ?? httpContext?.Request.Query["deviceId"];

        var pushClientId = httpContext?.Request.Query["pushClientId"];

        var deviceType = CurrentUser.GetDeviceType() ?? httpContext?.Request.Query["deviceType"];

        var queryId = httpContext?.Request.Query["id"];

        Logger.LogWarning($"DeviceId:{deviceId}");

        var connectedEto = new TConnPool()
        {
            ConnectionId = Context.ConnectionId,
            PushClientId = pushClientId,
            AppId = appId,
            AppName = await ClientApp.GetAppNameAsync(appId),
            QueryId = queryId,
            ClientId = CurrentClient.Id,
            ClientName = await ClientApp.GetClientNameAsync(CurrentClient.Id),
            Host = CurrentHosted.Name,
            IpAddress = WebClientInfoProvider.ClientIpAddress,
            UserId = CurrentUser.Id,
            UserName = CurrentUser.UserName,
            DeviceId = deviceId,
            DeviceType = deviceType,
            BrowserInfo = WebClientInfoProvider.BrowserInfo,
            DeviceInfo = WebClientInfoProvider.DeviceInfo,
            CreationTime = Clock.Now,
        };

        return connectedEto;

    }
}
