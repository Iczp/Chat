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
using Volo.Abp.Users;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class MessageManager
{
    protected IRepository<BlobMessageRef, Guid> BlobMessageRefRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BlobMessageRef, Guid>>();
    protected IBlobResolver BlobResolver => LazyServiceProvider.LazyGetRequiredService<IBlobResolver>();
    protected IBackgroundJobManager BackgroundJobManager => LazyServiceProvider.LazyGetRequiredService<IBackgroundJobManager>();
    protected UrlNormalizers.IUrlNormalizer UrlNormalizer => LazyServiceProvider.LazyGetRequiredService<UrlNormalizers.IUrlNormalizer>();
    protected ICurrentUser CurrentUser => LazyServiceProvider.LazyGetService<ICurrentUser>();

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
        var json = JsonSerializer.Serialize(contentDto);
        if (contentDto is TemplateContentInfo && System.Text.Encoding.UTF8.GetByteCount(json) >
            MessageTemplates.MessageTemplateConsts.MaxSnapshotBytes)
            throw new UserFriendlyException("模板消息快照超过大小限制。", ChatErrorCodes.TemplateMessages.TemplatePayloadTooLarge);
        return json;
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

        var blobs = (await Repository.GetBlobsAsNoTrackingAsync(blobIds))
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

            if (blob.Status == BlobStatus.Expired ||
                (blob.Status == BlobStatus.Temp && blob.ExpireTime.HasValue && blob.ExpireTime.Value < Clock.Now))
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
        if (messageContent is HistoryContent historyContent)
        {
            if (message.ForwardMessageId.HasValue)
            {
                await BindHistoryAttachmentsAsync(message.Id, message.SessionId, [message.ForwardMessageId.Value]);
            }
            else
            {
                var selectedIds = historyContent.HistoryMessageList?
                    .Select(x => x.MessageId > 0 ? x.MessageId : x.Message?.Id ?? 0)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList() ?? [];
                await BindHistoryAttachmentsAsync(message.Id, message.SessionId, selectedIds);
            }
            return;
        }

        var blobIds = CollectAttachmentBlobIds(messageContent);
        if (blobIds.Count == 0)
        {
            return;
        }

        var sourceType = message.ForwardMessageId.HasValue
            ? BlobRefSourceTypes.Forward
            : BlobRefSourceTypes.Send;

        await Repository.BindBlobsToMessageAsync(
            message.Id,
            message.SessionId,
            blobIds,
            sourceType,
            CurrentUser?.Id,
            Clock.Now,
            markAsBound: true);

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
        const int maxVisitedMessages = 500;
        var effectiveMaxDepth = Math.Clamp(maxDepth, 1, 5);
        var historyMessageRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<HistoryMessage>>();
        var visitedMessageIds = new HashSet<long>();
        var collectedBlobIds = new HashSet<Guid>();
        var currentFrontier = selectedMessageIds?.Where(x => x > 0).Distinct().ToList() ?? [];

        for (int depth = 1; depth <= effectiveMaxDepth && currentFrontier.Count > 0 && visitedMessageIds.Count < maxVisitedMessages; depth++)
        {
            var remainingBudget = maxVisitedMessages - visitedMessageIds.Count;
            var toProcess = currentFrontier
                .Where(id => visitedMessageIds.Add(id))
                .Take(remainingBudget)
                .ToList();
            if (toProcess.Count == 0)
            {
                break;
            }

            // 1. 仅处理非私密且未撤回的消息
            var msgRows = (await Repository.GetQueryableAsync())
                .Where(x => toProcess.Contains(x.Id) && !x.IsPrivate && !x.IsRollbacked && !x.RollbackTime.HasValue)
                .Select(x => new { x.Id, x.MessageType, x.QuoteMessageId, x.SessionId })
                .ToList();

            if (msgRows.Count == 0)
            {
                break;
            }

            // 2. 收集同会话内非私密、未撤回的引用消息
            var candidateQuotePairs = msgRows
                .Where(x => x.QuoteMessageId.HasValue && x.SessionId.HasValue)
                .Select(x => new { QuoteId = x.QuoteMessageId!.Value, SessionId = x.SessionId!.Value })
                .Distinct()
                .ToList();

            var validQuoteIds = new List<long>();
            if (candidateQuotePairs.Count > 0)
            {
                var qIds = candidateQuotePairs.Select(x => x.QuoteId).Distinct().ToList();
                var quoteSessionMap = (await Repository.GetQueryableAsync())
                    .Where(x => qIds.Contains(x.Id) && !x.IsPrivate && !x.IsRollbacked && !x.RollbackTime.HasValue && x.SessionId.HasValue)
                    .Select(x => new { x.Id, SessionId = x.SessionId!.Value })
                    .ToDictionary(x => x.Id, x => x.SessionId);

                foreach (var pair in candidateQuotePairs)
                {
                    if (quoteSessionMap.TryGetValue(pair.QuoteId, out var qSessionId) &&
                        qSessionId == pair.SessionId &&
                        !visitedMessageIds.Contains(pair.QuoteId))
                    {
                        validQuoteIds.Add(pair.QuoteId);
                    }
                }
            }

            var visibleProcessIds = msgRows.Select(x => x.Id).ToList();
            var allQueryIds = visibleProcessIds.Concat(validQuoteIds).Distinct().ToList();

            // 3. 从 BlobMessageRef 收集所有已授权 BlobId
            var refRows = (await BlobMessageRefRepository.GetQueryableAsync())
                .Where(x => allQueryIds.Contains(x.MessageId))
                .Select(x => new { x.MessageId, x.BlobId })
                .ToList();

            foreach (var r in refRows)
            {
                collectedBlobIds.Add(r.BlobId);
            }

            // 4. 历史数据缺少 BlobMessageRef 时，按结构化附件表精确回退收集
            var coveredMessageIds = refRows.Select(x => x.MessageId).ToHashSet();
            var missingRefAttachmentIds = allQueryIds.Where(id => !coveredMessageIds.Contains(id)).ToList();
            if (missingRefAttachmentIds.Count > 0)
            {
                var legacyMessages = (await Repository.GetQueryableAsync())
                    .Where(x => missingRefAttachmentIds.Contains(x.Id) &&
                        (x.MessageType == MessageTypes.Image ||
                         x.MessageType == MessageTypes.Video ||
                         x.MessageType == MessageTypes.Sound ||
                         x.MessageType == MessageTypes.File))
                    .ToList();
                if (legacyMessages.Count > 0)
                {
                    await Repository.LoadMessageContentsBatchAsync(legacyMessages);
                    foreach (var legacyMsg in legacyMessages)
                    {
                        foreach (var bId in CollectAttachmentBlobIds(legacyMsg.GetTypedContentEntity()))
                        {
                            collectedBlobIds.Add(bId);
                        }
                    }
                }
            }

            // 5. 向下展开嵌套的 HistoryContent 消息（受 maxDepth = 5 与 maxVisitedMessages 限制）
            if (depth < effectiveMaxDepth && visitedMessageIds.Count < maxVisitedMessages)
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

        await Repository.BindBlobsToMessageAsync(
            historyRootMessageId,
            targetSessionId,
            collectedBlobIds.ToList(),
            BlobRefSourceTypes.History,
            CurrentUser?.Id,
            Clock.Now,
            markAsBound: false);
    }

    public virtual async Task PropagateVideoGifAsync(
        long sourceMessageId,
        Guid videoContentId,
        Guid gifBlobId,
        string gifUrl,
        int batchSize = 100)
    {
        var effectiveBatchSize = Math.Clamp(batchSize, 10, 500);
        var historyMessageRepo = LazyServiceProvider.LazyGetRequiredService<IRepository<HistoryMessage>>();
        var relativeGifUrl = UrlNormalizer.ToRelative(gifUrl);
        long cursorId = 0;

        while (true)
        {
            List<long> batchIds;
            using (var queryUow = UnitOfWorkManager.Begin(new Volo.Abp.Uow.AbpUnitOfWorkOptions { IsTransactional = false }, requiresNew: true))
            {
                batchIds = (await Repository.GetQueryableAsync())
                    .Where(m => m.Id > cursorId &&
                        !m.IsRollbacked &&
                        !m.RollbackTime.HasValue &&
                        m.VideoContentList.Any(v => v.Id == videoContentId))
                    .OrderBy(m => m.Id)
                    .Select(m => m.Id)
                    .Take(effectiveBatchSize)
                    .ToList();
                await queryUow.CompleteAsync();
            }

            if (batchIds.Count == 0)
            {
                break;
            }

            cursorId = batchIds[^1];

            using (var uow = UnitOfWorkManager.Begin(new Volo.Abp.Uow.AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true))
            {
                var messages = (await Repository.GetQueryableAsync())
                    .Where(m => batchIds.Contains(m.Id) && !m.IsRollbacked && !m.RollbackTime.HasValue)
                    .OrderBy(m => m.Id)
                    .ToList();

                if (messages.Count > 0)
                {
                    await Repository.LoadMessageContentsBatchAsync(messages);
                }

                foreach (var msg in messages)
                {
                    var sourceType = msg.Id == sourceMessageId && !msg.ForwardMessageId.HasValue
                        ? BlobRefSourceTypes.Send
                        : BlobRefSourceTypes.Forward;

                    await Repository.BindBlobsToMessageAsync(
                        msg.Id,
                        msg.SessionId,
                        [gifBlobId],
                        sourceType,
                        creatorId: null,
                        now: Clock.Now,
                        markAsBound: true);

                    if (NeedsVideoGifSnapshotUpdate(msg, gifBlobId, relativeGifUrl))
                    {
                        await RecordMessageUpdatedAsync(
                            msg,
                            Clock.Now,
                            "生成视频GIF缩略图",
                            applyChanges: target => ApplyVideoGifSnapshot(target, gifBlobId, relativeGifUrl));
                    }
                }

                // 沿 HistoryMessage 与同会话引用关系向上反查受影响的聊天记录根消息（最多 5 层）
                var affectedHistoryIds = new HashSet<long>();
                var frontier = batchIds;
                for (int depth = 1; depth <= 5 && frontier.Count > 0; depth++)
                {
                    var parentHistoryIds = (await historyMessageRepo.GetQueryableAsync())
                        .Where(hm => frontier.Contains(hm.MessageId))
                        .SelectMany(hm => hm.HistoryContent.MessageList
                            .Where(pm => !pm.IsRollbacked && !pm.RollbackTime.HasValue)
                            .Select(pm => pm.Id))
                        .Distinct()
                        .ToList();

                    var frontierSessions = (await Repository.GetQueryableAsync())
                        .Where(m => frontier.Contains(m.Id) && m.SessionId.HasValue)
                        .Select(m => new { m.Id, SessionId = m.SessionId!.Value })
                        .ToList();
                    var quoteParentIds = new List<long>();
                    foreach (var grp in frontierSessions.GroupBy(x => x.SessionId))
                    {
                        var sid = grp.Key;
                        var childIds = grp.Select(x => x.Id).ToList();
                        quoteParentIds.AddRange((await Repository.GetQueryableAsync())
                            .Where(m => m.SessionId == sid &&
                                m.QuoteMessageId.HasValue &&
                                childIds.Contains(m.QuoteMessageId.Value) &&
                                !m.IsRollbacked &&
                                !m.RollbackTime.HasValue)
                            .Select(m => m.Id)
                            .ToList());
                    }

                    var newlyFound = parentHistoryIds
                        .Concat(quoteParentIds)
                        .Distinct()
                        .Where(id => affectedHistoryIds.Add(id))
                        .Take(500)
                        .ToList();
                    frontier = newlyFound;
                }

                if (affectedHistoryIds.Count > 0)
                {
                    var historyRows = (await Repository.GetQueryableAsync())
                        .Where(m => affectedHistoryIds.Contains(m.Id) && !m.IsRollbacked && !m.RollbackTime.HasValue)
                        .Select(m => new { m.Id, m.SessionId })
                        .ToList();

                    var newlyBoundHistoryIds = new List<long>();
                    foreach (var hist in historyRows)
                    {
                        var addedCount = await Repository.BindBlobsToMessageAsync(
                            hist.Id,
                            hist.SessionId,
                            [gifBlobId],
                            BlobRefSourceTypes.History,
                            creatorId: null,
                            now: Clock.Now,
                            markAsBound: false);
                        if (addedCount > 0)
                        {
                            newlyBoundHistoryIds.Add(hist.Id);
                        }
                    }

                    if (newlyBoundHistoryIds.Count > 0)
                    {
                        var changedHistMessages = (await Repository.GetQueryableAsync())
                            .Where(m => newlyBoundHistoryIds.Contains(m.Id) && !m.IsRollbacked && !m.RollbackTime.HasValue)
                            .ToList();
                        foreach (var histMsg in changedHistMessages)
                        {
                            await RecordMessageUpdatedAsync(histMsg, Clock.Now, "更新聊天记录视频GIF");
                        }
                    }
                }

                await uow.CompleteAsync();
            }

            if (batchIds.Count < effectiveBatchSize)
            {
                break;
            }
        }
    }

    private bool NeedsVideoGifSnapshotUpdate(Message message, Guid gifBlobId, string relativeGifUrl)
    {
        var videoContent = message.GetTypedContentEntity<VideoContent>();
        if (videoContent != null && (videoContent.GifBlobId != gifBlobId || !string.Equals(UrlNormalizer.ToRelative(videoContent.GifUrl), relativeGifUrl, StringComparison.Ordinal)))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(message.ContentJson))
        {
            return true;
        }

        return !message.ContentJson.Contains(relativeGifUrl, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyVideoGifSnapshot(Message message, Guid gifBlobId, string relativeGifUrl)
    {
        var videoContent = message.GetTypedContentEntity<VideoContent>();
        if (videoContent == null)
        {
            return;
        }

        videoContent.GifBlobId = gifBlobId;
        videoContent.GifUrl = relativeGifUrl;
        if ((videoContent.ImageWidth == null || videoContent.ImageWidth <= 0) && videoContent.Width > 0)
            videoContent.ImageWidth = videoContent.Width;
        if ((videoContent.ImageHeight == null || videoContent.ImageHeight <= 0) && videoContent.Height > 0)
            videoContent.ImageHeight = videoContent.Height;

        var videoDto = ObjectMapper.Map<VideoContent, VideoContentInfo>(videoContent);
        videoDto.Url = UrlNormalizer.ToRelative(videoDto.Url);
        videoDto.SnapshotUrl = UrlNormalizer.ToRelative(videoDto.SnapshotUrl);
        videoDto.SnapshotThumbnailUrl = UrlNormalizer.ToRelative(videoDto.SnapshotThumbnailUrl);
        videoDto.GifBlobId = gifBlobId;
        videoDto.GifUrl = relativeGifUrl;
        if (videoDto.ImageWidth <= 0 && videoDto.Width > 0) videoDto.ImageWidth = videoDto.Width;
        if (videoDto.ImageHeight <= 0 && videoDto.Height > 0) videoDto.ImageHeight = videoDto.Height;

        message.SetContentJson(JsonSerializer.Serialize(videoDto));
    }
}
