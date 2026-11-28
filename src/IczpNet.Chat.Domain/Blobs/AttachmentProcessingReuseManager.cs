using IczpNet.Chat.Enums;
using IczpNet.Chat.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public interface IAttachmentProcessingReuseManager
{
    bool IsEnabled();

    string ComputeSha256(byte[] bytes);

    string ComputeRecipeHash(string recipeSpec);

    string BuildImagePrimaryRecipeHash(MessageOptions.ImageOptions imageSetting);

    string BuildImageThumbnailRecipeHash(MessageOptions.ImageOptions imageSetting);

    string BuildVideoSnapshotRecipeHash(MessageOptions.VideoOptions videoSetting);

    string BuildVideoSnapshotThumbnailRecipeHash(MessageOptions.VideoOptions videoSetting);

    string BuildVideoClientSnapshotThumbnailRecipeHash(MessageOptions.VideoOptions videoSetting);

    string BuildVideoGifRecipeHash(MessageOptions.VideoOptions videoSetting);

    string BuildSoundProbeRecipeHash();

    Task<AttachmentProcessingResult> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AttachmentProcessingResult> GetOrCreatePendingTaskAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        CancellationToken cancellationToken = default);

    Task<AttachmentProcessingResult> FindReadyResultAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        CancellationToken cancellationToken = default);

    Task<AttachmentProcessingResult> RecordReadyResultAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        Guid? outputStorageObjectId,
        string outputMimeType,
        string outputSuffix,
        long? outputFileSize,
        int? width,
        int? height,
        int? durationMs,
        string metadataJson = null,
        CancellationToken cancellationToken = default);

    Task<AttachmentProcessingResult> TryClaimTaskAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        string leaseOwner,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task CompleteTaskAsync(
        Guid resultId,
        long expectedVersion,
        Guid? outputStorageObjectId,
        string outputMimeType,
        string outputSuffix,
        long? outputFileSize,
        int? width,
        int? height,
        int? durationMs,
        string metadataJson = null,
        CancellationToken cancellationToken = default);

    Task FailTaskAsync(
        Guid resultId,
        long expectedVersion,
        string error,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default);

    Task RegisterWaiterAsync(
        Guid processingResultId,
        long messageId,
        Guid primaryBlobId,
        BlobRoles outputRole,
        CancellationToken cancellationToken = default);

    Task<List<AttachmentProcessingWaiter>> GetPendingWaitersAsync(
        Guid processingResultId,
        int maxCount = 200,
        CancellationToken cancellationToken = default);

    Task MarkWaiterStatusAsync(
        Guid waiterId,
        AttachmentProcessingWaiterStatus status,
        CancellationToken cancellationToken = default);

    Task<List<Guid>> MarkWaiterStatusByMessageAsync(
        long messageId,
        BlobRoles outputRole,
        AttachmentProcessingWaiterStatus status,
        CancellationToken cancellationToken = default);

    Task<int> InvalidateByStorageObjectIdAsync(
        Guid storageObjectId,
        CancellationToken cancellationToken = default);

    Task<int> RecoverPendingGifWaitersAsync(
        int maxResults = 20,
        CancellationToken cancellationToken = default);
}

