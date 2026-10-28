using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.ChatHubs;

public interface IChatDeviceSessionValidator
{
    Task<bool> IsValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

[ExposeServices(typeof(IChatDeviceSessionValidator))]
public class DefaultChatDeviceSessionValidator : IChatDeviceSessionValidator, ITransientDependency
{
    public Task<bool> IsValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default) => Task.FromResult(true);
}
