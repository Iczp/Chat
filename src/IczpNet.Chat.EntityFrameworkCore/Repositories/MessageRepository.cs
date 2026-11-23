using IczpNet.Chat.Blobs;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.EntityFrameworkCore;
using IczpNet.Chat.Specifications;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Guids;

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

        if (markAsBound)
        {
            await db.Blob
                .Where(x => distinctBlobIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.Status, BlobStatus.Bound)
                    .SetProperty(b => b.ExpireTime, (DateTime?)null)
                    .SetProperty(b => b.RefCount, b => b.RefCount + 1)
                    .SetProperty(b => b.LastModificationTime, time)
                    .SetProperty(b => b.LastModifierId, creatorId),
                    cancellationToken);
        }
        else
        {
            await db.Blob
                .Where(x => distinctBlobIds.Contains(x.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.RefCount, b => b.RefCount + 1)
                    .SetProperty(b => b.LastModificationTime, time)
                    .SetProperty(b => b.LastModifierId, creatorId),
                    cancellationToken);
        }

        var refs = distinctBlobIds
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
        var db = await GetDbContextAsync();
        const string extraProperties = "{}";
        const bool isDeleted = false;

        foreach (var item in refs.DistinctBy(x => (x.BlobId, x.MessageId)))
        {
            var id = guidGenerator.Create();
            var concurrencyStamp = Guid.NewGuid().ToString("N");
            var blobId = item.BlobId;
            var msgId = item.MessageId;
            var sessionId = item.SessionId;
            var sourceType = (int)item.SourceType;

            await db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO Chat_BlobMessageRef
    (Id, BlobId, MessageId, SessionId, SourceType, ExtraProperties, ConcurrencyStamp, CreationTime, CreatorId, IsDeleted)
VALUES
    ({id}, {blobId}, {msgId}, {sessionId}, {sourceType}, {extraProperties}, {concurrencyStamp}, {time}, {creatorId}, {isDeleted})",
                cancellationToken);
        }
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
