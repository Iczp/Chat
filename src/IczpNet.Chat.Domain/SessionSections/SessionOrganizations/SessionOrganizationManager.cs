using IczpNet.AbpTrees;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.SessionOrganizations
{
    public class SessionOrganizationManager : TreeManager<SessionOrganization, long, SessionOrganizationInfo>, ISessionOrganizationManager
    {
        public SessionOrganizationManager(IRepository<SessionOrganization, long> repository) : base(repository)
        {
        }
    }
}
