using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories
{
    public class SessionUnitRepository : ChatRepositoryBase<SessionUnit, Guid>, ISessionUnitRepository
    {
        public SessionUnitRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

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

        public virtual Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId)
        {
            return BatchUpdateByEf7Async(sessionId, lastMessageId);
        }

        protected virtual async Task<int> BatchUpdateBySqlAsync(Guid sessionId, long lastMessageId)
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
            return await context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        protected virtual async Task<int> BatchUpdateByEf7Async(Guid sessionId, long lastMessageId)
        {
            var context = await GetDbContextAsync();

            ////EF7.0  https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew
            return await context.SessionUnit
                .Where(x => x.SessionId == sessionId && !x.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.LastMessageId, b => lastMessageId)
                    .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                );
        }
    }
}
