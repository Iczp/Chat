using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat;

[ExposeServices(typeof(IChatDeviceContext))]
public class DefaultChatDeviceContext : IChatDeviceContext, ITransientDependency
{
    public Task<string> GetDeviceIdAsync() => Task.FromResult<string>(null);
}
