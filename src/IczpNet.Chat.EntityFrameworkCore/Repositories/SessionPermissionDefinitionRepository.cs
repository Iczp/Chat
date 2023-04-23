using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.Repositories
{
    public class SessionPermissionDefinitionRepository : ChatRepositoryBase<SessionPermissionDefinition, string>, ISessionPermissionDefinitionRepository
    {
        public SessionPermissionDefinitionRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public virtual async Task<int> BatchUpdateIsEnabledAsync(bool isEnabled)
        {
            var context = await GetDbContextAsync();

            return await context.SessionPermissionDefinition
               .Where(x => !x.IsDeleted)
               .ExecuteUpdateAsync(s => s
                   .SetProperty(b => b.IsEnabled, b => isEnabled)
               );
        }
    }
}
