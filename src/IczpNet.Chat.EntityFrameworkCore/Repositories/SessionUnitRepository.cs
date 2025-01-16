using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionUnits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories;

public class SessionUnitRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<SessionUnit, Guid>(dbContextProvider), ISessionUnitRepository
{
    protected virtual Task<string> GetTableNameForEntityAsync(ChatDbContext context, Type typeofEntity)
    {
        var entityType = context.Model.FindEntityType(typeof(SessionUnit));
        var schema = entityType.GetSchema();
        var tableName = entityType.GetTableName();
        if (string.IsNullOrWhiteSpace(tableName))
        {
            tableName = "dbo";
        }
        var table = $"{schema}.{tableName}";
        return Task.FromResult(table);
    }

    private static Expression<Func<SessionUnit, bool>> GetSessionUnitPredicate(DateTime messageCreationTime, Guid? sessionId = null)
    {
        var express = SessionUnit.GetActivePredicate(messageCreationTime);

        if (sessionId.HasValue)
        {
            express = express.And(x => x.SessionId == sessionId);
        }
        return express;
    }

    private async Task<IQueryable<SessionUnit>> GetQueryableAsync(DateTime messageCreationTime, Guid? sessionId = null)
    {
        var context = await GetDbContextAsync();

        var predicate = GetSessionUnitPredicate(messageCreationTime, sessionId);

        return context.SessionUnit.Where(predicate);
    }

    protected virtual Task<int> BatchUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
    {
        return BatchUpdateLastMessageIdByEf7Async(sessionId, lastMessageId);
    }

