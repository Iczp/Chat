using IczpNet.AbpTrees;

namespace IczpNet.Chat.SessionPermissionGroups;

public interface ISessionPermissionGroupManager : ITreeManager<SessionPermissionGroup, long, SessionPermissionGroupInfo>
{
}
