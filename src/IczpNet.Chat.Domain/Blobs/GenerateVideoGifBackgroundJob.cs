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
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public class GenerateVideoGifBackgroundJob(
    IMessageRepository messageRepository,
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

        var videoBytes = await blobManager.GetBytesAsync(videoBlob.Container, videoBlob.Name);
        var mediaInfo = await mediaResolver.GetVideoInfoAsync(videoBytes, videoBlob.FileName);
        if (mediaInfo == null || string.IsNullOrWhiteSpace(mediaInfo.GifSnapshotPath))
        {
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
                return;
            }

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
        finally
        {
            DeleteTempFile(mediaInfo.ImageSnapshotPath);
            DeleteTempFile(mediaInfo.GifSnapshotPath);
            DeleteTempFile(mediaInfo.TempVideoPath);
        }
    }

    private async Task EnqueuePropagationAsync(PropagateVideoGifArgs propArgs)
    {
        if (unitOfWorkManager.Current != null)
        {
            unitOfWorkManager.Current.OnCompleted(async () =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                var jobManager = scope.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
                await jobManager.EnqueueAsync(propArgs);
            });
        }
        else
        {
            using var scope = serviceScopeFactory.CreateScope();
            var jobManager = scope.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
            await jobManager.EnqueueAsync(propArgs);
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
