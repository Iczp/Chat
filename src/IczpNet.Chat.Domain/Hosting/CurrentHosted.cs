using System.Net;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Hosting;

public class CurrentHosted : DomainService, ICurrentHosted, ISingletonDependency
{
    public string Name => Dns.GetHostName();

    public IPAddress[] IPAddress => Dns.GetHostAddresses(Name);
}
