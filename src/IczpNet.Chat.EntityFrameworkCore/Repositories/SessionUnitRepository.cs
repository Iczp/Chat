﻿using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageAutoId)
        {
            var context = await GetDbContextAsync();

            var table = await GetTableNameForEntityAsync(context, typeof(SessionUnit));

            var sql = @$"Update {table} set {nameof(SessionUnit.LastMessageAutoId)}=@LastMessageAutoId where {nameof(SessionUnit.SessionId)}=@SessionId and [{nameof(SessionUnit.IsDeleted)}]=@IsDeleted and {nameof(SessionUnit.LastMessageAutoId)}<@LastMessageAutoId";

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@LastMessageAutoId", lastMessageAutoId),
                new SqlParameter("@SessionId", sessionId),
                new SqlParameter("@IsDeleted", false),
            };

            ////EF7.0  https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew
            //await context.SessionUnit
            //    .Where(x => x.SessionId == sessionId && !x.IsDeleted)
            //    .ExecuteUpdateAsync(s => s
            //        .SetProperty(b => b.LastMessageAutoId, b => lastMessageAutoId)
            //        .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
            //    );

            return await context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}