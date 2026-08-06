using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

public class FlushDirtyProcessingJob(
    ISessionUnitCacheManager cacheManager,
    ISessionUnitRepository repository,
    IUnitOfWorkManager unitOfWorkManager)
        : AsyncBackgroundJob<FlushDirtyProcessingJobArgs>, ITransientDependency
{
    public ISessionUnitCacheManager SessionUnitCacheManager { get; set; } = cacheManager;
    public ISessionUnitRepository Repository { get; set; } = repository;
    public IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;

    [UnitOfWork]

    public override async Task ExecuteAsync(FlushDirtyProcessingJobArgs args)
    {
        Logger.LogInformation("FlushDirtyProcessingJobArgs {args}", args.ToString());

        var affect = await BatchUpdateAsync(args.SessionUnitIds);

        await SessionUnitCacheManager.UpdateFlushDirtyProgressAsync(args.ProcessingKey, affect);
    }
    public async Task<int> BatchUpdateAsync(List<Guid> sessionUnitIds)
    {
        if (sessionUnitIds.Count == 0)
        {
            return 0;
        }

        var watch = Stopwatch.StartNew();

        var caches = (await SessionUnitCacheManager.GetManyAsync(sessionUnitIds))
            .Select(x => x.Value)
            .ToList();

        Logger.LogInformation(
            "GetManyAsync Count={Count}, Cost={Elapsed}ms",
            caches.Count,
            watch.ElapsedMilliseconds);

        if (caches.Count == 0)
        {
            return 0;
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

        return affect;
    }
}