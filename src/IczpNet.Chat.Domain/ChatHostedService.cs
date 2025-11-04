//using IczpNet.Chat.ConnectionPools;
//using IczpNet.Chat.Hosting;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System.Threading;
//using System.Threading.Tasks;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Domain.Services;

//namespace IczpNet.Chat;

//public class ChatHostedService(IConnectionPoolManager connectionPoolManager,
//    ICurrentHosted currentHosted) : DomainService, IHostedService, ISingletonDependency
//{
//    public IConnectionPoolManager ConnectionPoolManager { get; } = connectionPoolManager;
//    public ICurrentHosted CurrentHosted { get; } = currentHosted;

//    public async Task StartAsync(CancellationToken cancellationToken)
//    {
//        Logger.LogWarning($"App Start,HostName:{CurrentHosted.Name}");
//        await ConnectionPoolManager.ClearAllAsync(CurrentHosted.Name, "App Start", cancellationToken);
//        await Task.CompletedTask;
//    }

//    public async Task StopAsync(CancellationToken cancellationToken)
//    {
//        Logger.LogWarning($"App Stop,HostName:{CurrentHosted.Name}");
//        await ConnectionPoolManager.ClearAllAsync(CurrentHosted.Name, "App Stop", cancellationToken);
//        await Task.CompletedTask;
//    }
//}
