using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat;

[ExposeServices(typeof(IDeviceContext))]
public class DefaultDeviceContext : IDeviceContext, ITransientDependency
{
    public Task<string> GetDeviceIdAsync() => Task.FromResult<string>(null);
}
