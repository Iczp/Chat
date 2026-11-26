using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public interface ITempBlobCleanupService
{
    Task<int> CleanupExpiredTempBlobsAsync(int batchSize = 200, CancellationToken cancellationToken = default);
}

public class TempBlobCleanupService(
    IRepository<Blob, Guid> blobRepository,
    IRepository<BlobObject, Guid> blobObjectRepository,
    IBlobContainerFactory blobContainerFactory,
    IAbpDistributedLock distributedLock) : DomainService, ITempBlobCleanupService
{
    public virtual async Task<int> CleanupExpiredTempBlobsAsync(int batchSize = 200, CancellationToken cancellationToken = default)
    {
        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        var now = Clock.Now;
        var expiredIds = new List<(Guid Id, string Container, string Name, Guid? StorageObjectId)>();
        var touchedObjectIds = new HashSet<Guid>();
        var legacyPhysicalToDelete = new List<(string Container, string Name)>();

        // Phase 1: In a committed transaction, claim & delete only expired unreferenced Temp Blobs.
        // Physical I/O NEVER runs inside this transaction, so a rolled-back send/cleanup cannot orphan refs.
        using (var uow1 = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            var candidates = (await blobRepository.GetQueryableAsync())
                .Where(x => x.Status == BlobStatus.Temp && x.ExpireTime.HasValue && x.ExpireTime.Value < now && x.RefCount <= 0)
                .OrderBy(x => x.ExpireTime)
                .Take(batchSize)
                .Select(x => new { x.Id, x.Container, x.Name, x.StorageObjectId })
                .ToList();

            foreach (var item in candidates)
            {
                var entity = await blobRepository.FindAsync(item.Id, cancellationToken: cancellationToken);
                if (entity == null ||
                    entity.Status != BlobStatus.Temp ||
                    entity.RefCount > 0 ||
                    !entity.ExpireTime.HasValue ||
                    entity.ExpireTime.Value >= now)
                {
                    continue;
                }

                await blobRepository.DeleteAsync(entity, autoSave: true, cancellationToken: cancellationToken);
                expiredIds.Add((item.Id, item.Container, item.Name, item.StorageObjectId));
                if (item.StorageObjectId.HasValue)
                {
                    touchedObjectIds.Add(item.StorageObjectId.Value);
                }
                else
                {
                    legacyPhysicalToDelete.Add((item.Container, item.Name));
                }
            }

            await uow1.CompleteAsync(cancellationToken);
        }

        // Phase 2 (after logical Blob deletion committed): delete legacy physical files outside transaction.
        foreach (var (container, name) in legacyPhysicalToDelete)
        {
            try
            {
                await blobContainerFactory.Create(container).DeleteAsync(name, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to delete legacy expired temp blob file ({Container}/{Name})", container, name);
            }
        }

        // Phase 3: Reconcile touched objects + bounded scan of Deleting, stale Pending (>2h), and Ready(RefCount<=0) orphans.
        var candidateObjectIds = new HashSet<Guid>(touchedObjectIds);
        using (var scanUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var stalePendingCutoff = now.AddHours(-2);
            var orphanIds = (await blobObjectRepository.GetQueryableAsync())
                .Where(x =>
                    x.Status == BlobObjectStatus.Deleting ||
                    (x.Status == BlobObjectStatus.Pending && x.CreationTime < stalePendingCutoff && x.RefCount <= 0) ||
                    (x.Status == BlobObjectStatus.Ready && x.RefCount <= 0))
                .OrderBy(x => x.CreationTime)
                .Take(batchSize)
                .Select(x => x.Id)
                .ToList();

            foreach (var id in orphanIds)
            {
                candidateObjectIds.Add(id);
            }

            await scanUow.CompleteAsync(cancellationToken);
        }

        foreach (var objectId in candidateObjectIds)
        {
            await TryReconcileOrDeleteStorageObjectAsync(uowManager, objectId, cancellationToken);
        }

        return expiredIds.Count;
    }

    private async Task TryReconcileOrDeleteStorageObjectAsync(
        IUnitOfWorkManager uowManager,
        Guid objectId,
        CancellationToken cancellationToken)
    {
        try
        {
            BlobObject snapshot;
            using (var readUow = uowManager.Begin(requiresNew: true, isTransactional: false))
            {
                snapshot = await blobObjectRepository.FindAsync(objectId, cancellationToken: cancellationToken);
                await readUow.CompleteAsync(cancellationToken);
            }

            if (snapshot == null)
            {
                return;
            }

            var lockName = $"Chat:BlobObject:{snapshot.StorageScope}:{snapshot.Container}:{snapshot.Sha256}:{snapshot.FileSize}";
            await using var handle = await distributedLock.TryAcquireAsync(lockName, TimeSpan.FromSeconds(10), cancellationToken);
            if (handle == null)
            {
                return;
            }

            string physicalContainerToDelete = null;
            string physicalNameToDelete = null;

            // Step A: Commit Status = Deleting (or reconcile RefCount) inside a transactional UOW first.
            using (var markUow = uowManager.Begin(requiresNew: true, isTransactional: true))
            {
                var obj = await blobObjectRepository.FindAsync(objectId, cancellationToken: cancellationToken);
                if (obj == null)
                {
                    await markUow.CompleteAsync(cancellationToken);
                    return;
                }

                var remainingRefs = (await blobRepository.GetQueryableAsync())
                    .Count(x => x.StorageObjectId == objectId);

                if (remainingRefs > 0)
                {
                    if (obj.RefCount != remainingRefs)
                    {
                        obj.RefCount = remainingRefs;
                        await blobObjectRepository.UpdateAsync(obj, autoSave: true, cancellationToken: cancellationToken);
                    }
                    await markUow.CompleteAsync(cancellationToken);
                    return;
                }

                if (obj.Status != BlobObjectStatus.Deleting)
                {
                    obj.Status = BlobObjectStatus.Deleting;
                    obj.RefCount = 0;
                    obj.DeletingSince = Clock.Now;
                    await blobObjectRepository.UpdateAsync(obj, autoSave: true, cancellationToken: cancellationToken);
                }

                physicalContainerToDelete = obj.Container;
                physicalNameToDelete = obj.StorageName;
                await markUow.CompleteAsync(cancellationToken);
            }

            // Step B (after Deleting status is durably committed): delete physical bytes by immutable versioned StorageName.
            if (!string.IsNullOrWhiteSpace(physicalNameToDelete))
            {
                await blobContainerFactory.Create(physicalContainerToDelete).DeleteAsync(physicalNameToDelete, cancellationToken);
            }

            // Step C: Soft/remove the Deleting BlobObject row (only when still Deleting).
            using (var finalizeUow = uowManager.Begin(requiresNew: true, isTransactional: true))
            {
                var obj = await blobObjectRepository.FindAsync(objectId, cancellationToken: cancellationToken);
                if (obj != null && obj.Status == BlobObjectStatus.Deleting)
                {
                    await blobObjectRepository.DeleteAsync(obj, autoSave: true, cancellationToken: cancellationToken);
                }
                await finalizeUow.CompleteAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to reconcile/delete storage object {StorageObjectId}", objectId);
        }
    }
}
