using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories
{
    public class SessionUnitCounterRepository : EfCoreRepository<ChatDbContext, SessionUnitCounter>, ISessionUnitCounterRepository
    {
        public SessionUnitCounterRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}
