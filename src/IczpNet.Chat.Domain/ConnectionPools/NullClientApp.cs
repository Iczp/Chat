using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.ConnectionPools;

public class NullClientApp : IClientApp, ITransientDependency
{
    public Task<string> GetAppNameAsync(string appId)
    {
        return Task.FromResult<string>(null);
    }

    public Task<string> GetClientNameAsync(string clientId)
    {
        return Task.FromResult<string>(null);
    }
}
