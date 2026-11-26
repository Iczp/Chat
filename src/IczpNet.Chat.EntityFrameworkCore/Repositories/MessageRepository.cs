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
        var rootVisible = await query.AnyAsync(
            x => x.Id == targetMessageId && !x.IsRollbacked && !x.RollbackTime.HasValue,
            cancellationToken);
        if (!rootVisible)
        {
            return false;
        }

        if (childMessageId.HasValue && childMessageId.Value != targetMessageId)
        {
            var context = await GetDbContextAsync();
            var childAlive = await context.Message.AsNoTracking().AnyAsync(
                x => x.Id == childMessageId.Value && !x.IsPrivate && !x.IsRollbacked && !x.RollbackTime.HasValue,
                cancellationToken);
            if (!childAlive)
            {
                return false;
            }
        }

        return true;
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
    public virtual async Task BindBlobsToMessageAsync(
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
            return;
        }

        var distinctBlobIds = blobIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (distinctBlobIds.Count == 0)
        {
            return;
        }

        var time = now ?? Clock.Now;
        var db = await GetDbContextAsync();

        var existingBlobIds = await db.BlobMessageRef
            .Where(x => x.MessageId == messageId && distinctBlobIds.Contains(x.BlobId))
            .Select(x => x.BlobId)
            .ToListAsync(cancellationToken);

        var newBlobIds = distinctBlobIds.Except(existingBlobIds).ToList();

        if (markAsBound)
        {
            await db.Blob
                .Where(x => distinctBlobIds.Contains(x.Id) && (x.Status != BlobStatus.Bound || x.ExpireTime != null))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.Status, BlobStatus.Bound)
                    .SetProperty(b => b.ExpireTime, (DateTime?)null)
                    .SetProperty(b => b.LastModificationTime, time)
                    .SetProperty(b => b.LastModifierId, creatorId),
                    cancellationToken);
        }

        if (newBlobIds.Count == 0)
        {
            return;
        }

        await db.Blob
            .Where(x => newBlobIds.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.RefCount, b => b.RefCount + 1)
                .SetProperty(b => b.LastModificationTime, time)
                .SetProperty(b => b.LastModifierId, creatorId),
                cancellationToken);

        var refs = newBlobIds
            .Select(blobId => (blobId, messageId, sessionId, sourceType))
            .ToList();
        await InsertBlobMessageRefsAsync(refs, creatorId, time, cancellationToken);
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
