using IczpNet.AbpTrees;

namespace IczpNet.Chat.SessionOrganizations;

public interface ISessionOrganizationManager : ITreeManager<SessionOrganization, long, SessionOrganizationInfo>
{
}
