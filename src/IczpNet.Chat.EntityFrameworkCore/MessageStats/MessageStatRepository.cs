using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Select;
using Polly;
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
        var now = Clock.Now;
        var dateBucket = DataBucket.Create(now, dateBucketFormat);
        await IncrementSqlAsync(sessionId, messageType, dateBucket, now);
    }

    protected virtual async Task IncrementEFAsync(Guid sessionId, MessageTypes messageType, string dateBucketFormat = "yyyyMMdd")
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
            var messageStat = context.MessageStat.Add(new MessageStat(GuidGenerator.Create())
            {
                SessionId = sessionId,
                MessageType = messageType,
                DateBucket = dateBucket,
                Count = 1,
                CreationTime = now
            });
            await context.SaveChangesAsync();
            Logger.LogInformation("InsertAsync: SessionId={SessionId},MessageType={MessageType},DateBucket={DateBucket},Id={Id}", sessionId, messageType, dateBucket, messageStat.Entity.Id);
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

    protected async Task IncrementSqlAsync(
    Guid sessionId,
    MessageTypes messageType,
    long dateBucket,
    DateTime now)
    {
        var context = await GetDbContextAsync();

        var dbSchema = string.IsNullOrWhiteSpace(ChatDbProperties.DbSchema) ? "dbo" : ChatDbProperties.DbSchema;
        var tableName = $"{ChatDbProperties.DbTablePrefix}_{nameof(MessageStat)}";

        var sql = $@"
MERGE [{dbSchema}].[{tableName}] WITH (HOLDLOCK) AS T
USING (
    SELECT 
        @SessionId   AS SessionId,
        @MessageType AS MessageType,
        @DateBucket  AS DateBucket
) AS S
ON  T.SessionId   = S.SessionId
AND T.MessageType = S.MessageType
AND T.DateBucket  = S.DateBucket
WHEN MATCHED THEN
    UPDATE SET
        Count = T.Count + 1,
        LastModificationTime = @Now
WHEN NOT MATCHED THEN
    INSERT (
        Id,
        SessionId,
        MessageType,
        DateBucket,
        Count,
        CreationTime,
        ConcurrencyStamp,
        ExtraProperties
    )
    VALUES (
        @Id,
        @SessionId,
        @MessageType,
        @DateBucket,
        1,
        @Now,
        REPLACE(NEWID(), '-', ''),
        @ExtraProperties
    );
";

        await context.Database.ExecuteSqlRawAsync(
            sql,
            new SqlParameter("@Id", GuidGenerator.Create()),
            new SqlParameter("@SessionId", sessionId),
            new SqlParameter("@MessageType", messageType.ToString()),
            new SqlParameter("@DateBucket", dateBucket),
            new SqlParameter("@Now", now),
            new SqlParameter("@ExtraProperties", "{}")
        );
    }

}
