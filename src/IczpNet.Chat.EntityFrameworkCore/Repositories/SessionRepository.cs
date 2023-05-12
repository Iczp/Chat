using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.Sessions;
using System;

namespace IczpNet.Chat.Repositories
{
    public class SessionRepository : ChatRepositoryBase<Session, Guid>, ISessionRepository
    {
        public SessionRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}
