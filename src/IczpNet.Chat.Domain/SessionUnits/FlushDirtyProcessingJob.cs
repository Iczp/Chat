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
    IUnitOfWorkManager unitOfWorkManager,
    Volo.Abp.DistributedLocking.IAbpDistributedLock distributedLock)
        : AsyncBackgroundJob<FlushDirtyProcessingJobArgs>, ITransientDependency
{
    public ISessionUnitCacheManager SessionUnitCacheManager { get; set; } = cacheManager;
    public ISessionUnitRepository Repository { get; set; } = repository;
    public IUnitOfWorkManager UnitOfWorkManager { get; set; } = unitOfWorkManager;

    [UnitOfWork]

    public override async Task ExecuteAsync(FlushDirtyProcessingJobArgs args)
    {
        // Serialize persistence readers, not message sends, to prevent an older
        // cache snapshot from committing after a newer persistence batch.
        if (distributedLock is Volo.Abp.DistributedLocking.NullAbpDistributedLock)
            throw new InvalidOperationException("Dirty persistence requires a distributed lock provider.");
        await using var handle = await distributedLock.TryAcquireAsync("Chat:DirtyPersistence", TimeSpan.FromSeconds(10));
        if (handle == null) throw new InvalidOperationException("Dirty persistence is busy; retry this job.");
        if (args.DirtyVersions != null &&
            !await SessionUnitCacheManager.HasPendingDirtyAsync(args)) return;
        Logger.LogInformation("FlushDirtyProcessingJobArgs {args}", args.ToString());

        var sw = Stopwatch.StartNew();

        var affect = await BatchUpdateAsync(args.SessionUnitIds);

        var acknowledged = await SessionUnitCacheManager.AcknowledgeDirtyAsync(args);
        if (args.DirtyVersions == null || acknowledged > 0)
            await SessionUnitCacheManager.UpdateFlushDirtyProgressAsync(args.ProcessingKey,
                args.DirtyVersions == null ? affect : acknowledged, sw.ElapsedMilliseconds);
    }
    public async Task<int> BatchUpdateAsync(List<Guid> sessionUnitIds)
    {
        if (sessionUnitIds.Count == 0)
        {
            return 0;
        }

        var sw = Stopwatch.StartNew();

        var caches = (await SessionUnitCacheManager.GetManyAsync(sessionUnitIds))
            .Select(x => x.Value)
            .ToList();

        if (caches.Count != sessionUnitIds.Count || caches.Any(x => x == null))
            throw new InvalidOperationException("Missing dirty cache state. Retaining processing records for recovery.");

        Logger.LogInformation(
            "GetManyAsync Count={Count}, Cost={Elapsed}ms",
            caches.Count,
            sw.ElapsedMilliseconds);

        if (caches.Count == 0)
        {
            return 0;
        }

        sw.Restart();

        using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

        var affect = await Repository.BatchUpdateAsync(caches);

        Logger.LogInformation(
            "BatchUpdateAsync Affect={Affect}, Count={Count}, Cost={Elapsed}ms",
            affect,
            caches.Count,
            sw.ElapsedMilliseconds);

        await uow.CompleteAsync();

        return affect;
    }
}
