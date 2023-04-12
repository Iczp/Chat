using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories
{
    public class SessionPermissionDefinitionRepository : ChatRepositoryBase<SessionPermissionDefinition, string>, ISessionPermissionDefinitionRepository
    {
        public SessionPermissionDefinitionRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}
