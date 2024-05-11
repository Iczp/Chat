using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.IpAddresses;

[Dependency(TryRegister = true)]
public class NullIpAddressResolver : DomainService, IIpAddressResolver
{
    public virtual async Task<LocationInfo> ResolveAsync(string ipAddress)
    {
        Logger.LogWarning($"IpAddressResolver was not implemented! Using {nameof(NullIpAddressResolver)}:");

        Logger.LogWarning($"ipAddress : {ipAddress}" );

        await Task.Yield();

        return null;
    }
}
