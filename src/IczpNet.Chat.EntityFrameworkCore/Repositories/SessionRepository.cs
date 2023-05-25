using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories
{
    public class SessionRepository : ChatRepositoryBase<Session, Guid>, ISessionRepository
    {
        public SessionRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public virtual async Task<int> UpdateLastMessageIdAsync(Guid sessionId, long lastMessageId)
        {
            var context = await GetDbContextAsync();

            return await context.Session
                .Where(x => x.Id == sessionId)
                .Where(x => x.LastMessageId == null || x.LastMessageId.Value < lastMessageId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.LastMessageId, b => lastMessageId)
                );
        }
    }
}
