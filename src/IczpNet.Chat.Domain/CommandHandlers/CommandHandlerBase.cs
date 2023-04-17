using IczpNet.Pusher.Models;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using IczpNet.Chat.SessionSections.Sessions;
using Volo.Abp.Uow;
using Volo.Abp.Domain.Services;
using IczpNet.Pusher.Connections;
using IczpNet.Pusher.Commands;

namespace IczpNet.Chat.CommandHandlers;

public abstract class CommandHandlerBase : DomainService, ICommandHandler, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager => LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
    protected IJsonSerializer JsonSerializer => LazyServiceProvider.LazyGetRequiredService<IJsonSerializer>();
    protected IConnectionManager ConnectionManager => LazyServiceProvider.LazyGetRequiredService<IConnectionManager>();
    protected ISessionUnitManager SessionUnitManager => LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
    public CommandHandlerBase() { }

    public virtual async Task HanderAsync(ChannelMessagePayload commandPayload)
    {
        await Task.CompletedTask;
    }
}
