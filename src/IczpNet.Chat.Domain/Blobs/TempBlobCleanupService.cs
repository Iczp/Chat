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
    [UnitOfWork]
    public virtual async Task<int> CleanupExpiredTempBlobsAsync(int batchSize = 200, CancellationToken cancellationToken = default)
    {
        var now = Clock.Now;
        var expiredBlobs = (await blobRepository.GetQueryableAsync())
            .Where(x => x.Status == BlobStatus.Temp && x.ExpireTime.HasValue && x.ExpireTime.Value < now && x.RefCount <= 0)
            .OrderBy(x => x.ExpireTime)
            .Take(batchSize)
            .ToList();

        var deleted = 0;
        var touchedObjectIds = new HashSet<Guid>();

        foreach (var blob in expiredBlobs)
        {
            try
            {
                if (!blob.StorageObjectId.HasValue)
                {
                    var container = blobContainerFactory.Create(blob.Container);
                    await container.DeleteAsync(blob.Name, cancellationToken);
                    await blobRepository.DeleteAsync(blob, autoSave: true, cancellationToken: cancellationToken);
                    deleted++;
                    continue;
                }

                touchedObjectIds.Add(blob.StorageObjectId.Value);
                await blobRepository.DeleteAsync(blob, autoSave: true, cancellationToken: cancellationToken);
                deleted++;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to cleanup expired temp blob {BlobId} ({Container}/{Name})", blob.Id, blob.Container, blob.Name);
            }
        }

        // Reconcile refcounts and physically delete dereferenced BlobObjects safely under lock.
        foreach (var objectId in touchedObjectIds)
        {
            await TryReconcileOrDeleteStorageObjectAsync(objectId, cancellationToken);
        }

        // Recover stale Pending (>2h) or orphaned Deleting storage objects in small bounded batches.
        var stalePendingCutoff = now.AddHours(-2);
        var orphanObjectIds = (await blobObjectRepository.GetQueryableAsync())
            .Where(x =>
                (x.Status == BlobObjectStatus.Deleting) ||
                (x.Status == BlobObjectStatus.Pending && x.CreationTime < stalePendingCutoff && x.RefCount <= 0))
            .OrderBy(x => x.CreationTime)
            .Take(batchSize)
            .Select(x => x.Id)
            .ToList();

        foreach (var objectId in orphanObjectIds)
        {
            if (touchedObjectIds.Contains(objectId))
            {
                continue;
            }
            await TryReconcileOrDeleteStorageObjectAsync(objectId, cancellationToken);
        }

        return deleted;
    }

    private async Task TryReconcileOrDeleteStorageObjectAsync(Guid objectId, CancellationToken cancellationToken)
    {
        try
        {
            var obj = await blobObjectRepository.FindAsync(objectId, cancellationToken: cancellationToken);
            if (obj == null)
            {
                return;
            }

            var lockName = $"Chat:BlobObject:{obj.StorageScope}:{obj.Container}:{obj.Sha256}:{obj.FileSize}";
            await using var handle = await distributedLock.TryAcquireAsync(lockName, TimeSpan.FromSeconds(10), cancellationToken);
            if (handle == null)
            {
                return;
            }

            obj = await blobObjectRepository.FindAsync(objectId, cancellationToken: cancellationToken);
            if (obj == null)
            {
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
                return;
            }

            if (obj.Status != BlobObjectStatus.Deleting)
            {
                obj.Status = BlobObjectStatus.Deleting;
                obj.RefCount = 0;
                obj.DeletingSince = Clock.Now;
                await blobObjectRepository.UpdateAsync(obj, autoSave: true, cancellationToken: cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(obj.StorageName))
            {
                await blobContainerFactory.Create(obj.Container).DeleteAsync(obj.StorageName, cancellationToken);
            }

            await blobObjectRepository.DeleteAsync(obj, autoSave: true, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to reconcile/delete storage object {StorageObjectId}", objectId);
        }
    }
}
