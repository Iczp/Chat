using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

public interface IClientApp
{
    Task<string> GetClientNameAsync(string clientId);

    Task<string> GetAppNameAsync(string appId, HttpContext httpContext);
}
