using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

/// <summary>Bounded dirty dispatch and recovery, registered by worker hosts only.</summary>
public class SessionUnitFlushWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IOptions<SessionUnitOptions> _options;

    public SessionUnitFlushWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory,
        IOptions<SessionUnitOptions> options) : base(timer, serviceScopeFactory)
    {
        _options = options;
        Timer.Period = Math.Max(5, options.Value.FlushDirtyPeriodSeconds) * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        if (!_options.Value.AutoFlushDirtyEnabled) return;
        var uowManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        await workerContext.ServiceProvider.GetRequiredService<ISessionUnitManager>().FlushDirtyAsync();
        await uow.CompleteAsync();
    }
}
