using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.ConnectionPools;

public class NullClientApp : IClientApp, ITransientDependency
{
    public Task<string> GetAppNameAsync(string appId, HttpContext httpContext)
    {
        return Task.FromResult<string>(httpContext?.Request.Query["appName"]);
    }

    public Task<string> GetClientNameAsync(string clientId)
    {
        return Task.FromResult<string>(null);
    }
}
