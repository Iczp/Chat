using IczpNet.AbpCommons.Extensions;
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

        public virtual Task<int> BatchUpdateAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
        {
            return BatchUpdateByEf7Async(sessionId, lastMessageId);
        }

        protected virtual async Task<int> BatchUpdateBySqlAsync(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
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

        protected virtual async Task<int> BatchUpdateByEf7Async(Guid sessionId, long lastMessageId, List<Guid> sessionUnitIdList = null)
        {
            var context = await GetDbContextAsync();

            ////EF7.0  https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew
            return await context.SessionUnit
                .Where(x => x.SessionId == sessionId && !x.IsDeleted && x.ServiceStatus == Enums.ServiceStatus.Normal)
                .WhereIf(sessionUnitIdList.IsAny(), x => sessionUnitIdList.Contains(x.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.LastMessageId, b => lastMessageId)
                    .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                );
        }

        public virtual async Task<Dictionary<Guid, SessionUnitStatModel>> GetStatsAsync(List<Guid> sessionUnitIdList, long minMessageId = 0, bool? isImmersed = null)
        {
            var context = await GetDbContextAsync();

            return context.SessionUnit
                .Where(x => sessionUnitIdList.Contains(x.Id))
                .WhereIf(isImmersed.HasValue, x => x.IsImmersed == isImmersed)
                .Where(x => sessionUnitIdList.Contains(x.Id))
                //.Select(x => new
                //{
                //    x.Id,
                //    x.OwnerId,
                //    //x.OwnerFollowList,
                //    Messages = context.Message.Where(d =>
                //        x.SessionId == d.SessionId &&
                //        d.Id > minMessageId &&
                //        d.SenderId != x.OwnerId &&
                //        (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                //        (x.HistoryFristTime == null || d.CreationTime > x.HistoryFristTime) &&
                //        (x.HistoryLastTime == null || d.CreationTime < x.HistoryLastTime) &&
                //        (x.ClearTime == null || d.CreationTime > x.ClearTime))
                //})
                .Select(x => new SessionUnitStatModel
                {
                    Id = x.Id,
                    PublicBadge = context.Message.Where(d =>
                        x.SessionId == d.SessionId &&
                        d.Id > minMessageId &&
                        d.SenderId != x.OwnerId &&
                        (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                        (x.HistoryFristTime == null || d.CreationTime > x.HistoryFristTime) &&
                        (x.HistoryLastTime == null || d.CreationTime < x.HistoryLastTime) &&
                        (x.ClearTime == null || d.CreationTime > x.ClearTime))
                        .Count(d => !d.IsPrivate),
                    PrivateBadge = context.Message.Where(d =>
                        x.SessionId == d.SessionId &&
                        d.Id > minMessageId &&
                        d.SenderId != x.OwnerId &&
                        (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                        (x.HistoryFristTime == null || d.CreationTime > x.HistoryFristTime) &&
                        (x.HistoryLastTime == null || d.CreationTime < x.HistoryLastTime) &&
                        (x.ClearTime == null || d.CreationTime > x.ClearTime))
                        .Count(d => d.IsPrivate && d.ReceiverId == x.OwnerId),
                    FollowingCount = context.Message.Where(d =>
                        x.SessionId == d.SessionId &&
                        d.Id > minMessageId &&
                        d.SenderId != x.OwnerId &&
                        (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                        (x.HistoryFristTime == null || d.CreationTime > x.HistoryFristTime) &&
                        (x.HistoryLastTime == null || d.CreationTime < x.HistoryLastTime) &&
                        (x.ClearTime == null || d.CreationTime > x.ClearTime))
                        .Count(d => x.OwnerFollowList.Any(d => d.DestinationId == x.Id)),
                    RemindAllCount = context.Message.Where(d =>
                        x.SessionId == d.SessionId &&
                        d.Id > minMessageId &&
                        d.SenderId != x.OwnerId &&
                        (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                        (x.HistoryFristTime == null || d.CreationTime > x.HistoryFristTime) &&
                        (x.HistoryLastTime == null || d.CreationTime < x.HistoryLastTime) &&
                        (x.ClearTime == null || d.CreationTime > x.ClearTime)).Count(d => d.IsRemindAll && !d.IsRollbacked),
                    RemindMeCount = context.Message.Where(d =>
                        x.SessionId == d.SessionId &&
                        d.Id > minMessageId &&
                        d.SenderId != x.OwnerId &&
                        (x.ReadedMessageId == null || d.Id > x.ReadedMessageId) &&
                        (x.HistoryFristTime == null || d.CreationTime > x.HistoryFristTime) &&
                        (x.HistoryLastTime == null || d.CreationTime < x.HistoryLastTime) &&
                        (x.ClearTime == null || d.CreationTime > x.ClearTime))
                        .Count(d => d.MessageReminderList.Any(g => g.SessionUnitId == x.Id))
                })
                .ToDictionary(x => x.Id)
                ;
        }
    }
}
