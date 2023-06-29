using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

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
                    .SetProperty(b => b.LastModificationTime, b => DateTime.Now)
                    .SetProperty(b => b.LastMessageId, b => lastMessageId)
                );
        }
    }
}
