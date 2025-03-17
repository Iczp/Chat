using Microsoft.Extensions.Configuration;
using System.Net;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Hosting;

public class CurrentHosted(IConfiguration configuration) : DomainService, ICurrentHosted, ISingletonDependency
{
    protected string GetHostName() => Configuration["App:HostName"] ?? Dns.GetHostName();

    public string Name => GetHostName();

    public IPAddress[] IPAddress => Dns.GetHostAddresses(Name);

    public IConfiguration Configuration { get; } = configuration;
}