    protected virtual async Task<int> BatchUpdateLastMessageIdBySqlAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
    {
        var context = await GetDbContextAsync();

        var table = await GetTableNameForEntityAsync(context, typeof(SessionUnit));

        var sql = @$"Update {table} set {nameof(SessionUnit.LastMessageId)}=@LastMessageId where {nameof(SessionUnit.SessionId)}=@SessionId and [{nameof(SessionUnit.IsDeleted)}]=@IsDeleted and {nameof(SessionUnit.LastMessageId)}<@LastMessageId";

        var parameters = new List<SqlParameter>()
        {
            new SqlParameter("@LastMessageId", lastMessageId),
            new SqlParameter("@SessionId", sessionId),
            new SqlParameter("@IsDeleted", false),
        };

        if (sessionUnitIdList.IsAny())
        {
            sql += " and id in(@SessionUnitIdList)";
            parameters.Add(new SqlParameter("@SessionUnitIdList", sessionUnitIdList));
        }

        return await context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    protected virtual async Task<int> BatchUpdateLastMessageIdByEf7Async(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
    {
        var context = await GetDbContextAsync();

        ////EF7.0  https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew
        return await context.SessionUnit
            .Where(x => x.SessionId == sessionId && !x.IsDeleted)
            .WhereIf(sessionUnitIdList.IsAny(), x => sessionUnitIdList.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastMessageId, b => lastMessageId)
            //.SetProperty(b => b.LastModificationTime, b => DateTime.Now)
            );
    }

    protected virtual async Task<int> UpdateSenderLastMessageIdAsync(IQueryable<SessionUnit> query, Guid senderSessionUnitId, long lastMessageId)
    {
        //
        return await query
             .Where(x => x.Id == senderSessionUnitId)
             .Where(x => x.LastMessageId == null || x.LastMessageId < lastMessageId)
             .ExecuteUpdateAsync(s => s
                 .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                 .SetProperty(b => b.LastMessageId, b => lastMessageId)
             );
    }

    public virtual async Task<int> UpdateLastMessageIdAsync(Guid senderSessionUnitId, long lastMessageId)
    {
        var query = await GetQueryableAsync(DateTime.Now, null);

        return await UpdateSenderLastMessageIdAsync(query, senderSessionUnitId, lastMessageId);
    }

    public virtual async Task<int> IncrementPublicBadgeAndRemindAllCountAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, bool isRemindAll)
    {
        var query = (await GetQueryableAsync(messageCreationTime, sessionId));

        await UpdateSenderLastMessageIdAsync(query, senderSessionUnitId, lastMessageId);

        query = query.Where(x => x.Id != senderSessionUnitId);

        var ticks = DateTime.Now.Ticks;

        if (isRemindAll)
        {
            return await query
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.PublicBadge, b => b.PublicBadge + 1)
                    .SetProperty(b => b.LastMessageId, b => lastMessageId)
                    .SetProperty(b => b.Ticks, b => ticks)
                    .SetProperty(b => b.RemindAllCount, b => b.RemindAllCount + 1)
                );
        }

        return await query
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.PublicBadge, b => b.PublicBadge + 1)
                .SetProperty(b => b.LastMessageId, b => lastMessageId)
                .SetProperty(b => b.Ticks, b => ticks)
            );
    }

    public async Task<int> IncrementPrivateBadgeAndUpdateLastMessageIdAsync(Guid sessionId, long lastMessageId, DateTime messageCreationTime, Guid senderSessionUnitId, List<Guid> destinationSessionUnitIdList)
    {
        var query = await GetQueryableAsync(messageCreationTime, sessionId);

        await UpdateSenderLastMessageIdAsync(query, senderSessionUnitId, lastMessageId);

        query = query.Where(x => x.Id != senderSessionUnitId);

        var ticks = DateTime.Now.Ticks;

        return await query
            .Where(x => destinationSessionUnitIdList.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                .SetProperty(b => b.PublicBadge, b => b.PublicBadge + 1)
                .SetProperty(b => b.PrivateBadge, b => b.PrivateBadge + 1)
                .SetProperty(b => b.LastMessageId, b => lastMessageId)
                .SetProperty(b => b.Ticks, b => ticks)
            );
    }

    public virtual async Task<int> IncrementRemindMeCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList)
    {
        var query = await GetQueryableAsync(messageCreationTime, sessionId);

        return await query
            .Where(x => destinationSessionUnitIdList.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.RemindMeCount, b => b.RemindMeCount + 1)
            );
    }


    public virtual async Task<int> IncrementFollowingCountAsync(Guid sessionId, DateTime messageCreationTime, List<Guid> destinationSessionUnitIdList)
    {
        var query = await GetQueryableAsync(messageCreationTime, sessionId);

        return await query
            .Where(x => destinationSessionUnitIdList.Contains(x.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.FollowingCount, b => b.FollowingCount + 1)
            );
    }

    //public virtual async Task<int> BatchUpdateNameAsync(long chatObjectId, string name, string nameSpelling, string nameSpellingAbbreviation)
    //{
    //    var query = await GetQueryableAsync(DateTime.Now);

    //    var a = await query
    //         .Where(x => x.SessionUnitId == chatObjectId)
    //         .ExecuteUpdateAsync(s => s
    //             .SetProperty(b => b.OwnerName, b => name)
    //             .SetProperty(b => b.OwnerNameSpellingAbbreviation, b => nameSpellingAbbreviation)
    //         );
    //    var b = await query
    //         .Where(x => x.DestinationId == chatObjectId)
    //         .ExecuteUpdateAsync(s => s
    //             .SetProperty(b => b.DestinationName, b => name)
    //             .SetProperty(b => b.DestinationNameSpellingAbbreviation, b => nameSpellingAbbreviation)
    //         );
    //    return a + b;
    //}

    public async Task<int> BatchUpdateAppUserIdAsync(long chatObjectId, Guid appUserId)
    {
        var query = await GetQueryableAsync(DateTime.Now, sessionId: null);

        return await query
            .Where(x => x.OwnerId == chatObjectId)
            .ExecuteUpdateAsync(s => s
                 .SetProperty(b => b.AppUserId, b => appUserId)
             );
    }


}
