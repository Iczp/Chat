using IczpNet.Chat.Commands;
using IczpNet.Chat.Hosting;
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
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Json;
using Volo.Abp.Linq;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Blobs;

public class GenerateVideoGifBackgroundJob(
    IMessageRepository messageRepository,
    IAsyncQueryableExecuter asyncExecuter,
    IMessageManager messageManager,
    IBlobManager blobManager,
    IBlobResolver blobResolver,
    IRepository<Blob, Guid> blobRepository,
    IRepository<BlobMessageRef, Guid> blobMessageRefRepository,
    IMediaResolver mediaResolver,
    IGuidGenerator guidGenerator,
    IJsonSerializer jsonSerializer,
    IOptions<MessageOptions> messageOptions,
    IDistributedCache<MessageCacheItem, MessageCacheKey> messageCache,
    IClock clock,
    IServiceScopeFactory serviceScopeFactory,
    ICurrentHosted currentHosted,
    IUnitOfWorkManager unitOfWorkManager) : AsyncBackgroundJob<GenerateVideoGifArgs>, ITransientDependency
{
    [UnitOfWork]
    public override async Task ExecuteAsync(GenerateVideoGifArgs args)
    {
        Logger.LogInformation(
            "GenerateVideoGifBackgroundJob started: MessageId={MessageId}, VideoBlobId={VideoBlobId}",
            args.MessageId,
            args.VideoBlobId);

        var queryable = await messageRepository.WithDetailsAsync(x => x.VideoContentList);
        var message = await asyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == args.MessageId));
        if (message == null || message.IsRollbacked)
        {
            Logger.LogWarning(
                "GenerateVideoGifBackgroundJob skipped: Message not found or rollbacked. MessageId={MessageId}, Found={Found}, IsRollbacked={IsRollbacked}",
                args.MessageId,
                message != null,
                message?.IsRollbacked);
            return;
        }

        var videoContent = message.GetTypedContentEntity<VideoContent>();
        if (videoContent == null || videoContent.GifBlobId.HasValue)
        {
            Logger.LogInformation(
                "GenerateVideoGifBackgroundJob skipped: VideoContent missing or GifBlobId already set. MessageId={MessageId}, HasVideoContent={HasVideoContent}, GifBlobId={GifBlobId}",
                args.MessageId,
                videoContent != null,
                videoContent?.GifBlobId);
            return;
        }

        var videoBlob = await blobRepository.FindAsync(args.VideoBlobId);
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
                RefCount = 1,
            });

            var gifUrl = await blobResolver.GetFileUrlAsync(gifBlob.Id, gifBlob.MimeType);

            // 绑定到原消息（以及已转发到其他会话的消息引用）
            await blobMessageRefRepository.InsertAsync(new BlobMessageRef(
                guidGenerator.Create(),
                gifBlob.Id,
                message.Id,
                message.SessionId,
                BlobRefSourceTypes.Send));

            var forwardedMessages = (await messageRepository.GetQueryableAsync())
                .Where(x => x.ForwardMessageId == message.Id)
                .Select(x => new { x.Id, x.SessionId })
                .ToList();

            foreach (var fwd in forwardedMessages)
            {
                await blobMessageRefRepository.InsertAsync(new BlobMessageRef(
                    guidGenerator.Create(),
                    gifBlob.Id,
                    fwd.Id,
                    fwd.SessionId,
                    BlobRefSourceTypes.Forward));
                gifBlob.RefCount += 1;
            }

            var relativeVideoUrl = await blobResolver.GetFileUrlAsync(videoContent.BlobId ?? args.VideoBlobId, videoBlob.MimeType);
            var relativeSnapshotUrl = videoContent.SnapshotBlobId.HasValue
                ? await blobResolver.GetFileUrlAsync(videoContent.SnapshotBlobId.Value, "image/png")
                : null;
            var relativeSnapshotThumbnailUrl = videoContent.SnapshotThumbnailBlobId.HasValue
                ? await blobResolver.GetFileUrlAsync(videoContent.SnapshotThumbnailBlobId.Value, "image/png")
                : null;

            object contentDto = null;
            // 先写 Chat_MessageChange，再在回调中统一修改 VideoContent / ContentJson 与 MessageChangeId，
            // 确保只触发一次 UPDATE [Chat_Message] 与一次 EntityUpdatedEventData<Message>
            var changeVersion = await messageManager.RecordMessageUpdatedAsync(
                message,
                clock.Now,
                "生成视频GIF缩略图",
                applyChanges: msg =>
                {
                    videoContent.GifBlobId = gifBlob.Id;
                    videoContent.GifUrl = gifUrl;
                    contentDto = msg.GetContentDto();
                    if (contentDto is VideoContentInfo videoDto)
                    {
                        videoDto.Url = relativeVideoUrl;
                        if (relativeSnapshotUrl != null) videoDto.SnapshotUrl = relativeSnapshotUrl;
                        if (relativeSnapshotThumbnailUrl != null) videoDto.SnapshotThumbnailUrl = relativeSnapshotThumbnailUrl;
                        videoDto.GifUrl = gifUrl;
                    }
                    msg.SetContentJson(jsonSerializer.Serialize(contentDto));
                });

            await messageCache.RemoveAsync(new MessageCacheKey(message.Id));

            Logger.LogInformation(
                "GenerateVideoGifBackgroundJob completed: MessageId={MessageId}, GifBlobId={GifBlobId}, ChangeVersion={ChangeVersion}",
                message.Id,
                gifBlob.Id,
                changeVersion);

            if (!changeVersion.HasValue && message.SessionId.HasValue)
            {
                var notification = new SendMessageToClientDistributedEto
                {
                    Command = CommandConsts.MessageUpdated,
                    HostName = currentHosted.Name,
                    MessageId = message.Id,
                    SessionId = message.SessionId,
                    ChangeType = "Updated",
                    ChangeScope = "Shared",
                    Message = new MessageInfo<object>
                    {
                        Id = message.Id,
                        SyncVersion = message.SyncVersion,
                        ClientMessageId = message.ClientMessageId,
                        SessionId = message.SessionId.Value,
                        SessionSeq = message.SessionSeq,
                        SenderId = message.SenderId,
                        SenderSessionUnitId = message.SenderSessionUnitId,
                        ReceiverId = message.ReceiverId,
                        ReceiverSessionUnitId = message.ReceiverSessionUnitId,
                        MessageType = message.MessageType,
                        CreationTime = message.CreationTime,
                        IsPrivate = message.IsPrivate,
                        IsRollbacked = message.IsRollbackMessage(),
                        RollbackTime = message.RollbackTime,
                        Content = contentDto,
                    },
                };

                if (unitOfWorkManager.Current != null)
                {
                    unitOfWorkManager.Current.OnCompleted(async () =>
                    {
                        using var scope = serviceScopeFactory.CreateScope();
                        var bus = scope.ServiceProvider.GetRequiredService<IDistributedEventBus>();
                        await bus.PublishAsync(notification, onUnitOfWorkComplete: false, useOutbox: false);
                    });
                }
                else
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var bus = scope.ServiceProvider.GetRequiredService<IDistributedEventBus>();
                    await bus.PublishAsync(notification, onUnitOfWorkComplete: false, useOutbox: false);
                }
            }
        }
        finally
        {
            DeleteTempFile(mediaInfo.ImageSnapshotPath);
            DeleteTempFile(mediaInfo.GifSnapshotPath);
            DeleteTempFile(mediaInfo.TempVideoPath);
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
