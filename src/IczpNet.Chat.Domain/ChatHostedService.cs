using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using IczpNet.Chat.MessageReports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat;

public class ChatHostedService(
    IMessageReportManager messageReportManager,
    IConnectionPoolManager connectionPoolManager,
    IConnectionCacheManager connectionCacheManager,
    ILogger<ChatHostedService> logger,
    ICurrentHosted currentHosted) : IHostedService, ISingletonDependency
{
    public IMessageReportManager MessageReportManager { get; } = messageReportManager;
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
    public IConnectionCacheManager ConnectionCacheManager { get; } = connectionCacheManager;
    public ILogger<ChatHostedService> Logger { get; } = logger;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"[App Start],HostName:{CurrentHosted.Name}");

        //await ConnectionPoolManager.ClearAllAsync(CurrentHosted.Name, "App Start", cancellationToken);

        await MessageReportManager.InitializationAsync();

        await ConnectionCacheManager.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"[App Stop],HostName:{CurrentHosted.Name}");

        //await ConnectionPoolManager.ClearAllAsync(CurrentHosted.Name, "App Stop", cancellationToken);

        await ConnectionCacheManager.StopAsync(cancellationToken);

    }
}
