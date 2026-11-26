using IczpNet.Chat.Articles;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using Microsoft.EntityFrameworkCore;
using IczpNet.Chat.Specifications;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Repositories;

public class MessageRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<Message, long>(dbContextProvider), IMessageRepository
{
    public async Task<SessionMessageWindowCacheItem> GetReadWindowAsync(Guid sessionId, long minMessageId,
        long maxMessageId, int batchSize, bool isDescending, CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync();
        var query = context.Message.AsNoTracking().Where(x => x.SessionId == sessionId &&
            x.Id > minMessageId && x.Id < maxMessageId);
        query = isDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
        var items = await query.Select(x => new SessionMessageWindowCacheItem.Entry
        {
            Id = x.Id, IsPrivate = x.IsPrivate, SenderSessionUnitId = x.SenderSessionUnitId,
            ReceiverSessionUnitId = x.ReceiverSessionUnitId, CreationTime = x.CreationTime
        }).Take(batchSize + 1).ToListAsync(cancellationToken);
        var result = new SessionMessageWindowCacheItem { MinMessageId = minMessageId, MaxMessageId = maxMessageId };
        if (items.Count > batchSize)
        {
            if (isDescending) result.MinMessageId = items[^1].Id;
            else result.MaxMessageId = items[^1].Id;
            items.RemoveAt(items.Count - 1);
        }
        result.Items = items.OrderBy(x => x.Id).ToList();
        return result;
    }

    private async Task<IQueryable<Message>> GetVisibleQueryAsync(Guid sessionUnitId, CancellationToken cancellationToken)
    {
        var context = await GetDbContextAsync();
        var unit = await context.SessionUnit.AsNoTracking()
            .SingleAsync(x => x.Id == sessionUnitId, cancellationToken);
        return context.Message.AsNoTracking()
            .Where(x => x.SessionId == unit.SessionId)
            .Where(new OwnerMessageSpecification(unit.OwnerId, sessionUnitId).ToExpression());
    }

    public async Task<List<long>> GetVisibleIdsAsync(Guid sessionUnitId, long? minMessageId, long? maxMessageId,
        int maxResultCount, bool isDescending, CancellationToken cancellationToken = default)
    {
        var query = await GetVisibleQueryAsync(sessionUnitId, cancellationToken);
        if (minMessageId.HasValue) query = query.Where(x => x.Id > minMessageId.Value);
        if (maxMessageId.HasValue) query = query.Where(x => x.Id < maxMessageId.Value);
        query = isDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
        return await query.Select(x => x.Id).Take(maxResultCount).ToListAsync(cancellationToken);
    }

