using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.IpAddresses;

public interface IIpAddressResolver : ITransientDependency
{
    Task<LocationInfo> ResolveAsync(string ipAddress);
}
