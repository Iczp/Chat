using IczpNet.AbpTrees;

namespace IczpNet.Chat.SessionSections.SessionPermissionGroups;

public interface ISessionPermissionGroupManager : ITreeManager<SessionPermissionGroup, long, SessionPermissionGroupInfo>
{
}
