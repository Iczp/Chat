using System.Threading.Tasks;
using IczpNet.OpenIddict.Tokens;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace IczpNet.Chat.ChatHubs;

public sealed class TokenRevocationEventHandler(ICallerContextManager callers)
    : IDistributedEventHandler<TokenRevokedEto>, ITransientDependency
{
    public Task HandleEventAsync(TokenRevokedEto eventData) => callers.AbortRevokedTokensAsync(eventData);
}
