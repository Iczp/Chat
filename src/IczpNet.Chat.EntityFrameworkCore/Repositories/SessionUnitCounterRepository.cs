using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionUnitCounters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class SessionUnitCounterRepository : EfCoreRepository<ChatDbContext, SessionUnitCounter>, ISessionUnitCounterRepository
{


    public SessionUnitCounterRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
    {
    }

    private async Task<IQueryable<SessionUnitCounter>> GetQueryableAsync(Guid sessionId, DateTime? messageCreationTime)
    {
        var context = await GetDbContextAsync();

        var creationTime = messageCreationTime ?? DateTime.Now;

        Expression<Func<SessionUnitCounter, bool>> predicate = x =>
            x.SessionUnit.SessionId == sessionId &&
            !x.SessionUnit.IsDeleted &&
            !x.SessionUnit.Setting.IsKilled &&
            x.SessionUnit.Setting.IsEnabled &&
            (x.SessionUnit.Setting.HistoryFristTime == null || creationTime > x.SessionUnit.Setting.HistoryFristTime) &&
            (x.SessionUnit.Setting.HistoryLastTime == null || creationTime < x.SessionUnit.Setting.HistoryLastTime) &&
            (x.SessionUnit.Setting.ClearTime == null || creationTime > x.SessionUnit.Setting.ClearTime);

        return context.SessionUnitCounter.Where(predicate);
    }

    public virtual async Task<int> IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, bool isRemindAll)
    {
        var query = (await GetQueryableAsync(sessionId, messageCreationTime));

        await UpdateSenderLastMessageIdAsync(query, senderSessionUnitId, lastMessageId);

        if (isRemindAll)
        {
            return await query.Where(x => x.SessionUnitId != senderSessionUnitId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                    .SetProperty(b => b.PublicBadge, b => b.PublicBadge + 1)
                    .SetProperty(b => b.LastMessageId, b => lastMessageId)
                    .SetProperty(b => b.RemindAllCount, b => b.RemindAllCount + 1)
            );
        }

        return await query.Where(x => x.SessionUnitId != senderSessionUnitId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                .SetProperty(b => b.PublicBadge, b => b.PublicBadge + 1)
                .SetProperty(b => b.LastMessageId, b => lastMessageId)
            );
    }

    private static async Task<int> UpdateSenderLastMessageIdAsync(IQueryable<SessionUnitCounter> query, Guid senderSessionUnitId, long lastMessageId)
    {
        //
        return await query
             .Where(x => x.SessionUnitId == senderSessionUnitId)
             .Where(x => x.LastMessageId != lastMessageId)
             .ExecuteUpdateAsync(s => s
                 .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                 .SetProperty(b => b.LastMessageId, b => lastMessageId)
             );
    }

    public virtual async Task<int> IncrementPrivateBadgeAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, List<Guid> destinationSessionUnitIdList)
    {
        var query = await GetQueryableAsync(sessionId, messageCreationTime);

        await UpdateSenderLastMessageIdAsync(query, senderSessionUnitId, lastMessageId);

        return await query
            .Where(x => destinationSessionUnitIdList.Contains(x.SessionUnitId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                .SetProperty(b => b.PrivateBadge, b => b.PrivateBadge + 1)
                .SetProperty(b => b.LastMessageId, b => lastMessageId)
            );
    }

    public virtual async Task<int> IncrementRemindMeCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList)
    {
        var query = await GetQueryableAsync(sessionId, messageCreationTime);

        return await query
            .Where(x => destinationSessionUnitIdList.Contains(x.SessionUnitId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.RemindMeCount, b => b.RemindMeCount + 1)
            );
    }

    public virtual async Task<int> IncrementFollowingCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList)
    {
        var query = await GetQueryableAsync(sessionId, messageCreationTime);

        return await query
            .Where(x => destinationSessionUnitIdList.Contains(x.SessionUnitId))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.FollowingCount, b => b.FollowingCount + 1)
            );
    }


}
