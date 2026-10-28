using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.ConnectionPools;

public interface IChatDeviceLogout
{
    Task LogoutConnectionsAsync(List<string> connectionIds, string reason);
}

[ExposeServices(typeof(IChatDeviceLogout))]
public class DefaultChatDeviceLogout : IChatDeviceLogout, ITransientDependency
{
    public Task LogoutConnectionsAsync(List<string> connectionIds, string reason) => Task.CompletedTask;
}