    public async Task<long> CountVisibleAsync(Guid sessionUnitId, long? minMessageId, long? maxMessageId,
        CancellationToken cancellationToken = default)
    {
        var query = await GetVisibleQueryAsync(sessionUnitId, cancellationToken);
        if (minMessageId.HasValue) query = query.Where(x => x.Id > minMessageId.Value);
        if (maxMessageId.HasValue) query = query.Where(x => x.Id < maxMessageId.Value);
        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<bool> CanReadAsync(Guid sessionUnitId, long messageId, bool includeReferences = false,
        CancellationToken cancellationToken = default)
    {
        var query = await GetVisibleQueryAsync(sessionUnitId, cancellationToken);
        if (await query.AnyAsync(x => x.Id == messageId, cancellationToken)) return true;
        if (!includeReferences) return false;
        var context = await GetDbContextAsync();
        if (!await context.Message.AnyAsync(x => x.Id == messageId && !x.IsPrivate && !x.IsRollbacked, cancellationToken)) return false;
        return await query.AnyAsync(x => x.QuoteMessageId == messageId ||
            x.HistoryContentList.Any(c => c.HistoryMessageList.Any(h => h.MessageId == messageId)), cancellationToken);
    }

    public async Task<bool> CanReadAttachmentAsync(Guid sessionUnitId, long targetMessageId, long? childMessageId = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetVisibleQueryAsync(sessionUnitId, cancellationToken);
        var rootSessionId = await query
            .Where(x => x.Id == targetMessageId && !x.IsRollbacked && !x.RollbackTime.HasValue)
            .Select(x => x.SessionId)
            .FirstOrDefaultAsync(cancellationToken);
        if (rootSessionId == null)
        {
            return false;
        }

        if (!childMessageId.HasValue || childMessageId.Value == targetMessageId)
        {
            return true;
        }

        var context = await GetDbContextAsync();
        var childAlive = await context.Message.AsNoTracking().AnyAsync(
            x => x.Id == childMessageId.Value && !x.IsPrivate && !x.IsRollbacked && !x.RollbackTime.HasValue,
            cancellationToken);
        if (!childAlive)
        {
            return false;
        }

        // Verify childMessageId is genuinely reachable from targetMessageId within 1..5 hops
        // via HistoryContent -> HistoryMessage or same-session QuoteMessageId.
        var visited = new HashSet<long> { targetMessageId };
        var frontier = new List<long> { targetMessageId };
        for (int depth = 1; depth <= 5 && frontier.Count > 0 && visited.Count < 500; depth++)
        {
            var historyChildren = await context.Message.AsNoTracking()
                .Where(m => frontier.Contains(m.Id) && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .SelectMany(m => m.HistoryContentList.SelectMany(c => c.HistoryMessageList.Select(h => h.MessageId)))
                .Distinct()
                .ToListAsync(cancellationToken);

            var quoteChildren = await context.Message.AsNoTracking()
                .Where(m => frontier.Contains(m.Id) && m.QuoteMessageId.HasValue && m.SessionId == rootSessionId && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .Select(m => m.QuoteMessageId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            var candidateIds = historyChildren
                .Concat(quoteChildren)
                .Where(id => !visited.Contains(id))
                .Distinct()
                .Take(200)
                .ToList();

            if (candidateIds.Count == 0)
            {
                break;
            }

            var aliveCandidateIds = await context.Message.AsNoTracking()
                .Where(m => candidateIds.Contains(m.Id) && !m.IsPrivate && !m.IsRollbacked && !m.RollbackTime.HasValue)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);

            var nextFrontier = new List<long>();
            foreach (var cid in aliveCandidateIds)
            {
                if (cid == childMessageId.Value)
                {
                    return true;
                }

                if (visited.Add(cid))
                {
                    nextFrontier.Add(cid);
                }
            }

            frontier = nextFrontier;
        }

        return false;
    }

    public virtual async Task<bool> HasVisibleBlobForOwnersAsync(
        Guid blobId,
        IReadOnlyCollection<long> ownerIds,
        CancellationToken cancellationToken = default)
    {
        if (blobId == Guid.Empty || ownerIds == null || ownerIds.Count == 0)
        {
            return false;
        }

        var context = await GetDbContextAsync();
        var owners = ownerIds.Distinct().ToList();

        // DB-side EXISTS check combining BlobMessageRef -> Message -> active SessionUnit visibility + DeletedRecorder.
        var hasBoundRef = await context.BlobMessageRef.AsNoTracking()
            .Where(r => r.BlobId == blobId && r.SessionId.HasValue)
            .Join(
                context.Message.AsNoTracking().Where(m => m.SessionId.HasValue && !m.IsRollbacked && !m.RollbackTime.HasValue),
                r => r.MessageId,
                m => m.Id,
                (r, m) => m)
            .Where(m => context.SessionUnit.Any(u =>
                u.SessionId == m.SessionId &&
                owners.Contains(u.OwnerId) &&
                !u.Setting.IsKilled &&
                (!m.IsPrivate || m.SenderSessionUnitId == u.Id || m.ReceiverSessionUnitId == u.Id) &&
                (!u.Setting.StartMessageId.HasValue || m.Id > u.Setting.StartMessageId.Value) &&
                (!u.Setting.EndMessageId.HasValue || m.Id <= u.Setting.EndMessageId.Value) &&
                (!u.Setting.HistoryFristTime.HasValue || m.CreationTime >= u.Setting.HistoryFristTime.Value) &&
                (!u.Setting.HistoryLastTime.HasValue || m.CreationTime < u.Setting.HistoryLastTime.Value) &&
                (!u.Setting.ClearTime.HasValue || m.CreationTime > u.Setting.ClearTime.Value) &&
                !context.DeletedRecorder.Any(d => d.SessionUnitId == u.Id && d.MessageId == m.Id)))
            .AnyAsync(cancellationToken);

        if (hasBoundRef)
        {
            return true;
        }

        static IQueryable<Message> FilterVisibleForOwners(
            IQueryable<Message> query,
            ChatDbContext db,
            List<long> ownerList)
            => query.Where(m =>
                m.SessionId.HasValue &&
                !m.IsRollbacked &&
                !m.RollbackTime.HasValue &&
                db.SessionUnit.Any(u =>
                    u.SessionId == m.SessionId &&
                    ownerList.Contains(u.OwnerId) &&
                    !u.Setting.IsKilled &&
                    (!m.IsPrivate || m.SenderSessionUnitId == u.Id || m.ReceiverSessionUnitId == u.Id) &&
                    (!u.Setting.StartMessageId.HasValue || m.Id > u.Setting.StartMessageId.Value) &&
                    (!u.Setting.EndMessageId.HasValue || m.Id <= u.Setting.EndMessageId.Value) &&
                    (!u.Setting.HistoryFristTime.HasValue || m.CreationTime >= u.Setting.HistoryFristTime.Value) &&
                    (!u.Setting.HistoryLastTime.HasValue || m.CreationTime < u.Setting.HistoryLastTime.Value) &&
                    (!u.Setting.ClearTime.HasValue || m.CreationTime > u.Setting.ClearTime.Value) &&
                    !db.DeletedRecorder.Any(d => d.SessionUnitId == u.Id && d.MessageId == m.Id)));

        if (await FilterVisibleForOwners(
                context.ImageMessage.AsNoTracking()
                    .Where(x => x.BlobId == blobId || x.ThumbnailBlobId == blobId || x.OriginalBlobId == blobId)
                    .SelectMany(x => x.MessageList),
                context,
                owners).AnyAsync(cancellationToken))
        {
            return true;
        }

        if (await FilterVisibleForOwners(
                context.VideoContent.AsNoTracking()
                    .Where(x => x.BlobId == blobId || x.SnapshotBlobId == blobId || x.SnapshotThumbnailBlobId == blobId || x.GifBlobId == blobId)
                    .SelectMany(x => x.MessageList),
                context,
                owners).AnyAsync(cancellationToken))
        {
            return true;
        }

        if (await FilterVisibleForOwners(
                context.SoundContent.AsNoTracking()
                    .Where(x => x.BlobId == blobId)
                    .SelectMany(x => x.MessageList),
                context,
                owners).AnyAsync(cancellationToken))
        {
            return true;
        }

        return await FilterVisibleForOwners(
            context.FileContent.AsNoTracking()
                .Where(x => x.BlobId == blobId)
                .SelectMany(x => x.MessageList),
            context,
            owners).AnyAsync(cancellationToken);
    }

    public async Task<bool> TrySetVideoGifAsync(Guid videoContentId, Guid gifBlobId, string gifUrl,
        CancellationToken cancellationToken = default)
    {
        var context = await GetDbContextAsync();
        var affected = await context.VideoContent
            .Where(x => x.Id == videoContentId && x.GifBlobId == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(v => v.GifBlobId, gifBlobId)
                .SetProperty(v => v.GifUrl, gifUrl),
                cancellationToken);
        return affected > 0;
    }
    /// <inheritdoc />
    public virtual async Task<int> IncrementReadedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.ReadedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

    /// <inheritdoc />
    public virtual async Task<int> IncrementOpenedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.OpenedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

    /// <inheritdoc />
    public virtual async Task<int> IncrementFavoritedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.FavoritedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

    /// <inheritdoc />
    public async Task<int> IncrementDeletedCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.DeletedCounter
            .Where(x => messageIdList.Contains(x.MessageId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Count, b => b.Count + 1)
            );
    }

    /// <inheritdoc />
    public async Task<int> IncrementQuoteCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.Message
            .Where(x => messageIdList.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.QuoteCount, b => b.QuoteCount + 1)
            );
    }

