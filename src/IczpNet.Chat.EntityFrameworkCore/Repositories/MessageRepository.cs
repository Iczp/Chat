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

namespace IczpNet.Chat.Repositories;

public class MessageRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<Message, long>(dbContextProvider), IMessageRepository
{
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
