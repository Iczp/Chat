using IczpNet.AbpTrees;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups
{
    public class SessionPermissionGroupManager : TreeManager<SessionPermissionGroup, long, SessionPermissionGroupInfo>, ISessionPermissionGroupManager
    {
        public SessionPermissionGroupManager(IRepository<SessionPermissionGroup, long> repository) : base(repository)
        {
        }
    }
}