    /// <inheritdoc />
    public async Task<int> IncrementForwardCountAsync(List<long> messageIdList)
    {
        var context = await GetDbContextAsync();

        return await context.Message
            .Where(x => messageIdList.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.ForwardCount, b => b.ForwardCount + 1)
            );
    }

    /// <inheritdoc />
    public virtual async Task<List<Blob>> GetBlobsAsNoTrackingAsync(
        IReadOnlyCollection<Guid> blobIds,
        CancellationToken cancellationToken = default)
    {
        if (blobIds == null || blobIds.Count == 0)
        {
            return [];
        }

        var distinctIds = blobIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return [];
        }

        var db = await GetDbContextAsync();
        return await db.Blob
            .AsNoTracking()
            .Where(x => distinctIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> BindBlobsToMessageAsync(
        long messageId,
        Guid? sessionId,
        IReadOnlyCollection<Guid> blobIds,
        BlobRefSourceTypes sourceType,
        Guid? creatorId = null,
        DateTime? now = null,
        bool markAsBound = true,
        CancellationToken cancellationToken = default)
    {
        if (blobIds == null || blobIds.Count == 0)
        {
            return 0;
        }

        var distinctBlobIds = blobIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (distinctBlobIds.Count == 0)
        {
            return 0;
        }

        var time = now ?? Clock.Now;
        var db = await GetDbContextAsync();

        if (markAsBound)
        {
            // Conditional claim: only already-Bound or still-valid unexpired Temp blobs can be claimed for sending.
            await db.Blob
                .Where(x => distinctBlobIds.Contains(x.Id) &&
                    (x.Status == BlobStatus.Bound ||
                     (x.Status == BlobStatus.Temp && (!x.ExpireTime.HasValue || x.ExpireTime.Value >= time))) &&
                    (x.Status != BlobStatus.Bound || x.ExpireTime != null))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.Status, BlobStatus.Bound)
                    .SetProperty(b => b.ExpireTime, (DateTime?)null)
                    .SetProperty(b => b.LastModificationTime, time)
                    .SetProperty(b => b.LastModifierId, creatorId),
                    cancellationToken);

            var boundCount = await db.Blob.CountAsync(
                x => distinctBlobIds.Contains(x.Id) && x.Status == BlobStatus.Bound,
                cancellationToken);
            if (boundCount < distinctBlobIds.Count)
            {
                throw new Volo.Abp.BusinessException(ChatErrorCodes.Blobs.AttachmentNotFoundOrForbidden);
            }
        }

        var existingBlobIds = await db.BlobMessageRef
            .Where(x => x.MessageId == messageId && distinctBlobIds.Contains(x.BlobId))
            .Select(x => x.BlobId)
            .ToListAsync(cancellationToken);

        var newBlobIds = distinctBlobIds.Except(existingBlobIds).ToList();
        if (newBlobIds.Count == 0)
        {
            return 0;
        }

        var insertedBlobIds = await TryInsertNewRefsIdempotentlyAsync(
            db, messageId, sessionId, newBlobIds, sourceType, creatorId, time, cancellationToken);
        if (insertedBlobIds.Count == 0)
        {
            return 0;
        }

        await db.Blob
            .Where(x => insertedBlobIds.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.RefCount, b => b.RefCount + 1)
                .SetProperty(b => b.LastModificationTime, time)
                .SetProperty(b => b.LastModifierId, creatorId),
                cancellationToken);

        return insertedBlobIds.Count;
    }

    private async Task<List<Guid>> TryInsertNewRefsIdempotentlyAsync(
        ChatDbContext db,
        long messageId,
        Guid? sessionId,
        List<Guid> candidateBlobIds,
        BlobRefSourceTypes sourceType,
        Guid? creatorId,
        DateTime time,
        CancellationToken cancellationToken)
    {
        try
        {
            var refs = candidateBlobIds
                .Select(blobId => (blobId, messageId, sessionId, sourceType))
                .ToList();
            await InsertBlobMessageRefsAsync(refs, creatorId, time, cancellationToken);
            return candidateBlobIds;
        }
        catch (DbUpdateException)
        {
            // Concurrent writer won unique index (BlobId, MessageId): re-check and insert only remaining missing ones.
            var alreadyInserted = await db.BlobMessageRef
                .AsNoTracking()
                .Where(x => x.MessageId == messageId && candidateBlobIds.Contains(x.BlobId))
                .Select(x => x.BlobId)
                .ToListAsync(cancellationToken);

            var remaining = candidateBlobIds.Except(alreadyInserted).ToList();
            if (remaining.Count == 0)
            {
                return [];
            }

            foreach (var blobId in remaining)
            {
                try
                {
                    await InsertBlobMessageRefsAsync(
                        [(blobId, messageId, sessionId, sourceType)], creatorId, time, cancellationToken);
                }
                catch (DbUpdateException)
                {
                    // Already inserted concurrently.
                }
            }

            var finalPresent = await db.BlobMessageRef
                .AsNoTracking()
                .Where(x => x.MessageId == messageId && remaining.Contains(x.BlobId))
                .Select(x => x.BlobId)
                .ToListAsync(cancellationToken);
            return finalPresent.Except(alreadyInserted).ToList();
        }
    }

    /// <inheritdoc />
    public virtual async Task InsertBlobMessageRefsAsync(
        IReadOnlyCollection<(Guid BlobId, long MessageId, Guid? SessionId, BlobRefSourceTypes SourceType)> refs,
        Guid? creatorId = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default)
    {
        if (refs == null || refs.Count == 0)
        {
            return;
        }

        var time = now ?? Clock.Now;
        var guidGenerator = LazyServiceProvider.LazyGetRequiredService<IGuidGenerator>();
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<BlobMessageRef, Guid>>();
        var entities = refs.DistinctBy(x => (x.BlobId, x.MessageId))
            .Select(item => new BlobMessageRef(
                guidGenerator.Create(), item.BlobId, item.MessageId, item.SessionId, item.SourceType)
            {
                CreationTime = time,
                CreatorId = creatorId
            })
            .ToList();

        await repository.InsertManyAsync(entities, autoSave: true, cancellationToken: cancellationToken);
    }

    public virtual async Task<List<Message>> GetCacheMissMessagesAsync(
        IReadOnlyCollection<long> messageIds,
        CancellationToken cancellationToken = default)
    {
        if (messageIds == null || messageIds.Count == 0)
        {
            return [];
        }

        var distinctIds = messageIds.Where(x => x > 0).Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return [];
        }

        var db = await GetDbContextAsync();
        var messages = await db.Message.AsNoTracking()
            .Where(x => distinctIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        await LoadMessageContentsBatchAsync(messages, cancellationToken);
        return messages;
    }

    public virtual async Task LoadMessageContentsBatchAsync(
        IReadOnlyCollection<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (messages == null || messages.Count == 0)
        {
            return;
        }

        var db = await GetDbContextAsync();

        // 1. 批量无跟踪加载单层引用消息
        var quoteIds = messages
            .Where(x => x.QuoteMessageId.HasValue && x.QuoteMessage == null)
            .Select(x => x.QuoteMessageId!.Value)
            .Distinct()
            .ToList();

        var allTargetMessages = new List<Message>(messages);
        if (quoteIds.Count > 0)
        {
            var existingMap = messages.ToDictionary(x => x.Id);
            var missingQuoteIds = quoteIds.Where(id => !existingMap.ContainsKey(id)).ToList();
            var fetchedQuotes = missingQuoteIds.Count == 0
                ? []
                : await db.Message.AsNoTracking()
                    .Where(x => missingQuoteIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

            var quoteMap = new Dictionary<long, Message>(existingMap);
            foreach (var q in fetchedQuotes)
            {
                quoteMap[q.Id] = q;
                allTargetMessages.Add(q);
            }

            foreach (var msg in messages)
            {
                if (msg.QuoteMessageId.HasValue && msg.QuoteMessage == null &&
                    quoteMap.TryGetValue(msg.QuoteMessageId.Value, out var qMsg))
                {
                    msg.AttachQuoteMessage(qMsg);
                }
            }
        }

        // 2. 仅按本批消息实际出现的 MessageType 分组批量投影内容表（避免 13 张子表笛卡尔积 Join 与 N+1 懒加载）
        var messageById = allTargetMessages
            .Where(x => !x.IsRollbacked && !x.RollbackTime.HasValue)
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.ToList());
        if (messageById.Count == 0)
        {
            return;
        }

        var byType = allTargetMessages
            .Where(x => !x.IsRollbacked && !x.RollbackTime.HasValue && x.MessageType != MessageTypes.Template)
            .GroupBy(x => x.MessageType);

        foreach (var group in byType)
        {
            var ids = group.Select(x => x.Id).Distinct().ToList();
            if (ids.Count == 0) continue;

            switch (group.Key)
            {
                case MessageTypes.Text:
                    {
                        var rows = await db.Set<TextContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.TextContentList.Count == 0) m.TextContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Cmd:
                    {
                        var rows = await db.Set<CmdContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.CmdContentList.Count == 0) m.CmdContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Image:
                    {
                        var rows = await db.Set<ImageContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.ImageContentList.Count == 0) m.ImageContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Sound:
                    {
                        var rows = await db.Set<SoundContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.SoundContentList.Count == 0) m.SoundContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Video:
                    {
                        var rows = await db.Set<VideoContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.VideoContentList.Count == 0) m.VideoContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.File:
                    {
                        var rows = await db.Set<FileContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.FileContentList.Count == 0) m.FileContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Link:
                    {
                        var rows = await db.Set<LinkContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.LinkContentList.Count == 0) m.LinkContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Html:
                    {
                        var rows = await db.Set<HtmlContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.HtmlContentList.Count == 0) m.HtmlContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Article:
                    {
                        var rows = await db.Set<ArticleContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.ArticleContentList.Count == 0) m.ArticleContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Location:
                    {
                        var rows = await db.Set<LocationContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.LocationContentList.Count == 0) m.LocationContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.Contacts:
                    {
                        var rows = await db.Set<ContactsContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.ContactsContentList.Count == 0) m.ContactsContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.RedEnvelope:
                    {
                        var rows = await db.Set<RedEnvelopeContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.RedEnvelopeContentList.Count == 0) m.RedEnvelopeContentList.Add(r.Content);
                        break;
                    }
                case MessageTypes.History:
                    {
                        var rows = await db.Set<HistoryContent>().AsNoTracking()
                            .SelectMany(c => c.MessageList.Where(m => ids.Contains(m.Id)), (c, m) => new { MessageId = m.Id, Content = c })
                            .ToListAsync(cancellationToken);
                        foreach (var r in rows)
                            foreach (var m in messageById[r.MessageId])
                                if (m.HistoryContentList.Count == 0) m.HistoryContentList.Add(r.Content);
                        break;
                    }
            }
        }
    }



    public virtual async Task<Message> InsertForwardedMessageAsync(
        Message message,
        IContentEntity existingContent,
        CancellationToken cancellationToken = default)
    {
        var db = await GetDbContextAsync();
        if (existingContent != null)
        {
            var unproxied = Castle.DynamicProxy.ProxyUtil.GetUnproxiedInstance(existingContent);
            var entry = db.Entry(unproxied);
            if (entry.State == EntityState.Detached)
            {
                entry.State = EntityState.Unchanged;
            }
        }
        return await InsertAsync(message, autoSave: true, cancellationToken: cancellationToken);
    }

    //public virtual async Task<int> IncrementRecorderAsync(List<long> messageIdList)
    //{
    //    var context = await GetDbContextAsync();

    //    return await context.Message
    //        .Where(x => messageIdList.Contains(x.Id))
    //        .ExecuteUpdateAsync(s => s
    //            .SetProperty(b => b.FavoritedCount, b => context.FavoritedRecorder.Count(f => f.MessageId == b.Id))
    //            .SetProperty(b => b.ReadedCount, b => context.ReadedRecorder.Count(f => f.MessageId == b.Id))
    //            .SetProperty(b => b.OpenedCount, b => context.OpenedRecorder.Count(f => f.MessageId == b.Id))
    //        );
    //}
}
