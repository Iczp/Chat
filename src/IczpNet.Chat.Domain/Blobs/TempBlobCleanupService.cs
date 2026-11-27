using IczpNet.Chat.MessageSections.Messages;
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

        // Phase 1: In a committed transaction, atomically claim & soft-delete expired unreferenced Temp Blobs
        // using conditional ExecuteUpdate so concurrent Temp->Bound transitions cannot be deleted on stale reads.
        // Physical I/O NEVER runs inside this transaction.
        using (var uow1 = uowManager.Begin(requiresNew: true, isTransactional: true))
        {
            var messageRepository = LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
            var claimed = await messageRepository.ClaimExpiredTempBlobsAsync(now, batchSize, cancellationToken);
            foreach (var item in claimed)
            {
                expiredIds.Add(item);
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

        // Phase 3: Reconcile touched objects + bounded scan of Deleting, stale Pending (>2h),
        // Ready(RefCount<=0), and stale Ready orphans (including positive RefCount) with zero active logical Blob rows.
        var candidateObjectIds = new HashSet<Guid>(touchedObjectIds);
        using (var scanUow = uowManager.Begin(requiresNew: true, isTransactional: false))
        {
            var staleCutoff = now.AddHours(-2);
            var blobQuery = await blobRepository.GetQueryableAsync();
            var orphanIds = (await blobObjectRepository.GetQueryableAsync())
                .Where(x =>
                    x.Status == BlobObjectStatus.Deleting ||
                    (x.Status == BlobObjectStatus.Pending && x.CreationTime < staleCutoff && x.RefCount <= 0) ||
                    (x.Status == BlobObjectStatus.Ready && x.RefCount <= 0) ||
                    (x.Status == BlobObjectStatus.Ready &&
                     x.CreationTime < staleCutoff &&
                     (!x.LastVerifiedTime.HasValue || x.LastVerifiedTime.Value < staleCutoff) &&
                     !blobQuery.Any(b => b.StorageObjectId == x.Id)))
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

                var reuseManager = LazyServiceProvider?.LazyGetService<IAttachmentProcessingReuseManager>();
                if (reuseManager != null)
                {
                    await reuseManager.InvalidateByStorageObjectIdAsync(objectId, cancellationToken);
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
