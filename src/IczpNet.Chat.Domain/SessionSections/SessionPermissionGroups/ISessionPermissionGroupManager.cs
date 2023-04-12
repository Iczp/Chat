using IczpNet.AbpTrees;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;

namespace IczpNet.Chat.SessionSections.SessionOrganizations
{
    public interface ISessionPermissionGroupManager : ITreeManager<SessionPermissionGroup, long, SessionPermissionGroupInfo>
    {
    }
}
