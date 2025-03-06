using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat;

public class ChatHostedService : DomainService, IHostedService, ISingletonDependency
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"App Start,HostName:{Dns.GetHostName()}");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"App Stop,HostName:{Dns.GetHostName()}");
        await Task.CompletedTask;
    }
}
