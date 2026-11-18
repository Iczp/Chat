using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
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
    IBlobContainerFactory blobContainerFactory) : DomainService, ITempBlobCleanupService
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

        if (expiredBlobs.Count == 0)
        {
            return 0;
        }

        var deleted = 0;
        foreach (var blob in expiredBlobs)
        {
            try
            {
                var container = blobContainerFactory.Create(blob.Container);
                await container.DeleteAsync(blob.Name, cancellationToken);
                await blobRepository.DeleteAsync(blob, cancellationToken: cancellationToken);
                deleted++;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to cleanup expired temp blob {BlobId} ({Container}/{Name})", blob.Id, blob.Container, blob.Name);
            }
        }

        return deleted;
    }
}
