using IczpNet.Chat.Medias;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public class GenerateVideoGifBackgroundJob(
    IMessageRepository messageRepository,
    IRepository<VideoGifPropagationOutbox, Guid> gifOutboxRepository,
    IAsyncQueryableExecuter asyncExecuter,
    IBlobManager blobManager,
    IBlobResolver blobResolver,
    IMediaResolver mediaResolver,
    IGuidGenerator guidGenerator,
    IOptions<MessageOptions> messageOptions,
    IServiceScopeFactory serviceScopeFactory,
    IUnitOfWorkManager unitOfWorkManager,
    IAbpDistributedLock distributedLock) : AsyncBackgroundJob<GenerateVideoGifArgs>, ITransientDependency
{
    [UnitOfWork]
    public override async Task ExecuteAsync(GenerateVideoGifArgs args)
    {
        Logger.LogInformation(
            "GenerateVideoGifBackgroundJob started: MessageId={MessageId}, VideoBlobId={VideoBlobId}",
            args.MessageId,
            args.VideoBlobId);

        await using var lockHandle = distributedLock is not NullAbpDistributedLock
            ? await distributedLock.TryAcquireAsync($"Chat:VideoGif:{args.MessageId}", TimeSpan.FromMinutes(2))
            : null;
        if (distributedLock is not NullAbpDistributedLock && lockHandle == null)
        {
            Logger.LogInformation(
                "GenerateVideoGifBackgroundJob skipped due to active lock: MessageId={MessageId}",
                args.MessageId);
            return;
        }

        var queryable = await messageRepository.WithDetailsAsync(x => x.VideoContentList);
        var message = await asyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == args.MessageId));
        if (message == null || message.IsRollbacked || message.RollbackTime.HasValue)
        {
            Logger.LogWarning(
                "GenerateVideoGifBackgroundJob skipped: Message not found or rollbacked. MessageId={MessageId}, Found={Found}, IsRollbacked={IsRollbacked}",
                args.MessageId,
                message != null,
                message?.IsRollbacked);
            return;
        }

        var videoContent = message.GetTypedContentEntity<VideoContent>();
        if (videoContent == null)
        {
            Logger.LogInformation(
                "GenerateVideoGifBackgroundJob skipped: VideoContent missing. MessageId={MessageId}",
                args.MessageId);
            return;
        }

        // 若 GIF 已存在（例如重跑或并发 Job 已生成），跳过转码但仍触发下游传播以确保引用和变更完整
        if (videoContent.GifBlobId.HasValue)
        {
            var existingGifUrl = !string.IsNullOrWhiteSpace(videoContent.GifUrl)
                ? videoContent.GifUrl
                : await blobResolver.GetFileUrlAsync(videoContent.GifBlobId.Value, "image/gif");

            await EnqueuePropagationAsync(new PropagateVideoGifArgs
            {
                SourceMessageId = message.Id,
                VideoContentId = videoContent.Id,
                GifBlobId = videoContent.GifBlobId.Value,
                GifUrl = existingGifUrl,
            });
            return;
        }

        var videoBlob = (await messageRepository.GetBlobsAsNoTrackingAsync([args.VideoBlobId])).FirstOrDefault();
        if (videoBlob == null)
        {
            Logger.LogWarning(
                "GenerateVideoGifBackgroundJob skipped: VideoBlob not found. MessageId={MessageId}, VideoBlobId={VideoBlobId}",
                args.MessageId,
                args.VideoBlobId);
            return;
        }

        using var reuseScope = serviceScopeFactory.CreateScope();
        var processingReuseManager = reuseScope.ServiceProvider.GetService<IAttachmentProcessingReuseManager>();
        var currentTenant = reuseScope.ServiceProvider.GetService<Volo.Abp.MultiTenancy.ICurrentTenant>();
        var reuseEnabled = processingReuseManager?.IsEnabled() ?? false;
        var storageScope = currentTenant?.Id?.ToString("N") ?? "host";
        var gifRecipeHash = reuseEnabled
            ? processingReuseManager.BuildVideoGifRecipeHash(messageOptions.Value.VideoSetting)
            : null;

        byte[] videoBytes = null;
        string videoSha256 = null;
        long videoFileSize = videoBlob.FileSize;
        var videoContainer = videoBlob.Container;

        if (reuseEnabled && videoBlob.StorageObjectId.HasValue)
        {
            var storageObj = await blobManager.FindStorageObjectAsync(videoBlob.StorageObjectId.Value);
            if (storageObj != null && storageObj.Status == BlobObjectStatus.Ready)
            {
                storageScope = storageObj.StorageScope;
                videoContainer = storageObj.Container;
                videoSha256 = storageObj.Sha256;
                videoFileSize = storageObj.FileSize;
            }
        }

        if (reuseEnabled && string.IsNullOrWhiteSpace(videoSha256))
        {
            var location = await blobManager.ResolvePhysicalLocationAsync(videoBlob);
            videoBytes = await blobManager.GetBytesAsync(
                !string.IsNullOrWhiteSpace(location.Container) ? location.Container : videoBlob.Container,
                !string.IsNullOrWhiteSpace(location.StorageName) ? location.StorageName : videoBlob.Name);
            videoSha256 = processingReuseManager.ComputeSha256(videoBytes);
            videoFileSize = videoBytes.LongLength;
        }

        AttachmentProcessingResult claimedTask = null;
        if (reuseEnabled)
        {
            var readyGif = await processingReuseManager.FindReadyResultAsync(
                storageScope,
                videoContainer,
                videoSha256,
                videoFileSize,
                AttachmentMediaType.Video,
                BlobRoles.Gif,
                gifRecipeHash);
            if (readyGif?.OutputStorageObjectId != null)
            {
                await ApplyReadyGifToMessageAsync(message, videoContent, videoBlob, readyGif);
                await BackfillPendingWaitersAsync(processingReuseManager, readyGif);
                return;
            }

            var leaseOwner = $"{Environment.MachineName}:{Guid.NewGuid():N}";
            claimedTask = await processingReuseManager.TryClaimTaskAsync(
                storageScope,
                videoContainer,
                videoSha256,
                videoFileSize,
                AttachmentMediaType.Video,
                BlobRoles.Gif,
                gifRecipeHash,
                leaseOwner,
                TimeSpan.FromMinutes(3));

            if (claimedTask == null)
            {
                var pendingTask = await processingReuseManager.GetOrCreatePendingTaskAsync(
                    storageScope,
                    videoContainer,
                    videoSha256,
                    videoFileSize,
                    AttachmentMediaType.Video,
                    BlobRoles.Gif,
                    gifRecipeHash);
                await processingReuseManager.RegisterWaiterAsync(pendingTask.Id, message.Id, videoBlob.Id, BlobRoles.Gif);
                Logger.LogInformation(
                    "GenerateVideoGifBackgroundJob deferred to active task waiter: MessageId={MessageId}, ProcessingResultId={ResultId}",
                    args.MessageId,
                    pendingTask.Id);
                return;
            }

            if (claimedTask.Status == AttachmentProcessingStatus.Ready && claimedTask.OutputStorageObjectId.HasValue)
            {
                await ApplyReadyGifToMessageAsync(message, videoContent, videoBlob, claimedTask);
                await BackfillPendingWaitersAsync(processingReuseManager, claimedTask);
                return;
            }
        }

        if (videoBytes == null)
        {
            var location = await blobManager.ResolvePhysicalLocationAsync(videoBlob);
            videoBytes = await blobManager.GetBytesAsync(
                !string.IsNullOrWhiteSpace(location.Container) ? location.Container : videoBlob.Container,
                !string.IsNullOrWhiteSpace(location.StorageName) ? location.StorageName : videoBlob.Name);
        }

        var mediaInfo = await mediaResolver.GetVideoInfoAsync(
            videoBytes,
            videoBlob.FileName,
            new VideoProcessOptions(IncludeSnapshot: false, IncludeGif: true));
        if (mediaInfo == null || string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath))
        {
            if (claimedTask != null)
            {
                await processingReuseManager.FailTaskAsync(
                    claimedTask.Id,
                    claimedTask.Version,
                    "MediaResolver returned empty GifSnapshotPath",
                    TimeSpan.FromMinutes(1));
            }
            Logger.LogWarning(
                "GenerateVideoGifBackgroundJob skipped: MediaResolver returned empty GifSnapshotPath. MessageId={MessageId}, VideoBlobId={VideoBlobId}, HasMediaInfo={HasMediaInfo}",
                args.MessageId,
                args.VideoBlobId,
                mediaInfo != null);
            return;
        }

        try
        {
            if (!File.Exists(mediaInfo.GifSnapshotPath))
            {
                return;
            }

            var gifBytes = await File.ReadAllBytesAsync(mediaInfo.GifSnapshotPath);
            var gifBlobId = guidGenerator.Create();
            var directoryName = await blobResolver.GetDirectoryNameAsync(new BlobContext
            {
                BlobId = gifBlobId,
                Container = videoBlob.Container,
                MediaType = AttachmentMediaType.Video,
                BlobRole = BlobRoles.Gif,
                FileName = $"{videoBlob.FileName}.gif",
                MimeType = "image/gif",
                FileSize = gifBytes.Length,
            });
            var fileName = await blobResolver.GenerateFileNameAsync(".gif");
            var gifBlobName = Path.Combine(directoryName, fileName).Replace(@"\", "/");

            var gifBlob = await blobManager.CreateAsync(new Blob(gifBlobId, videoBlob.Container)
            {
                IsPublic = videoBlob.IsPublic,
                FileSize = gifBytes.Length,
                MimeType = "image/gif",
                FileName = $"{Path.GetFileNameWithoutExtension(videoBlob.FileName)}.gif",
                Name = gifBlobName,
                Suffix = ".gif",
                Bytes = gifBytes,
                UploaderChatObjectId = videoBlob.UploaderChatObjectId,
                UploadSessionUnitId = videoBlob.UploadSessionUnitId,
                UploadSessionId = videoBlob.UploadSessionId,
                ParentBlobId = videoBlob.Id,
                Role = BlobRoles.Gif,
                Status = BlobStatus.Bound,
                RefCount = 0,
            });

            var gifUrl = await blobResolver.GetFileUrlAsync(gifBlob.Id, gifBlob.MimeType);

            AttachmentProcessingResult completedResult = null;
            if (claimedTask != null && gifBlob.StorageObjectId.HasValue)
            {
                await processingReuseManager.CompleteTaskAsync(
                    claimedTask.Id,
                    claimedTask.Version,
                    gifBlob.StorageObjectId,
                    gifBlob.MimeType,
                    gifBlob.Suffix,
                    gifBlob.FileSize,
                    null,
                    null,
                    null);
                completedResult = await processingReuseManager.FindByIdAsync(claimedTask.Id);
            }
            else if (reuseEnabled && gifBlob.StorageObjectId.HasValue)
            {
                completedResult = await processingReuseManager.RecordReadyResultAsync(
                    storageScope,
                    videoContainer,
                    videoSha256,
                    videoFileSize,
                    AttachmentMediaType.Video,
                    BlobRoles.Gif,
                    gifRecipeHash,
                    gifBlob.StorageObjectId,
                    gifBlob.MimeType,
                    gifBlob.Suffix,
                    gifBlob.FileSize,
                    null,
                    null,
                    null);
            }

            // 条件更新 VideoContent，确保并发 Job 只发布一个有效 GIF
            var won = await messageRepository.TrySetVideoGifAsync(videoContent.Id, gifBlob.Id, gifUrl);
            if (!won)
            {
                var latestMessage = await asyncExecuter.FirstOrDefaultAsync(
                    (await messageRepository.WithDetailsAsync(x => x.VideoContentList)).Where(x => x.Id == args.MessageId));
                var latestVideo = latestMessage?.GetTypedContentEntity<VideoContent>();
                if (latestVideo?.GifBlobId != null)
                {
                    await EnqueuePropagationAsync(new PropagateVideoGifArgs
                    {
                        SourceMessageId = message.Id,
                        VideoContentId = videoContent.Id,
                        GifBlobId = latestVideo.GifBlobId.Value,
                        GifUrl = latestVideo.GifUrl ?? await blobResolver.GetFileUrlAsync(latestVideo.GifBlobId.Value, "image/gif"),
                    });
                }
            }
            else
            {
                videoContent.GifBlobId = gifBlob.Id;
                videoContent.GifUrl = gifUrl;

                await EnqueuePropagationAsync(new PropagateVideoGifArgs
                {
                    SourceMessageId = message.Id,
                    VideoContentId = videoContent.Id,
                    GifBlobId = gifBlob.Id,
                    GifUrl = gifUrl,
                });

                Logger.LogInformation(
                    "GenerateVideoGifBackgroundJob generated GIF and queued propagation: MessageId={MessageId}, GifBlobId={GifBlobId}",
                    message.Id,
                    gifBlob.Id);
            }

            if (reuseEnabled && completedResult != null)
            {
                await BackfillPendingWaitersAsync(processingReuseManager, completedResult);
            }
        }
        catch (Exception ex)
        {
            if (claimedTask != null)
            {
                await processingReuseManager.FailTaskAsync(
                    claimedTask.Id,
                    claimedTask.Version,
                    ex.Message,
                    TimeSpan.FromMinutes(1));
            }
            throw;
        }
        finally
        {
            DeleteTempFile(mediaInfo.ImageSnapshotPath);
            DeleteTempFile(mediaInfo.GifSnapshotPath);
            DeleteTempFile(mediaInfo.TempVideoPath);
        }
    }

    private async Task ApplyReadyGifToMessageAsync(
        Message message,
        VideoContent videoContent,
        Blob videoBlob,
        AttachmentProcessingResult readyGif)
    {
        var reusedGifBlobId = guidGenerator.Create();
        var reusedDir = await blobResolver.GetDirectoryNameAsync(new BlobContext
        {
            BlobId = reusedGifBlobId,
            Container = videoBlob.Container,
            MediaType = AttachmentMediaType.Video,
            BlobRole = BlobRoles.Gif,
            FileName = $"{videoBlob.FileName}.gif",
            MimeType = readyGif.OutputMimeType ?? "image/gif",
            FileSize = readyGif.OutputFileSize ?? 0,
        });
        var reusedFileName = await blobResolver.GenerateFileNameAsync(readyGif.OutputSuffix ?? ".gif");
        var reusedGifBlob = await blobManager.CreateAsync(new Blob(reusedGifBlobId, videoBlob.Container)
        {
            IsPublic = videoBlob.IsPublic,
            FileSize = readyGif.OutputFileSize ?? 0,
            MimeType = readyGif.OutputMimeType ?? "image/gif",
            FileName = $"{Path.GetFileNameWithoutExtension(videoBlob.FileName)}.gif",
            Name = Path.Combine(reusedDir, reusedFileName).Replace(@"\", "/"),
            Suffix = readyGif.OutputSuffix ?? ".gif",
            StorageObjectId = readyGif.OutputStorageObjectId,
            UploaderChatObjectId = videoBlob.UploaderChatObjectId,
            UploadSessionUnitId = videoBlob.UploadSessionUnitId,
            UploadSessionId = videoBlob.UploadSessionId,
            ParentBlobId = videoBlob.Id,
            Role = BlobRoles.Gif,
            Status = BlobStatus.Bound,
            RefCount = 0,
        });
        var reusedGifUrl = await blobResolver.GetFileUrlAsync(reusedGifBlob.Id, reusedGifBlob.MimeType);
        if (await messageRepository.TrySetVideoGifAsync(videoContent.Id, reusedGifBlob.Id, reusedGifUrl))
        {
            await EnqueuePropagationAsync(new PropagateVideoGifArgs
            {
                SourceMessageId = message.Id,
                VideoContentId = videoContent.Id,
                GifBlobId = reusedGifBlob.Id,
                GifUrl = reusedGifUrl,
            });
        }
    }

    private async Task BackfillPendingWaitersAsync(
        IAttachmentProcessingReuseManager processingReuseManager,
        AttachmentProcessingResult readyGif)
    {
        if (readyGif?.OutputStorageObjectId == null)
        {
            return;
        }

        var waiters = await processingReuseManager.GetPendingWaitersAsync(readyGif.Id, maxCount: 200);
        foreach (var waiter in waiters)
        {
            try
            {
                var queryable = await messageRepository.WithDetailsAsync(x => x.VideoContentList);
                var msg = await asyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == waiter.MessageId));
                if (msg == null || msg.IsRollbacked || msg.RollbackTime.HasValue)
                {
                    await processingReuseManager.MarkWaiterStatusAsync(waiter.Id, AttachmentProcessingWaiterStatus.Skipped);
                    continue;
                }

                var vc = msg.GetTypedContentEntity<VideoContent>();
                if (vc == null || vc.GifBlobId.HasValue)
                {
                    await processingReuseManager.MarkWaiterStatusAsync(waiter.Id, AttachmentProcessingWaiterStatus.Skipped);
                    continue;
                }

                var vBlob = (await messageRepository.GetBlobsAsNoTrackingAsync([waiter.PrimaryBlobId])).FirstOrDefault();
                if (vBlob == null)
                {
                    await processingReuseManager.MarkWaiterStatusAsync(waiter.Id, AttachmentProcessingWaiterStatus.Skipped);
                    continue;
                }

                await ApplyReadyGifToMessageAsync(msg, vc, vBlob, readyGif);
                await processingReuseManager.MarkWaiterStatusAsync(waiter.Id, AttachmentProcessingWaiterStatus.Completed);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to backfill GIF waiter: WaiterId={WaiterId}, MessageId={MessageId}", waiter.Id, waiter.MessageId);
            }
        }
    }

    private async Task EnqueuePropagationAsync(PropagateVideoGifArgs propArgs)
    {
        // Write to the domain outbox inside the active business UOW so propagation is strictly atomic
        // with GIF persistence, even though Quartz uses a separate connection/JobStoreTX.
        await gifOutboxRepository.InsertAsync(
            new VideoGifPropagationOutbox(
                guidGenerator.Create(),
                propArgs.SourceMessageId,
                propArgs.VideoContentId,
                propArgs.GifBlobId,
                propArgs.GifUrl),
            autoSave: true);

        // Best-effort fast-path enqueue only after the business transaction commits; if skipped or interrupted,
        // VideoGifPropagationWorker delivers the outbox record reliably.
        if (unitOfWorkManager.Current is { } currentUow)
        {
            currentUow.OnCompleted(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var jobManager = scope.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
                    await jobManager.EnqueueAsync(propArgs);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Best-effort GIF propagation Quartz enqueue failed; outbox worker will deliver. MessageId={MessageId}", propArgs.SourceMessageId);
                }
            });
        }
    }

    private void DeleteTempFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var root = Path.GetFullPath(messageOptions.Value.OutputTempPath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            if (fullPath.StartsWith(root, StringComparison.Ordinal))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not delete temporary media file {Path}", path);
        }
    }
}