public class AttachmentProcessingReuseManager(
    IRepository<AttachmentProcessingResult, Guid> resultRepository,
    IRepository<AttachmentProcessingWaiter, Guid> waiterRepository,
    IRepository<BlobObject, Guid> blobObjectRepository,
    IAbpDistributedLock distributedLock) : DomainService, IAttachmentProcessingReuseManager
{
    public virtual bool IsEnabled()
    {
        var opts = LazyServiceProvider?.LazyGetService<IOptions<AttachmentUploadOptions>>()?.Value;
        return opts?.EnableProcessingResultReuse ?? true;
    }

    public virtual string ComputeSha256(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return string.Empty;
        }

        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public virtual string ComputeRecipeHash(string recipeSpec)
    {
        var bytes = Encoding.UTF8.GetBytes(recipeSpec ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public virtual string BuildImagePrimaryRecipeHash(MessageOptions.ImageOptions imageSetting)
        => ComputeRecipeHash($"v1|img-primary|{imageSetting.BigSize}");

    public virtual string BuildImageThumbnailRecipeHash(MessageOptions.ImageOptions imageSetting)
        => ComputeRecipeHash($"v1|img-thumb|{imageSetting.ThumbnailSize}|{BuildImagePrimaryRecipeHash(imageSetting)}");

    public virtual string BuildVideoSnapshotRecipeHash(MessageOptions.VideoOptions videoSetting)
        => ComputeRecipeHash(
            $"v1|video-snapshot|{videoSetting.ImgCaptureSeconds}|{videoSetting.ImgSnapshotSize.Width}x{videoSetting.ImgSnapshotSize.Height}");

    public virtual string BuildVideoSnapshotThumbnailRecipeHash(MessageOptions.VideoOptions videoSetting)
        => ComputeRecipeHash(
            $"v1|video-snap-thumb|{videoSetting.SnapshotThumbnailSize}|{BuildVideoSnapshotRecipeHash(videoSetting)}");

    public virtual string BuildVideoClientSnapshotThumbnailRecipeHash(MessageOptions.VideoOptions videoSetting)
        => ComputeRecipeHash($"v1|video-client-snap-thumb|{videoSetting.SnapshotThumbnailSize}");

    public virtual string BuildVideoGifRecipeHash(MessageOptions.VideoOptions videoSetting)
        => ComputeRecipeHash(
            $"v1|video-gif|{videoSetting.ImgCaptureSeconds}|{videoSetting.GifSeconds}|{videoSetting.GifSnapshotSize.Width}x{videoSetting.GifSnapshotSize.Height}");

    public virtual string BuildSoundProbeRecipeHash()
        => ComputeRecipeHash("v1|sound-probe");

    public virtual Task<AttachmentProcessingResult> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => resultRepository.FindAsync(id, cancellationToken: cancellationToken);

    public virtual async Task<AttachmentProcessingResult> GetOrCreatePendingTaskAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        CancellationToken cancellationToken = default)
    {
        var existing = await resultRepository.FirstOrDefaultAsync(x =>
            x.StorageScope == storageScope &&
            x.Container == container &&
            x.SourceSha256 == sourceSha256 &&
            x.SourceFileSize == sourceFileSize &&
            x.MediaType == mediaType &&
            x.OutputRole == outputRole &&
            x.RecipeHash == recipeHash,
            cancellationToken: cancellationToken);

        if (existing != null)
        {
            return existing;
        }

        var lockKey = $"Chat:ProcClaim:{storageScope}:{container}:{sourceSha256}:{sourceFileSize}:{(int)mediaType}:{(int)outputRole}:{recipeHash}";
        await using var handle = await distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);

        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        try
        {
            using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
            var rechecked = await resultRepository.FirstOrDefaultAsync(x =>
                x.StorageScope == storageScope &&
                x.Container == container &&
                x.SourceSha256 == sourceSha256 &&
                x.SourceFileSize == sourceFileSize &&
                x.MediaType == mediaType &&
                x.OutputRole == outputRole &&
                x.RecipeHash == recipeHash,
                cancellationToken: cancellationToken);

            if (rechecked != null)
            {
                await uow.CompleteAsync(cancellationToken);
                return rechecked;
            }

            var entity = new AttachmentProcessingResult(
                GuidGenerator.Create(),
                storageScope,
                container,
                sourceSha256,
                sourceFileSize,
                mediaType,
                outputRole,
                recipeHash);

            var inserted = await resultRepository.InsertAsync(entity, autoSave: true, cancellationToken: cancellationToken);
            await uow.CompleteAsync(cancellationToken);
            return inserted;
        }
        catch (Exception ex) when (BlobDbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            using var recoverUow = uowManager.Begin(requiresNew: true, isTransactional: false);
            var winner = await resultRepository.FirstOrDefaultAsync(x =>
                x.StorageScope == storageScope &&
                x.Container == container &&
                x.SourceSha256 == sourceSha256 &&
                x.SourceFileSize == sourceFileSize &&
                x.MediaType == mediaType &&
                x.OutputRole == outputRole &&
                x.RecipeHash == recipeHash,
                cancellationToken: cancellationToken);
            await recoverUow.CompleteAsync(cancellationToken);
            if (winner != null)
            {
                return winner;
            }
            throw;
        }
    }

    public virtual async Task<AttachmentProcessingResult> FindReadyResultAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled() || string.IsNullOrWhiteSpace(sourceSha256) || sourceFileSize <= 0)
        {
            return null;
        }

        var result = await resultRepository.FirstOrDefaultAsync(x =>
            x.StorageScope == storageScope &&
            x.Container == container &&
            x.SourceSha256 == sourceSha256 &&
            x.SourceFileSize == sourceFileSize &&
            x.MediaType == mediaType &&
            x.OutputRole == outputRole &&
            x.RecipeHash == recipeHash,
            cancellationToken: cancellationToken);

        if (result == null || result.Status != AttachmentProcessingStatus.Ready)
        {
            return null;
        }

        if (result.OutputStorageObjectId.HasValue)
        {
            var objStatus = (await blobObjectRepository.GetQueryableAsync())
                .Where(x => x.Id == result.OutputStorageObjectId.Value)
                .Select(x => (BlobObjectStatus?)x.Status)
                .FirstOrDefault();

            if (objStatus != BlobObjectStatus.Ready)
            {
                result.Status = AttachmentProcessingStatus.Invalidated;
                result.Version++;
                await resultRepository.UpdateAsync(result, autoSave: true, cancellationToken: cancellationToken);
                return null;
            }
        }

        return result;
    }

    public virtual async Task<AttachmentProcessingResult> RecordReadyResultAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        Guid? outputStorageObjectId,
        string outputMimeType,
        string outputSuffix,
        long? outputFileSize,
        int? width,
        int? height,
        int? durationMs,
        string metadataJson = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled() || string.IsNullOrWhiteSpace(sourceSha256) || sourceFileSize <= 0)
        {
            return null;
        }

        var lockKey = $"Chat:ProcResult:{storageScope}:{container}:{sourceSha256}:{sourceFileSize}:{(int)mediaType}:{(int)outputRole}:{recipeHash}";
        await using var handle = await distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);
        if (handle == null)
        {
            return null;
        }

        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

        var existing = await resultRepository.FirstOrDefaultAsync(x =>
            x.StorageScope == storageScope &&
            x.Container == container &&
            x.SourceSha256 == sourceSha256 &&
            x.SourceFileSize == sourceFileSize &&
            x.MediaType == mediaType &&
            x.OutputRole == outputRole &&
            x.RecipeHash == recipeHash,
            cancellationToken: cancellationToken);

        if (existing == null)
        {
            existing = new AttachmentProcessingResult(
                GuidGenerator.Create(),
                storageScope,
                container,
                sourceSha256,
                sourceFileSize,
                mediaType,
                outputRole,
                recipeHash)
            {
                Status = AttachmentProcessingStatus.Ready,
                OutputStorageObjectId = outputStorageObjectId,
                OutputMimeType = outputMimeType,
                OutputSuffix = outputSuffix,
                OutputFileSize = outputFileSize,
                Width = width,
                Height = height,
                DurationMs = durationMs,
                MetadataJson = metadataJson,
                LastVerifiedTime = Clock.Now,
                Version = 1,
            };
            await resultRepository.InsertAsync(existing, autoSave: true, cancellationToken: cancellationToken);
        }
        else
        {
            existing.Status = AttachmentProcessingStatus.Ready;
            existing.OutputStorageObjectId = outputStorageObjectId;
            existing.OutputMimeType = outputMimeType ?? existing.OutputMimeType;
            existing.OutputSuffix = outputSuffix ?? existing.OutputSuffix;
            existing.OutputFileSize = outputFileSize ?? existing.OutputFileSize;
            existing.Width = width ?? existing.Width;
            existing.Height = height ?? existing.Height;
            existing.DurationMs = durationMs ?? existing.DurationMs;
            existing.MetadataJson = metadataJson ?? existing.MetadataJson;
            existing.LastVerifiedTime = Clock.Now;
            existing.LeaseOwner = null;
            existing.LeaseExpireTime = null;
            existing.LastError = null;
            existing.Version++;
            await resultRepository.UpdateAsync(existing, autoSave: true, cancellationToken: cancellationToken);
        }

        await uow.CompleteAsync(cancellationToken);
        return existing;
    }

    public virtual async Task<AttachmentProcessingResult> TryClaimTaskAsync(
        string storageScope,
        string container,
        string sourceSha256,
        long sourceFileSize,
        AttachmentMediaType mediaType,
        BlobRoles outputRole,
        string recipeHash,
        string leaseOwner,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"Chat:ProcClaim:{storageScope}:{container}:{sourceSha256}:{sourceFileSize}:{(int)mediaType}:{(int)outputRole}:{recipeHash}";
        await using var handle = await distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10), cancellationToken);
        if (handle == null)
        {
            return null;
        }

        var now = Clock.Now;
        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

        var existing = await resultRepository.FirstOrDefaultAsync(x =>
            x.StorageScope == storageScope &&
            x.Container == container &&
            x.SourceSha256 == sourceSha256 &&
            x.SourceFileSize == sourceFileSize &&
            x.MediaType == mediaType &&
            x.OutputRole == outputRole &&
            x.RecipeHash == recipeHash,
            cancellationToken: cancellationToken);

        if (existing == null)
        {
            existing = new AttachmentProcessingResult(
                GuidGenerator.Create(),
                storageScope,
                container,
                sourceSha256,
                sourceFileSize,
                mediaType,
                outputRole,
                recipeHash)
            {
                Status = AttachmentProcessingStatus.Processing,
                LeaseOwner = leaseOwner,
                LeaseExpireTime = now.Add(leaseDuration),
                Version = 1,
            };
            await resultRepository.InsertAsync(existing, autoSave: true, cancellationToken: cancellationToken);
            await uow.CompleteAsync(cancellationToken);
            return existing;
        }

        if (existing.Status == AttachmentProcessingStatus.Ready)
        {
            await uow.CompleteAsync(cancellationToken);
            return existing;
        }

        if (existing.Status == AttachmentProcessingStatus.Processing &&
            existing.LeaseExpireTime.HasValue &&
            existing.LeaseExpireTime.Value > now &&
            !string.Equals(existing.LeaseOwner, leaseOwner, StringComparison.Ordinal))
        {
            // Another worker holds a valid lease
            await uow.CompleteAsync(cancellationToken);
            return null;
        }

        if (existing.Status == AttachmentProcessingStatus.Failed &&
            existing.NextRetryTime.HasValue &&
            existing.NextRetryTime.Value > now)
        {
            await uow.CompleteAsync(cancellationToken);
            return null;
        }

        existing.Status = AttachmentProcessingStatus.Processing;
        existing.LeaseOwner = leaseOwner;
        existing.LeaseExpireTime = now.Add(leaseDuration);
        existing.Version++;
        await resultRepository.UpdateAsync(existing, autoSave: true, cancellationToken: cancellationToken);
        await uow.CompleteAsync(cancellationToken);
        return existing;
    }

    public virtual async Task CompleteTaskAsync(
        Guid resultId,
        long expectedVersion,
        Guid? outputStorageObjectId,
        string outputMimeType,
        string outputSuffix,
        long? outputFileSize,
        int? width,
        int? height,
        int? durationMs,
        string metadataJson = null,
        CancellationToken cancellationToken = default)
    {
        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

        var entity = await resultRepository.GetAsync(resultId, cancellationToken: cancellationToken);
        if (entity.Version != expectedVersion)
        {
            Logger.LogWarning("AttachmentProcessingResult version mismatch on complete: ResultId={ResultId}, Expected={Expected}, Actual={Actual}",
                resultId, expectedVersion, entity.Version);
            return;
        }

        entity.Status = AttachmentProcessingStatus.Ready;
        entity.OutputStorageObjectId = outputStorageObjectId;
        entity.OutputMimeType = outputMimeType;
        entity.OutputSuffix = outputSuffix;
        entity.OutputFileSize = outputFileSize;
        entity.Width = width;
        entity.Height = height;
        entity.DurationMs = durationMs;
        entity.MetadataJson = metadataJson;
        entity.LastVerifiedTime = Clock.Now;
        entity.LeaseOwner = null;
        entity.LeaseExpireTime = null;
        entity.LastError = null;
        entity.Version++;

        await resultRepository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        await uow.CompleteAsync(cancellationToken);
    }

    public virtual async Task FailTaskAsync(
        Guid resultId,
        long expectedVersion,
        string error,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default)
    {
        var uowManager = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>();
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

        var entity = await resultRepository.GetAsync(resultId, cancellationToken: cancellationToken);
        if (entity.Version != expectedVersion)
        {
            return;
        }

        entity.Status = AttachmentProcessingStatus.Failed;
        entity.RetryCount++;
        entity.NextRetryTime = Clock.Now.Add(retryDelay);
        entity.LastError = error?.Length > 1024 ? error[..1024] : error;
        entity.LeaseOwner = null;
        entity.LeaseExpireTime = null;
        entity.Version++;

        await resultRepository.UpdateAsync(entity, autoSave: true, cancellationToken: cancellationToken);
        await uow.CompleteAsync(cancellationToken);
    }

    public virtual async Task RegisterWaiterAsync(
        Guid processingResultId,
        long messageId,
        Guid primaryBlobId,
        BlobRoles outputRole,
        CancellationToken cancellationToken = default)
    {
        var existing = await waiterRepository.FirstOrDefaultAsync(x =>
            x.MessageId == messageId && x.OutputRole == outputRole,
            cancellationToken: cancellationToken);

        if (existing != null)
        {
            return;
        }

        var waiter = new AttachmentProcessingWaiter(
            GuidGenerator.Create(),
            processingResultId,
            messageId,
            primaryBlobId,
            outputRole);

        await waiterRepository.InsertAsync(waiter, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task<List<AttachmentProcessingWaiter>> GetPendingWaitersAsync(
        Guid processingResultId,
        int maxCount = 200,
        CancellationToken cancellationToken = default)
    {
        var query = await waiterRepository.GetQueryableAsync();
        return query
            .Where(x => x.ProcessingResultId == processingResultId && x.Status == AttachmentProcessingWaiterStatus.Pending)
            .OrderBy(x => x.CreationTime)
            .Take(maxCount)
            .ToList();
    }

    public virtual async Task MarkWaiterStatusAsync(
        Guid waiterId,
        AttachmentProcessingWaiterStatus status,
        CancellationToken cancellationToken = default)
    {
        var waiter = await waiterRepository.FirstOrDefaultAsync(x => x.Id == waiterId, cancellationToken: cancellationToken);
        if (waiter == null)
        {
            return;
        }

        waiter.Status = status;
        if (status is AttachmentProcessingWaiterStatus.Completed or AttachmentProcessingWaiterStatus.Skipped)
        {
            waiter.CompletedTime = Clock.Now;
        }

        await waiterRepository.UpdateAsync(waiter, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task<List<Guid>> MarkWaiterStatusByMessageAsync(
        long messageId,
        BlobRoles outputRole,
        AttachmentProcessingWaiterStatus status,
        CancellationToken cancellationToken = default)
    {
        var waiters = await waiterRepository.GetListAsync(
            x => x.MessageId == messageId &&
                 x.OutputRole == outputRole &&
                 x.Status == AttachmentProcessingWaiterStatus.Pending,
            cancellationToken: cancellationToken);
        if (waiters.Count == 0)
        {
            return [];
        }

        var now = Clock.Now;
        var resultIds = new List<Guid>(waiters.Count);
        foreach (var waiter in waiters)
        {
            waiter.Status = status;
            if (status is AttachmentProcessingWaiterStatus.Completed or AttachmentProcessingWaiterStatus.Skipped)
            {
                waiter.CompletedTime = now;
            }
            if (!resultIds.Contains(waiter.ProcessingResultId))
            {
                resultIds.Add(waiter.ProcessingResultId);
            }
        }

        await waiterRepository.UpdateManyAsync(waiters, autoSave: true, cancellationToken: cancellationToken);
        return resultIds;
    }

    public virtual async Task<int> InvalidateByStorageObjectIdAsync(
        Guid storageObjectId,
        CancellationToken cancellationToken = default)
    {
        var results = await resultRepository.GetListAsync(x =>
            x.OutputStorageObjectId == storageObjectId &&
            x.Status != AttachmentProcessingStatus.Invalidated,
            cancellationToken: cancellationToken);

        if (results.Count == 0)
        {
            return 0;
        }

        foreach (var item in results)
        {
            item.Status = AttachmentProcessingStatus.Invalidated;
            item.Version++;
        }

        await resultRepository.UpdateManyAsync(results, autoSave: true, cancellationToken: cancellationToken);
        return results.Count;
    }

    public virtual async Task<int> RecoverPendingGifWaitersAsync(
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
        {
            return 0;
        }

        var jobManager = LazyServiceProvider?.LazyGetService<Volo.Abp.BackgroundJobs.IBackgroundJobManager>();
        if (jobManager == null)
        {
            return 0;
        }

        var now = Clock.Now;
        var resultQuery = await resultRepository.GetQueryableAsync();
        var recoverableResultIds = resultQuery
            .Where(r => r.OutputRole == BlobRoles.Gif &&
                        (r.Status == AttachmentProcessingStatus.Ready ||
                         r.Status == AttachmentProcessingStatus.Pending ||
                         (r.Status == AttachmentProcessingStatus.Processing && r.LeaseExpireTime != null && r.LeaseExpireTime <= now) ||
                         (r.Status == AttachmentProcessingStatus.Failed && (r.NextRetryTime == null || r.NextRetryTime <= now))))
            .OrderBy(r => r.CreationTime)
            .Select(r => r.Id)
            .Take(maxResults * 2)
            .ToList();

        if (recoverableResultIds.Count == 0)
        {
            return 0;
        }

        var waiterQuery = await waiterRepository.GetQueryableAsync();
        var candidates = waiterQuery
            .Where(w => w.Status == AttachmentProcessingWaiterStatus.Pending &&
                        w.OutputRole == BlobRoles.Gif &&
                        recoverableResultIds.Contains(w.ProcessingResultId))
            .OrderBy(w => w.CreationTime)
            .Select(w => new { w.ProcessingResultId, w.MessageId, w.PrimaryBlobId })
            .Take(maxResults * 5)
            .ToList();

        var enqueued = 0;
        foreach (var group in candidates.GroupBy(x => x.ProcessingResultId).Take(maxResults))
        {
            var first = group.First();
            await jobManager.EnqueueAsync(new GenerateVideoGifArgs
            {
                MessageId = first.MessageId,
                VideoBlobId = first.PrimaryBlobId,
            });
            enqueued++;
        }

        return enqueued;
    }
}
