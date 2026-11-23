using IczpNet.Chat.Blobs;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class MessageManager
{
    protected IRepository<Blob, Guid> BlobRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Blob, Guid>>();
    protected IRepository<BlobMessageRef, Guid> BlobMessageRefRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BlobMessageRef, Guid>>();
    protected IBlobResolver BlobResolver => LazyServiceProvider.LazyGetRequiredService<IBlobResolver>();
    protected IBackgroundJobManager BackgroundJobManager => LazyServiceProvider.LazyGetRequiredService<IBackgroundJobManager>();
    protected UrlNormalizers.IUrlNormalizer UrlNormalizer => LazyServiceProvider.LazyGetRequiredService<UrlNormalizers.IUrlNormalizer>();

    /// <summary>
    /// 将消息内容 DTO 中的本站绝对 URL 还原为相对路径后再序列化入库，便于后续域名/地址迁移
    /// </summary>
    protected virtual string SerializeContentJsonForStorage(Message message)
    {
        var contentDto = message.GetContentDto();
        switch (contentDto)
        {
            case VideoContentInfo video:
                video.Url = UrlNormalizer.ToRelative(video.Url);
                video.SnapshotUrl = UrlNormalizer.ToRelative(video.SnapshotUrl);
                video.SnapshotThumbnailUrl = UrlNormalizer.ToRelative(video.SnapshotThumbnailUrl);
                video.GifUrl = UrlNormalizer.ToRelative(video.GifUrl);
                if (video.ImageWidth <= 0 && video.Width > 0) video.ImageWidth = video.Width;
                if (video.ImageHeight <= 0 && video.Height > 0) video.ImageHeight = video.Height;
                if (video.Width <= 0 && video.ImageWidth > 0) video.Width = video.ImageWidth;
                if (video.Height <= 0 && video.ImageHeight > 0) video.Height = video.ImageHeight;
                break;
            case ImageContentInfo image:
                image.Url = UrlNormalizer.ToRelative(image.Url);
                image.OriginalUrl = UrlNormalizer.ToRelative(image.OriginalUrl);
                image.ThumbnailUrl = UrlNormalizer.ToRelative(image.ThumbnailUrl);
                image.ActionUrl = UrlNormalizer.ToRelative(image.ActionUrl);
                image.ThumbnailActionUrl = UrlNormalizer.ToRelative(image.ThumbnailActionUrl);
                break;
            case SoundContentInfo sound:
                sound.Url = UrlNormalizer.ToRelative(sound.Url);
                break;
            case FileContentInfo file:
                file.Url = UrlNormalizer.ToRelative(file.Url);
                file.ActionUrl = UrlNormalizer.ToRelative(file.ActionUrl);
                break;
        }
        return JsonSerializer.Serialize(contentDto);
    }

    protected virtual List<Guid> CollectAttachmentBlobIds(IContentEntity messageContent)
    {
        if (messageContent is not MessageContentAttachmentsEntityBase attachment)
        {
            return [];
        }

        var ids = new List<Guid>();
        if (attachment.BlobId.HasValue && attachment.BlobId.Value != Guid.Empty)
        {
            ids.Add(attachment.BlobId.Value);
        }

        switch (attachment)
        {
            case ImageContent img:
                if (img.ThumbnailBlobId.HasValue) ids.Add(img.ThumbnailBlobId.Value);
                if (img.OriginalBlobId.HasValue) ids.Add(img.OriginalBlobId.Value);
                break;
            case VideoContent video:
                if (video.SnapshotBlobId.HasValue) ids.Add(video.SnapshotBlobId.Value);
                if (video.SnapshotThumbnailBlobId.HasValue) ids.Add(video.SnapshotThumbnailBlobId.Value);
                if (video.GifBlobId.HasValue) ids.Add(video.GifBlobId.Value);
                break;
        }

        return ids.Distinct().ToList();
    }

    /// <summary>
    /// 校验前端传入的附件 BlobId 归属与父子关系，并由服务端强制回填真实 URL（防越权/防伪造）
    /// </summary>
    protected virtual async Task PrepareAndValidateAttachmentAsync(SessionUnitCacheItem senderSessionUnit, IContentEntity messageContent)
    {
        if (messageContent is not MessageContentAttachmentsEntityBase attachment)
        {
            return;
        }

        var blobIds = CollectAttachmentBlobIds(messageContent);
        if (blobIds.Count == 0)
        {
            return;
        }

        var blobs = (await BlobRepository.GetListAsync(x => blobIds.Contains(x.Id)))
            .ToDictionary(x => x.Id);

        foreach (var id in blobIds)
        {
            if (!blobs.TryGetValue(id, out var blob))
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AttachmentNotFoundOrForbidden)
                    .WithData("BlobId", id);
            }

            if (blob.UploaderChatObjectId.HasValue && blob.UploaderChatObjectId.Value != senderSessionUnit.OwnerId)
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AttachmentNotFoundOrForbidden)
                    .WithData("BlobId", id);
            }

            if (attachment.BlobId.HasValue &&
                blob.Id != attachment.BlobId.Value &&
                blob.ParentBlobId.HasValue &&
                blob.ParentBlobId.Value != attachment.BlobId.Value)
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AttachmentNotFoundOrForbidden)
                    .WithData("BlobId", id);
            }
        }

        if (attachment.BlobId.HasValue && blobs.TryGetValue(attachment.BlobId.Value, out var primaryBlob))
        {
            attachment.Url = await BlobResolver.GetFileUrlAsync(primaryBlob.Id, primaryBlob.MimeType);
            attachment.FileName ??= primaryBlob.FileName;
            attachment.ContentType ??= primaryBlob.MimeType;
            attachment.Suffix ??= primaryBlob.Suffix;
            attachment.Size ??= primaryBlob.FileSize;
        }

        if (attachment is ImageContent image)
        {
            if (image.ThumbnailBlobId.HasValue && blobs.TryGetValue(image.ThumbnailBlobId.Value, out var thumbBlob))
            {
                image.ThumbnailUrl = await BlobResolver.GetFileUrlAsync(thumbBlob.Id, thumbBlob.MimeType);
            }
            if (image.OriginalBlobId.HasValue && blobs.TryGetValue(image.OriginalBlobId.Value, out var origBlob))
            {
                image.OriginalUrl = await BlobResolver.GetFileUrlAsync(origBlob.Id, origBlob.MimeType);
            }
        }
        else if (attachment is VideoContent video)
        {
            if ((video.ImageWidth == null || video.ImageWidth <= 0) && video.Width > 0) video.ImageWidth = video.Width;
            if ((video.ImageHeight == null || video.ImageHeight <= 0) && video.Height > 0) video.ImageHeight = video.Height;
            if ((video.Width == null || video.Width <= 0) && video.ImageWidth > 0) video.Width = video.ImageWidth;
            if ((video.Height == null || video.Height <= 0) && video.ImageHeight > 0) video.Height = video.ImageHeight;
            if (video.SnapshotBlobId.HasValue && blobs.TryGetValue(video.SnapshotBlobId.Value, out var snapBlob))
            {
                video.SnapshotUrl = await BlobResolver.GetFileUrlAsync(snapBlob.Id, snapBlob.MimeType);
            }
            if (video.SnapshotThumbnailBlobId.HasValue && blobs.TryGetValue(video.SnapshotThumbnailBlobId.Value, out var snapThumbBlob))
            {
                video.SnapshotThumbnailUrl = await BlobResolver.GetFileUrlAsync(snapThumbBlob.Id, snapThumbBlob.MimeType);
            }
            if (video.GifBlobId.HasValue && blobs.TryGetValue(video.GifBlobId.Value, out var gifBlob))
            {
                video.GifUrl = await BlobResolver.GetFileUrlAsync(gifBlob.Id, gifBlob.MimeType);
            }
        }
    }

    /// <summary>
    /// 消息落库后绑定 Blob 状态、引用计数与 BlobMessageRef 授权记录，并按需触发视频 GIF 后台生成
    /// </summary>
    protected virtual async Task BindAttachmentsAfterInsertAsync(Message message, IContentEntity messageContent)
    {
        var blobIds = CollectAttachmentBlobIds(messageContent);
        if (blobIds.Count == 0)
        {
            return;
        }

        var sourceType = message.ForwardMessageId.HasValue
            ? BlobRefSourceTypes.Forward
            : BlobRefSourceTypes.Send;

        var blobs = await BlobRepository.GetListAsync(x => blobIds.Contains(x.Id));
        foreach (var blob in blobs)
        {
            blob.Status = BlobStatus.Bound;
            blob.ExpireTime = null;
            blob.RefCount += 1;
            await BlobMessageRefRepository.InsertAsync(new BlobMessageRef(
                GuidGenerator.Create(),
                blob.Id,
                message.Id,
                message.SessionId,
                sourceType));
        }

        if (!message.ForwardMessageId.HasValue &&
            messageContent is VideoContent video &&
            video.BlobId.HasValue &&
            !video.GifBlobId.HasValue &&
            Config.VideoSetting.IsGenerateSnapshot)
        {
            var args = new GenerateVideoGifArgs
            {
                MessageId = message.Id,
                VideoBlobId = video.BlobId.Value,
            };

            if (UnitOfWorkManager.Current != null)
            {
                var scopeFactory = LazyServiceProvider.LazyGetRequiredService<IServiceScopeFactory>();
                UnitOfWorkManager.Current.OnCompleted(async () =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var jobManager = scope.ServiceProvider.GetRequiredService<IBackgroundJobManager>();
                    var jobId = await jobManager.EnqueueAsync(args);
                    Logger.LogInformation(
                        "Enqueued GenerateVideoGifArgs: JobId={JobId}, MessageId={MessageId}, VideoBlobId={VideoBlobId}",
                        jobId,
                        args.MessageId,
                        args.VideoBlobId);
                });
            }
            else
            {
                var jobId = await BackgroundJobManager.EnqueueAsync(args);
                Logger.LogInformation(
                    "Enqueued GenerateVideoGifArgs: JobId={JobId}, MessageId={MessageId}, VideoBlobId={VideoBlobId}",
                    jobId,
                    args.MessageId,
                    args.VideoBlobId);
            }
        }
    }

    public virtual async Task BindHistoryAttachmentsAsync(
        long historyRootMessageId,
        Guid? targetSessionId,
        IEnumerable<long> selectedMessageIds,
        int maxDepth = 5)
    {
        var historyMessageRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<HistoryMessage>>();
        var visitedMessageIds = new HashSet<long>();
        var collectedBlobIds = new HashSet<Guid>();
        var currentFrontier = selectedMessageIds.Distinct().ToList();

        for (int depth = 1; depth <= maxDepth && currentFrontier.Count > 0; depth++)
        {
            var toProcess = currentFrontier.Where(id => visitedMessageIds.Add(id)).ToList();
            if (toProcess.Count == 0)
            {
                break;
            }

            // 1. 收集当前层消息引用的 QuoteMessageId（仅同会话内引用）
            var msgRows = (await Repository.GetQueryableAsync())
                .Where(x => toProcess.Contains(x.Id))
                .Select(x => new { x.Id, x.MessageType, x.QuoteMessageId, x.SessionId })
                .ToList();

            var quoteIds = msgRows
                .Where(x => x.QuoteMessageId.HasValue)
                .Select(x => x.QuoteMessageId!.Value)
                .Distinct()
                .Where(qId => !visitedMessageIds.Contains(qId))
                .ToList();

            var allQueryIds = toProcess.Concat(quoteIds).Distinct().ToList();

            // 2. 从 BlobMessageRef 收集所有相关 BlobId
            var refBlobIds = (await BlobMessageRefRepository.GetQueryableAsync())
                .Where(x => allQueryIds.Contains(x.MessageId))
                .Select(x => x.BlobId)
                .Distinct()
                .ToList();

            foreach (var bId in refBlobIds)
            {
                collectedBlobIds.Add(bId);
            }

            // 3. 向下展开嵌套的 HistoryContent 消息（受 maxDepth = 5 限制）
            if (depth < maxDepth)
            {
                var historyMsgIds = msgRows
                    .Where(x => x.MessageType == MessageTypes.History)
                    .Select(x => x.Id)
                    .ToList();

                if (historyMsgIds.Count > 0)
                {
                    currentFrontier = (await historyMessageRepo.GetQueryableAsync())
                        .Where(hm => hm.HistoryContent.MessageList.Any(pm => historyMsgIds.Contains(pm.Id)))
                        .Select(hm => hm.MessageId)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    currentFrontier = [];
                }
            }
            else
            {
                currentFrontier = [];
            }
        }

        if (collectedBlobIds.Count == 0)
        {
            return;
        }

        var existingBlobIds = (await BlobMessageRefRepository.GetQueryableAsync())
            .Where(x => x.MessageId == historyRootMessageId && collectedBlobIds.Contains(x.BlobId))
            .Select(x => x.BlobId)
            .ToHashSet();

        var newBlobIds = collectedBlobIds.Where(id => !existingBlobIds.Contains(id)).ToList();
        if (newBlobIds.Count == 0)
        {
            return;
        }

        var blobs = await BlobRepository.GetListAsync(x => newBlobIds.Contains(x.Id));
        foreach (var blob in blobs)
        {
            blob.RefCount += 1;
            await BlobMessageRefRepository.InsertAsync(new BlobMessageRef(
                GuidGenerator.Create(),
                blob.Id,
                historyRootMessageId,
                targetSessionId,
                BlobRefSourceTypes.History));
        }
    }
}
