using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnitCounters;

public class SessionUnitCounterIncrementJob : AsyncBackgroundJob<SessionUnitCounterArgs>, ITransientDependency
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected ISessionUnitCounterManager SessionUnitCounterManager { get; }

    public SessionUnitCounterIncrementJob(
        IUnitOfWorkManager unitOfWorkManager,
        ISessionUnitCounterManager sessionUnitCounterManager)
    {
        UnitOfWorkManager = unitOfWorkManager;
        SessionUnitCounterManager = sessionUnitCounterManager;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(SessionUnitCounterArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var totalCount = await SessionUnitCounterManager.IncremenetAsync(args);

        Logger.LogInformation($"SessionUnitCounterIncrementJob Completed totalCount:{totalCount}.");
    }
}
