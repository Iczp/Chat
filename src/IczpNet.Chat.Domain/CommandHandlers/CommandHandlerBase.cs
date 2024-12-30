using IczpNet.Chat.SessionUnits;
using IczpNet.Pusher.Commands;
using IczpNet.Pusher.Models;
using IczpNet.Pusher.Pools;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Json;
using Volo.Abp.Uow;

namespace IczpNet.Chat.CommandHandlers;

public abstract class CommandHandlerBase : DomainService, ICommandHandler, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager => LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
    protected IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();
    protected IPoolsManager PoolsManager => LazyServiceProvider.LazyGetRequiredService<IPoolsManager>();
    protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public CommandHandlerBase() { }

    public virtual async Task HanderAsync(ChannelMessagePayload commandPayload)
    {
        await Task.Yield();
    }
}
