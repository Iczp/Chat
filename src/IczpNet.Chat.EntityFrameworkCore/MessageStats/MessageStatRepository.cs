using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.MessageStats;

public class MessageStatRepository(
    IDbContextProvider<ChatDbContext> dbContextProvider,
    IDataBucket dataBucket) : ChatRepositoryBase<MessageStat, Guid>(dbContextProvider), IMessageStatRepository
{
    public IDataBucket DataBucket { get; } = dataBucket;

    private static bool IsUniqueConflict(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
    }

    public virtual async Task IncrementAsync(Guid sessionId, MessageTypes messageType, string dateBucketFormat = "yyyyMMdd")
    {
        var context = await GetDbContextAsync();
        var now = Clock.Now;
        var dateBucket = DataBucket.Create(now, dateBucketFormat);

        //await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        async Task<long> UpdateAsync()
        {
            return await context.MessageStat
                .Where(x =>
                    x.SessionId == sessionId &&
                    x.MessageType == messageType &&
                    x.DateBucket == dateBucket)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.Count, b => b.Count + 1)
                    .SetProperty(b => b.LastModificationTime, b => now)
                );

        }
        async Task InsertAsync()
        {
            await context.MessageStat.AddAsync(new MessageStat(GuidGenerator.Create())
            {
                SessionId = sessionId,
                MessageType = messageType,
                DateBucket = dateBucket,
                Count = 1,
                CreationTime = now
            });
        }

        try
        {
            var rows = await UpdateAsync();

            if (rows == 0)
            {
                await InsertAsync();
            }

        }
        catch (DbUpdateException ex) when (IsUniqueConflict(ex))
        {
            // 并发下别人已经插入成功，再更新一次（仍然在 Serializable 中）
            await UpdateAsync();

        }
    }

}
