using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

public class FlushSessionUnitJob(
    ISessionUnitCacheManager cacheManager,
    ISessionUnitRepository repository,
    IUnitOfWorkManager unitOfWorkManager)
        : AsyncBackgroundJob<FlushSessionUnitJobArgs>, ITransientDependency
{
    public ISessionUnitCacheManager SessionUnitCacheManager { get; set; } = cacheManager;
    public ISessionUnitRepository Repository { get; set; } = repository;
    public IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(FlushSessionUnitJobArgs args)
    {
        if (args.SessionUnitIds.Count == 0)
        {
            return;
        }

        var watch = Stopwatch.StartNew();

        var caches = (await SessionUnitCacheManager.GetManyAsync(args.SessionUnitIds))
            .Select(x => x.Value)
            .ToList();

        Logger.LogInformation(
            "GetManyAsync Count={Count}, Cost={Elapsed}ms",
            caches.Count,
            watch.ElapsedMilliseconds);

        if (caches.Count == 0)
        {
            return;
        }

        watch.Restart();

        using var uow = UnitOfWorkManager.Begin();

        var affect = await Repository.BatchUpdateAsync(caches);

        Logger.LogInformation(
            "BatchUpdateAsync Affect={Affect}, Count={Count}, Cost={Elapsed}ms",
            affect,
            caches.Count,
            watch.ElapsedMilliseconds);

        await uow.CompleteAsync();
    }
}