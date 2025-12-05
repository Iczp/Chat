using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat;

public class ChatHostedService(
    IConnectionPoolManager connectionPoolManager,
    IConnectionCacheManager connectionCacheManager,
    ILogger<ChatHostedService> logger,
    ICurrentHosted currentHosted) : IHostedService, ISingletonDependency
{
    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
    public IConnectionCacheManager ConnectionCacheManager { get; } = connectionCacheManager;
    public ILogger<ChatHostedService> Logger { get; } = logger;
    public ICurrentHosted CurrentHosted { get; } = currentHosted;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"[App Start],HostName:{CurrentHosted.Name}");
        //await ConnectionPoolManager.ClearAllAsync(CurrentHosted.Name, "App Start", cancellationToken);
        await ConnectionCacheManager.StartAsync(cancellationToken);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogWarning($"[App Stop],HostName:{CurrentHosted.Name}");
        //await ConnectionPoolManager.ClearAllAsync(CurrentHosted.Name, "App Stop", cancellationToken);
        await ConnectionCacheManager.StopAsync(cancellationToken);
        await Task.CompletedTask;
    }
}
