using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageStats;
using IczpNet.Chat.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.MessageReports;

public abstract class MessageReportRepositoryBase<T>(
    IDbContextProvider<ChatDbContext> dbContextProvider)
    : ChatRepositoryBase<T, Guid>(dbContextProvider), IMessageReportRepositoryBase<T>
    where T : MessageReportBase, new()
{
    protected IDataBucket DataBucket => LazyServiceProvider.LazyGetRequiredService<IDataBucket>();

    private static bool IsUniqueConflict(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
    }

    /// <summary>
    /// 通用增量接口，根据 dateBucketFormat 自动选择表名
    /// </summary>
    public virtual async Task IncrementAsync(
        Guid sessionId,
        MessageTypes messageType,
        string dateBucketFormat = "yyyyMMdd")
    {
        var now = Clock.Now;
        var dateBucket = DataBucket.Create(now, dateBucketFormat);
        await IncrementSqlAsync(sessionId, messageType, dateBucket, now);
    }

    /// <summary>
    /// EF Core 增量方式（可作为回退或调试用）
    /// </summary>
    protected virtual async Task IncrementEFAsync(
        Guid sessionId,
        MessageTypes messageType,
        string dateBucketFormat = "yyyyMMdd")
    {
        var context = await GetDbContextAsync();
        var now = Clock.Now;
        var dateBucket = DataBucket.Create(now, dateBucketFormat);

        async Task<long> UpdateAsync()
        {
            return await context.Set<T>()
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
            var entity = new T()
            {
                SessionId = sessionId,
                MessageType = messageType,
                DateBucket = dateBucket,
                Count = 1,
                CreationTime = now,
                LastModificationTime = now,
            };

            entity.SetId(GuidGenerator.Create());

            context.Add(entity);
            await context.SaveChangesAsync();

            Logger.LogInformation(
                "InsertEFAsync: SessionId={SessionId},MessageType={MessageType},DateBucket={DateBucket},Id={Id}",
                sessionId, messageType, dateBucket, entity.Id);
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
            // 并发下别人已经插入成功，再更新一次
            await UpdateAsync();
        }
    }

    /// <summary>
    /// SQL MERGE 增量方式（高并发推荐）
    /// </summary>
    protected async Task IncrementSqlAsync(
        Guid sessionId,
        MessageTypes messageType,
        long dateBucket,
        DateTime now)
    {
        var context = await GetDbContextAsync();
        var dbSchema = string.IsNullOrWhiteSpace(ChatDbProperties.DbSchema) ? "dbo" : ChatDbProperties.DbSchema;
        var tableName = $"{ChatDbProperties.DbTablePrefix}_{typeof(T).Name}";

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
        LastModificationTime,
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
        @Now,
        REPLACE(NEWID(), '-', ''),
        @ExtraProperties
    );";

        await context.Database.ExecuteSqlRawAsync(
            sql,
            new SqlParameter("@Id", Guid.NewGuid()),
            new SqlParameter("@SessionId", sessionId),
            new SqlParameter("@MessageType", (int)messageType),
            new SqlParameter("@DateBucket", dateBucket),
            new SqlParameter("@Now", now),
            new SqlParameter("@ExtraProperties", "{}")
        );
    }
}
